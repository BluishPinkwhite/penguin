using Godot;
using Incremental.scripts.director;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.station;
using Incremental.scripts.planet.data;
using Incremental.scripts.planet.rendering;

namespace Incremental.scripts.entity.pawn.roles;

public partial class PawnMiner : Pawn
{
    private PlanetTile _targetTile;
    private Vector2I _targetCoords;

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
            else
            {
                if (_targetTile != null && !_targetTile.IsEmpty())
                {
                    _targetTile.Integrity -= d * 0.25f / _targetTile.Material.BreakTime();

                    if (_targetTile.Integrity < 0)
                    {
                        Item item = _targetTile.Destroy();

                        // TODO fix :(
                        PlanetRenderer.isLightDirty = true;
                        // Game.I._data.PropagateLight(Mathf.FloorToInt(gravityY),
                            // Mathf.FloorToInt(PolarPos.X), PlanetTile.LightMax);

                        if (item != Item.None)
                            Pickup.Instantiate(PolarPos, item);

                        Counter++;

                        if (Counter >= 5)
                        {
                            Counter = 0;
                            State = PawnState.ReturnH;
                            Target = new Vector2(ResourceStation.I.Surface.X, ResourceStation.I.Surface.Y);
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
        else if (State == PawnState.DropOff)
        {
            State = PawnState.Idle;
            SetCooldown(1);
        }
    }

    private bool GetNewMiningTarget()
    {
        if (Game.I._data.NextMiningTarget(ID, ResourceStation.I.Surface, out Vector2 target, out PlanetTile tile))
        {
            _targetCoords = new Vector2I(Mathf.FloorToInt(target.X), Mathf.FloorToInt(target.Y));
            _targetTile = tile;
            _targetTile.OwnerID = ID;
            return true;
        }

        return false;
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