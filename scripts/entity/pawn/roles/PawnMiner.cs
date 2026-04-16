using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.station;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.entity.pawn.roles;

public partial class PawnMiner : Pawn
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

        if (State is PawnState.Idle or PawnState.GiveUp)
        {
            if (State == PawnState.GiveUp)
            {
                visual.Rotate(Game.RandomAround(0.28f, 0.1f));
            }
            
            // wait till the pawn is on ground
            PlanetTile below = Game.I._data.GetTileAtPolarCoords(PolarPos.X, gravityY);
            if (below != null && !below.IsEmpty() && GetNewMiningTarget())
            {
                if (State == PawnState.Idle)
                {
                    Target = new Vector2(_targetCoords.X + 0.5f, _targetCoords.Y + 1.25f);
                    State = PawnState.Move;
                    SetCooldown(1);
                }
                else
                {
                    if (_targetTile != null && _targetTile.OwnerID == ID)
                        _targetTile.OwnerID = -1;
                    
                    BreakTile(below, Mathf.FloorToInt(PolarPos.X), Mathf.FloorToInt(gravityY));
                    Retire();
                }
            }
        }
        else if (State is PawnState.Move or PawnState.ReturnH or PawnState.RetireH)
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
                else if (State == PawnState.RetireH)
                {
                    State = PawnState.RetireV;
                    SetCooldown(0.35f);
                }
            }
        }
        else if (State is PawnState.ReturnV or PawnState.RetireV)
        {
            if (FlyToTarget(d))
            {
                if (State == PawnState.ReturnV)
                {
                    State = PawnState.DropOff;
                    SetCooldown(2.5f);
                }
                else
                {
                    State = PawnState.GiveUp;
                    SetCooldown(0.25f);
                }
            }
        }
        else if (State == PawnState.Action)
        {
            // find a new tile when this tile was broken by someone else
            if (Mathf.FloorToInt(Target.X) != Mathf.FloorToInt(PolarPos.X) ||
                Mathf.FloorToInt(Target.Y) != Mathf.FloorToInt(PolarPos.Y) || _targetTile?.OwnerID != ID)
            {
                State = PawnState.Idle;

                if (_targetTile?.OwnerID == ID)
                    _targetTile.OwnerID = -1;
            }
        }
        else if (State == PawnState.DropOff)
        {
            if (Counter > Consts.Pawns[Role.Miner].RetirementCycles)
            {
                State = PawnState.RetireH;
                
                bool hasTarget = _targetTile != null;
                
                float x = hasTarget ? Game.RandomAround(_targetCoords.X, 5) : Game.RandomAround(PolarPos.X, 5);
                float y = Game.RandomAround(ResourceStation.I.Surface.Y + 50, 5);

                Target = new Vector2(x, y);
                
                if (_targetTile != null && _targetTile.OwnerID == ID)
                    _targetTile.OwnerID = -1;
            }
            else {
                State = PawnState.Idle;
                SetCooldown(1);
            }
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
                if (_targetTile != null && !_targetTile.IsEmpty() && _targetTile.OwnerID == ID)
                {
                    _targetTile.Integrity -= 0.25f / _targetTile.Material.BreakTime();
                    
                    SFX.PitchScale = (float)GD.RandRange(0.8f, 1.1f);
                    SFX.VolumeDb = (float)GD.RandRange(-5f, 1f);
                    SFX.Play();

                    if (_targetTile.Integrity <= 0)
                    {
                        BreakTile(_targetTile, _targetCoords.X, _targetCoords.Y);
                        Counter++;

                        if (Counter % 5 == 0)
                        {
                            State = PawnState.ReturnH;
                            Target = ResourceStation.I.GetParent().GetChild<OrbitEntity>(1).PolarPos;
                            SetCooldown(1);
                        }
                        else
                        {
                            State = PawnState.Idle;
                            SetCooldown(2.5f);
                        }
                    }
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