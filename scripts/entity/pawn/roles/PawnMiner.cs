using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.director.data.recipe;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.station;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.entity.pawn.roles;

public partial class PawnMiner : Pawn
{
    protected PlanetTile _targetTile;
    
    [Export] protected AudioStreamPlayer2D SFX;

    public override void _Ready()
    {
        base._Ready();
        visual.FrameChanged += OnFrameChanged;
    }

    protected override void DoBehaviour(float d)
    {
        int polarX = Mathf.FloorToInt(PolarPos.X);
        int polarY = Mathf.FloorToInt(PolarPos.Y);

        if (State is PawnState.Idle or PawnState.GiveUp)
        {
            if (State == PawnState.GiveUp)
            {
                visual.Rotate(Game.RandomAround(0.28f, 0.1f));
            }

            // mined tile was broken by someone else, find a new one
            if (_targetTile != null && (_targetTile.IsEmpty() || !_targetTile.IsOwnedBy(this)))
                _targetTile = null;
            
            // find a new tile to mine when on ground
            if (onGround && _targetTile == null && GetNewMiningTarget() && _targetTile != null)
            {
                if (State == PawnState.Idle)
                {
                    Game.I._data.GetTileAbove(_targetTile.PolarX, _targetTile.PolarY, out bool isSplit);
                    Target = new Vector2((isSplit ? (int)(_targetTile.PolarX * 2) : _targetTile.PolarX) + 0.5f, 
                        _targetTile.PolarY + 0.75f);
                    
                    State = PawnState.Move;
                    SetCooldown(1);
                }
                else
                {
                    // break by impact
                    if (_targetTile != null && _targetTile.IsOwnedBy(this))
                        _targetTile.RemoveOwner(this);
                    
                    PlanetTile below = Game.I._data.GetTileBelow(polarX, polarY, out _);
                    Game.I.Pawns.RecordDamage(Role.Unemployed, below.Integrity);
                    BreakTile(below);
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
            if (_targetTile == null)
            {
                State = PawnState.Idle;
                SetCooldown(1);
            }
            
            // find a new tile when this tile was broken by someone else
            else if (_targetTile != null && !_targetTile.IsOwnedBy(this))
            {
                State = PawnState.Idle;
                _targetTile = null;
                SetCooldown(2);
            }
        }
        else if (State == PawnState.DropOff)
        {
            if (Counter > Consts.Pawns[Role].RetirementCycles)
            {
                State = PawnState.RetireH;
                
                bool hasTarget = _targetTile != null;
                
                float x = hasTarget ? Game.RandomAround(_targetTile.PolarX, 5) : Game.RandomAround(PolarPos.X, 5);
                float y = Game.RandomAround(ResourceStation.I.Surface.Y + 50, 5);

                Target = new Vector2(x, y);
                
                if (_targetTile != null && _targetTile.IsOwnedBy(this))
                    _targetTile.RemoveOwner(this);
            }
            else {
                State = PawnState.Idle;
                SetCooldown(1);
            }
        }
    }

    private bool GetNewMiningTarget()
    {
        if (Game.I._data.NextMiningTarget(this, out PlanetTile tile))
        {
            _targetTile = tile;
            _targetTile.AddOwner(this);
            return true;
        }

        return false;
    }
    
    
    private void OnFrameChanged()
    {
        if (visual.Animation != "mine" || visual.Frame != 4) return;
        if (_targetTile == null || _targetTile.IsEmpty() || !_targetTile.IsOwnedBy(this)) return;

        if (_targetTile.Material == TileMaterial.Core)
        {
            _targetTile = null;
            State = PawnState.Idle;
            return;
        }

        float damage = Inventory.IsResearchUnlocked(RecipeID.Research_BasaltUpgrade) ? 0.5f : 0.25f;
        damage += Inventory.Items[Item.Tougher_Pickaxes].Amount * 0.2f;
                    
        if (Inventory.IsResearchUnlocked(RecipeID.Research_MagmaReinforcement))
        {
            Game.I.Pawns.RecordDamage(Role, _targetTile.Integrity);
            _targetTile.Integrity = 0;
        }

        damage /= _targetTile.Material.BreakTime();
        _targetTile.Integrity -= damage;
        Game.I.Pawns.RecordDamage(Role, damage);

        if (Inventory.IsResearchUnlocked(RecipeID.Research_PrecisePickaxes))
        {
            float chance = 0.15f;
            chance += Inventory.Items[Item.Higher_Crit_Chance].Amount * 0.1f;
                        
            if (GD.Randf() < chance)
            {
                _targetTile.Integrity -= damage;
                Game.I.Pawns.RecordDamage(Role, damage);
            }
                        
            SFX.PitchScale = (float)GD.RandRange(0.4f, 0.7f);
            SFX.VolumeDb = (float)GD.RandRange(-1f, 3f);
            SFX.Play();
        }
        else
        {
            SFX.PitchScale = (float)GD.RandRange(0.8f, 1.1f);
            SFX.VolumeDb = (float)GD.RandRange(-5f, 1f);
            SFX.Play();
        }
                    

        if (_targetTile.Integrity <= 0)
        {
            BreakTile(_targetTile);
            Counter++;
                        
            int counterPeriod = Inventory.IsResearchUnlocked(RecipeID.Research_EnergyDrinks) ? 8 : 5;

            if (Counter % counterPeriod == 0)
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