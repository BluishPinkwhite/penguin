using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.station;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.entity.pawn.roles;

public partial class PawnArcheologist : Pawn
{
    private PlanetTile _targetTile;
    private Vector2I _targetCoords;
    
    [Export] private AudioStreamPlayer2D SFX;

    public override void _Ready()
    {
        base._Ready();
        visual.FrameChanged += OnFrameChanged;
    }

    protected override void DoBehaviour(float d)
    {
        float gravityY = PolarPos.Y - d * Gravity;

        if (State == PawnState.Idle)
        {
            // wait till the pawn is on ground
            PlanetTile below = Game.I._data.GetTileAtPolarCoords(PolarPos.X, gravityY);
            if (below != null && !below.IsEmpty() && GetNewMiningTarget())
            {
                Target = new Vector2(_targetCoords.X + 0.5f, _targetCoords.Y + 1.25f);
                State = PawnState.Move;
                SetCooldown(1);
            }
        }
        else if (State is PawnState.Move or PawnState.ReturnH)
        {
            if (WalkToTarget(d))
            {
                if (State == PawnState.Move)
                {
                    State = PawnState.Action;
                    SetCooldown(1);
                }
                else if (State == PawnState.ReturnH)
                {
                    State = PawnState.ReturnV;
                    Target = ResourceStation.I.PolarPos;
                    SetCooldown(0.35f);
                }
            }
        }
        else if (State == PawnState.ReturnV)
        {
            if (FlyToTarget(d))
            {
                State = PawnState.DropOff;
                SetCooldown(2.5f);
            }
        }
        else if (State == PawnState.Action)
        {
            // find a new tile when this tile was broken by someone else
            if (Mathf.FloorToInt(Target.X) != Mathf.FloorToInt(PolarPos.X) ||
                Mathf.FloorToInt(Target.Y) != Mathf.FloorToInt(PolarPos.Y))
            {
                State = PawnState.Idle;

                if (_targetTile.OwnerID == ID)
                    _targetTile.OwnerID = -1;
            }
        }
        else if (State == PawnState.DropOff)
        {
            State = PawnState.Idle;
            SetCooldown(1);
        }
    }

    private bool GetNewMiningTarget()
    {
        if (Game.I._data.NextMiningTarget(ResourceStation.I.Surface, out Vector2 target, out PlanetTile tile))
        {
            _targetCoords = new Vector2I(Mathf.FloorToInt(target.X), Mathf.FloorToInt(target.Y));
            _targetTile = tile;
            _targetTile.OwnerID = ID;
            return true;
        }

        return false;
    }
    
    
    private void OnFrameChanged()
    {
        if (visual.Animation == "mine")
        {
            if (visual.Frame == 4)
            {
                if (_targetTile != null && !_targetTile.IsEmpty())
                {
                    _targetTile.Integrity -= 0.02f / _targetTile.Material.BreakTime();
                    
                    SFX.PitchScale = (float)GD.RandRange(1.8f, 2.4f);
                    SFX.VolumeDb = (float)GD.RandRange(-8f, -4f);
                    SFX.Play();

                    if (_targetTile.Integrity <= 0)
                    {
                        RecipeID recipe = _targetTile.Destroy();

                        Game.I._data.PropagateLight(_targetCoords.Y,
                            _targetCoords.X, PlanetTile.LightMax);
                        
                        if (recipe != RecipeID.None)
                        {
                            foreach ((Item item, int amount) tuple in Inventory.TryGetRecipe(recipe))
                            {
                                for (int i = 0; i < tuple.amount; i++)
                                {
                                    Pickup.Instantiate(PolarPos, tuple.item);
                                }
                            }
                        }

                        Counter++;

                        State = PawnState.ReturnH;
                        Target = new Vector2(ResourceStation.I.Surface.X, ResourceStation.I.Surface.Y);
                    }
                    SetCooldown(1);
                }
            }
        }
    }

    protected override void UpdateAnimationState()
    {
        if (_isOnCooldown || State == PawnState.Idle)
        {
            visual.Animation = "idle";
        }
        else if (State is PawnState.Move or PawnState.ReturnH)
        {
            visual.Animation = Flying ? "fly" : "walk";
        }
        else if (State == PawnState.ReturnV)
        {
            visual.Animation = "fly";
        }
        else if (State == PawnState.Action)
        {
            visual.Animation = "mine";
        }
    }
}