using Godot;
using Incremental.scripts.debug;
using Incremental.scripts.director;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.entity.pawn;

public partial class Pawn : SurfaceEntity
{
    private PawnState _state = PawnState.Idle;

    [Export] private SpriteFrames _spriteFramesMiner;
    [Export] private SpriteFrames _spriteFramesHauler;
    [Export] private AnimatedSprite2D visual;

    public PawnState State
    {
        get => _state;
        set
        {
            _state = value;
            UpdateAnimationState(_role, _state, _flying, _isOnCooldown);
        }
    }

    private bool _flying = false;

    public bool Flying
    {
        get => _flying;
        set
        {
            _flying = value;
            UpdateAnimationState(_role, _state, _flying, _isOnCooldown);
        }
    }

    private Role _role = Role.Unemployed;

    public Role Role
    {
        get => _role;
        set
        {
            _role = value;
            UpdateAnimationState(_role, _state, _flying, _isOnCooldown);
        }
    }

    private bool _isOnCooldown = false;


    public float Cooldown = 0;
    public int Counter = 0;

    public Item InventoryID = Item.None;
    public int InventoryCount = 0;

    [Export] public Label DebugText;


    public const float PawnAngularWidth = 1.45f;
    public const float PawnHeight = PawnAngularWidth;
    public const float WalkSpeed = 6.5f;
    public const float FlySpeed = Gravity * 1.2f;

    public int ID { private set; get; }
    private static int _nextID = 0;


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
            // Cooldown = 0;

            if (!_isOnCooldown)
            {
                _isOnCooldown = true;
                UpdateAnimationState(Role, State, Flying, true);
            }
        }
        else
        {
            if (_isOnCooldown)
            {
                _isOnCooldown = false;
                UpdateAnimationState(Role, State, Flying, false);
            }

            switch (Role)
            {
                case Role.Miner:
                    this.DoBehaviourMiner(d);
                    break;
                case Role.Hauler:
                    this.DoBehaviourHauler(d);
                    break;
                default:
                    QueueFree();
                    break;
            }
        }

        Game.I.Debug.SetLine(ID, Position, Game.I._data.PolarToWorld(Target),
            DebugDraw.GetColor((int)Role));

        DoLayerChecks();
        ApplyPolarTransform();
    }

    public new float GetHalfWidthTiles(int layer)
    {
        int size = Game.I._data.GetLayerSize(layer);
        return PawnAngularWidth / size;
    }


    public void SetCooldown(float value)
    {
        Cooldown = Game.RandomAround(value, value * 0.25f);
    }

    public bool WalkToTarget(float d)
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

    public bool FlyToTarget(float d)
    {
        float dy = Target.Y - PolarPos.Y;
        if (dy * dy > 0.1f || Mathf.FloorToInt(Target.Y) != Mathf.FloorToInt(PolarPos.Y))
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

    public void UpdateAnimationState(Role role, PawnState state, bool flying, bool onCooldown)
    {
        float dir = Target.X - PolarPos.X;

        if (dir > 0)
        {
            visual.FlipH = false;
        }
        else if (dir < 0)
        {
            visual.FlipH = true;
        }
        
        if (role == Role.Miner)
        {
            visual.SetSpriteFrames(_spriteFramesMiner);

            if (onCooldown || state == PawnState.Idle)
            {
                visual.Animation = "idle";
            }
            else if (state == PawnState.Move || state == PawnState.ReturnH)
            {
                if (flying)
                {
                    visual.Animation = "fly";
                }
                else
                {
                    visual.Animation = "walk";
                }
            }
            else if (state == PawnState.ReturnV)
            {
                visual.Animation = "fly";
            }
            else if (state == PawnState.Action)
            {
                visual.Animation = "mine";
            }
        }
        else if (role == Role.Hauler)
        {
            visual.SetSpriteFrames(_spriteFramesHauler);
            if (onCooldown || state == PawnState.Idle)
            {
                visual.Animation = "idle";
            }
            else if (state == PawnState.Move || state == PawnState.ReturnH)
            {
                if (flying)
                {
                    visual.Animation = "fly";
                }
                else
                {
                    visual.Animation = "walk";
                }
            }
            else if (state == PawnState.ReturnV)
            {
                visual.Animation = "fly";
            }
        }

        visual.Play();
    }
}