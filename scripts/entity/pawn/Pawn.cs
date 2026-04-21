using Godot;
using Incremental.scripts.debug;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.director.data.recipe;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.pawn.roles;
using Incremental.scripts.planet.data;

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

    protected const float PawnAngularWidth = 1.45f;

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
        Target = PolarPos;
        SetCooldown(2);

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

        if (Inventory.IsResearchUnlocked(RecipeID.Research_ErgonomicHandles))
            Cooldown *= 0.7f;
    }

    protected bool WalkToTarget(float d)
    {
        int targetLayer = Mathf.FloorToInt(Target.Y);
        int targetSize = Game.I._data.GetLayerSize(targetLayer);
        float targetX = Target.X * _currSize / targetSize;
        targetX = Mathf.PosMod(targetX, _currSize);

        float dx = CircularDelta(PolarPos.X, targetX, _currSize);

        float mult = Consts.Pawns[Role].WalkSpeed;
        if (Inventory.IsResearchUnlocked(RecipeID.Research_Running))
            mult *= 1.35f;
        
        float stepX = Mathf.Clamp(dx, -1f, 1f) * d * mult;
        
        float newX = PolarPos.X + stepX;


        bool reachedTarget = false;
        if (!Flying)
        {
            // horizontal movement
            if (dx * dx > 0.05f)
            {
                if (!CheckCollision(newX, PolarPos.Y))
                    PolarPos.X = newX;
                else
                {
                    Flying = true;

                    if (Inventory.IsResearchUnlocked(RecipeID.Research_JetpackShoes))
                    {
                        float stepY = Consts.Pawns[Role].FlySpeed * 1.5f * Gravity * d;
                        float newY = PolarPos.Y + stepY;
                        
                        PolarPos.Y = newY;
                    }
                }
            }
            else
                reachedTarget = true;
        }
        else
        {
            // vertical movement
            mult = Consts.Pawns[Role].FlySpeed;
            if (Inventory.IsResearchUnlocked(RecipeID.Research_FasterJetpackAscent))
                mult *= 1.4f;
            
            float stepY = mult * Gravity * d;
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
            float stepY = Mathf.Clamp(dy * 5, -1f, 1f) * d * Consts.Pawns[Role].FlySpeed * Gravity;
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
            visual.Scale = visual.Scale with { X = 1 };
        else if (dir < 0)
            visual.Scale = visual.Scale with { X = -1 };
        
        UpdateAnimationState();

        visual.Play();
    }

    protected void Retire()
    {
        SetVisible(false);
        ItemRecipe.TryApplyRecipe(RecipeID.Penguin_Retirement);
        Game.I.Debug.RemoveLine(ID);
        QueueFree();
    }
    
    protected void BreakTile(PlanetTile planetTile, int tile, int layer)
    {
        RecipeID recipe = planetTile.Destroy();

        Game.I._data.PropagateLight(layer, tile, PlanetTile.LightMax);
                        
        if (recipe != RecipeID.None)
        {
            foreach ((Item item, int amount) tuple in ItemRecipe.TryGetOutput(recipe))
            {
                for (int i = 0; i < tuple.amount; i++)
                {
                    Pickup.Instantiate(PolarPos, tuple.item);
                }
            }
        }
    }

    protected abstract void DoBehaviour(float d);
    protected abstract void UpdateAnimationState();
}