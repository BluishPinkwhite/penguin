using Godot;
using Incremental.scripts.director;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.station;
using Incremental.scripts.planet.data;
using Incremental.scripts.planet.rendering;

namespace Incremental.scripts.entity.pawn.roles;

public static class PawnMiner
{
    public static void DoBehaviourMiner(this Pawn pawn, float d)
    {
        float gravityY = pawn.PolarPos.Y - d * SurfaceEntity.Gravity;
        
        if (pawn.State == PawnState.Idle)
        {
            PlanetTile below = Game.I._data.GetTileAtPolarCoords(pawn.PolarPos.X, gravityY);
            if (below != null && !below.IsEmpty())
            {
                GetNewMiningTarget(pawn);
            }
        }
        else if (pawn.State is PawnState.Move or PawnState.ReturnH)
        {
            if (pawn.WalkToTarget(d))
            {
                if (pawn.State == PawnState.Move)
                {
                    pawn.State = PawnState.Action;
                    pawn.SetCooldown(1);
                }
                else if (pawn.State == PawnState.ReturnH)
                {
                    pawn.State = PawnState.ReturnV;
                    pawn.Target = ResourceStation.I.PolarPos;
                    pawn.SetCooldown(0.35f);
                }
            }
        }
        else if (pawn.State == PawnState.ReturnV)
        {
            if (pawn.FlyToTarget(d))
            {
                pawn.State = PawnState.DropOff;
                pawn.SetCooldown(2.5f);
            }
        }
        else if (pawn.State == PawnState.Action)
        {
            // find a new tile when this tile was broken by someone else
            if (Mathf.FloorToInt(pawn.Target.X) != Mathf.FloorToInt(pawn.PolarPos.X) ||
                Mathf.FloorToInt(pawn.Target.Y) != Mathf.FloorToInt(pawn.PolarPos.Y))
            {
                pawn.State = PawnState.Idle;
                Game.I._data.Layers[Mathf.FloorToInt(pawn.Target.Y - 1.25f)][Mathf.FloorToInt(pawn.Target.X)].OwnerID = -1;
            }
            else
            {
                PlanetTile below = pawn.GetTileBelow(gravityY);

                if (below != null && !below.IsEmpty())
                {
                    below.OwnerID = pawn.ID;
                    below.Integrity -= d * 0.25f / below.Material.BreakTime();

                    if (below.Integrity < 0)
                    {
                        Item item = below.Destroy();

                        Game.I._data.PropagateLight(Mathf.FloorToInt(gravityY), 
                            Mathf.FloorToInt(pawn.PolarPos.X), PlanetTile.LightMax);
                        
                        if (item != Item.None)
                            Pickup.Instantiate(pawn.PolarPos, item);

                        pawn.Counter++;

                        if (pawn.Counter >= 5)
                        {
                            pawn.Counter = 0;
                            pawn.State = PawnState.ReturnH;
                            pawn.Target = new Vector2(ResourceStation.I.Surface.X, ResourceStation.I.Surface.Y);
                            pawn.SetCooldown(1);
                        }
                        else
                        {
                            pawn.State = PawnState.Idle;
                            pawn.SetCooldown(2.5f);
                        }
                    }
                }
            }
        }
        else if (pawn.State == PawnState.DropOff)
        {
            pawn.State = PawnState.Idle;
            pawn.SetCooldown(1);
        }
    }

    private static void GetNewMiningTarget(Pawn pawn)
    {
        if (Game.I._data.NextMiningTarget(pawn.ID, ResourceStation.I.Surface, out Vector2 target))
        {
            pawn.Target = new Vector2(target.X + 0.5f, target.Y + 1.25f);
            pawn.State = PawnState.Move;
            pawn.SetCooldown(1);
        }
    }
}