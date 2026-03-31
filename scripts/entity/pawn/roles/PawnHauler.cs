using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.station;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.entity.pawn.roles;

public static class PawnHauler
{
    public static void DoBehaviourHauler(this Pawn pawn, float d)
    {
        float gravityY = pawn.PolarPos.Y - d * SurfaceEntity.Gravity;

        if (pawn.State == PawnState.Idle)
        {
            PlanetTile below = Game.I._data.GetTileAtPolarCoords(pawn.PolarPos.X, gravityY);
            if (below != null && !below.Destroyed)
            {
                GetNewPickupTarget(pawn);
                pawn.State = PawnState.Move;
            }
            else
            {
                pawn.SetCooldown(2);
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
            Pickup pickup = GetNewPickupTarget(pawn);
            if (pickup != null && pickup.PolarPos.DistanceSquaredTo(pawn.PolarPos) < 0.2f)
            {
                pawn.InventoryID = pickup.Item;
                pawn.InventoryCount += 1;
                pickup.QueueFree();
                
                pawn.State = PawnState.ReturnH;
                pawn.Target = new Vector2(ResourceStation.I.Below.X, ResourceStation.I.Below.Y);
            }
            else if (pickup != null)
            {
                pawn.Target = pickup.PolarPos;
                pawn.State = PawnState.Move;
            }
            else
            {
                pawn.State = PawnState.Idle;
            }

            pawn.SetCooldown(1);
        }
        else if (pawn.State == PawnState.DropOff)
        {
            if (pawn.InventoryID != Item.None)
            {
                if (!Inventory.Items.TryAdd(pawn.InventoryID, pawn.InventoryCount))
                {
                    Inventory.Items[pawn.InventoryID] += pawn.InventoryCount;
                }
                Resources.I.UpdateVisuals();

                pawn.InventoryID = Item.None;
                pawn.InventoryCount = 0;
            }
            
            pawn.State = PawnState.Idle;
            pawn.SetCooldown(1);
        }
    }

    private static Pickup GetNewPickupTarget(Pawn pawn)
    {
        Pickup closest = null;
        float minDist = Single.PositiveInfinity;

        foreach (Node node in Game.I.Pickups.GetChildren())
        {
            Pickup child = (Pickup)node;

            float dist = child.PolarPos.DistanceSquaredTo(pawn.PolarPos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = child;
            }
        }

        if (closest != null)
            pawn.Target = closest.PolarPos;
        return closest;
    }
}