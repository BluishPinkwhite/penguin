using Godot;
using Incremental.scripts.debug;
using Incremental.scripts.director;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.entity.pawn;

public abstract partial class Pawn : SurfaceEntity
{
    private PawnState _state = PawnState.Idle;

    [Export] protected AnimatedSprite2D visual;
    [Export] public Label DebugText;

    private bool _flying = false;
    private bool _animationDirty = false;
    private Role _role = Role.Unemployed;
    protected bool _isOnCooldown = false;
    
    protected float Cooldown = 0;
    protected int Counter = 0;

    public Item InventoryID = Item.None;
    public int InventoryCount = 0;

    protected const float PawnAngularWidth = 1.45f;
    protected const float WalkSpeed = 6.5f;
    protected const float FlySpeed = Gravity * 1.2f;

    private static int _nextID = 0;


    public int ID { private set; get; }

    public Role Role
    {
        get => _role;
        set
        {
            _role = value;
            _animationDirty = true;
        }
    }

    public PawnState State
    {
        get => _state;
        set
        {
            _state = value;
            _animationDirty = true;
        }
    }

    public bool Flying
    {
        get => _flying;
        set
        {
            _flying = value;
            _animationDirty = true;
        }
    }

    /////////////////////////


    public override void _Ready()
    {
        ID = _nextID++;

        PolarPos = new Vector2(
            (int)(Game.RandomTo(Game.I._data.GetLayerSize(Game.I._data.Layers.Count)) / 8),
            Game.I._data.Layers.Count + 2);
        Target = PolarPos;

        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        float d = (float)delta;

        // debug
        DebugText.Text = Cooldown > 0 ? $"<{Cooldown:F1}>" : $"[{State}]";

        if ((int)State > 0)
            ApplyGravity(d);

        // behaviour
        if (Cooldown > 0)
        {
            Cooldown -= d;

            if (!_isOnCooldown)
            {
                _isOnCooldown = true;
                _animationDirty = true;
            }
        }
        else
        {
            if (_isOnCooldown)
            {
                _isOnCooldown = false;
                _animationDirty = true;
            }
            
            DoBehaviour(d);
        }

        Game.I.Debug.SetLine(ID, Position, Game.I._data.PolarToWorld(Target),
            DebugDraw.GetColor((int)Role));

        DoLayerChecks();
        ApplyPolarTransform();

        if (_animationDirty)
        {
            _animationDirty = false;
            LoadAnimationState();
        }
    }

    private new float GetHalfWidthTiles(int layer)
    {
        int size = Game.I._data.GetLayerSize(layer);
        return PawnAngularWidth / size;
    }


    public void SetCooldown(float value)
    {
        Cooldown = Game.RandomAround(value, value * 0.25f);
    }

    protected bool WalkToTarget(float d)
    {
        int targetLayer = Mathf.FloorToInt(Target.Y);
        int targetSize = Game.I._data.GetLayerSize(targetLayer);
        float targetX = Target.X * _currSize / targetSize;
        targetX = Mathf.PosMod(targetX, _currSize);

        float dx = CircularDelta(PolarPos.X, targetX, _currSize);

        float stepX = Mathf.Clamp(dx, -1f, 1f) * d * WalkSpeed;
        float newX = PolarPos.X + stepX;


        bool reachedTarget = false;
        if (!Flying)
        {
            // horizontal movement
            if (dx * dx > 0.05f || Mathf.FloorToInt(Target.X) != Mathf.FloorToInt(PolarPos.X + 0.45f))
            {
                if (!CheckCollision(newX, PolarPos.Y))
                    PolarPos.X = newX;
                else
                    Flying = true;
            }
            else
                reachedTarget = true;
        }
        else
        {
            // vertical movement
            float stepY = FlySpeed * d;
            float newY = PolarPos.Y + stepY;

            if (!CheckCollision(PolarPos.X, newY))
                PolarPos.Y = newY;

            if (!CheckCollision(newX, PolarPos.Y))
            {
                PolarPos.X = newX;
                Flying = false;
            }
        }

        PolarPos.X = Mathf.PosMod(PolarPos.X, _currSize);

        return reachedTarget;
    }

    protected bool FlyToTarget(float d)
    {
        float dy = Target.Y - PolarPos.Y;
        if (dy * dy > 0.05f)
        {
            // vertical movement
            float stepY = Mathf.Clamp(dy * 5, -1f, 1f) * d * FlySpeed;
            float newY = PolarPos.Y + stepY;

            if (!CheckCollision(PolarPos.X, newY))
                PolarPos.Y = newY;

            return false;
        }

        return true;
    }

    public void LoadAnimationState()
    {
        float dir = CircularDelta(PolarPos.X, Target.X, _currSize);

        if (dir > 0)
            visual.FlipH = false;
        else if (dir < 0)
            visual.FlipH = true;
        
        UpdateAnimationState();

        visual.Play();
    }

    protected abstract void DoBehaviour(float d);
    protected abstract void UpdateAnimationState();
}