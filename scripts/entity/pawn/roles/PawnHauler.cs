using System;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.station;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.entity.pawn.roles;

public partial class PawnHauler : Pawn
{
    protected override void DoBehaviour(float d)
    {
        float gravityY = PolarPos.Y - d * Gravity;

        if (State == PawnState.Idle)
        {
            // wait till the pawn is on ground
            PlanetTile below = Game.I._data.GetTileAtPolarCoords(PolarPos.X, gravityY);
            if (below != null && !below.IsEmpty())
            {
                GetNewPickupTarget();
                State = PawnState.Move;
            }
            else
            {
                SetCooldown(2);
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
            Pickup pickup = GetNewPickupTarget();
            if (pickup != null && pickup.PolarPos.DistanceSquaredTo(PolarPos) < 0.2f)
            {
                InventoryID = pickup.Item;
                InventoryCount += 1;
                pickup.QueueFree();
                
                State = PawnState.ReturnH;
                Target = new Vector2(ResourceStation.I.Surface.X, ResourceStation.I.Surface.Y);
            }
            else if (pickup != null)
            {
                Target = pickup.PolarPos;
                State = PawnState.Move;
            }
            else
            {
                State = PawnState.Idle;
            }

            SetCooldown(1);
        }
        else if (State == PawnState.DropOff)
        {
            if (InventoryID != Item.None)
            {
                if (!Inventory.Items.TryAdd(InventoryID, InventoryCount))
                {
                    Inventory.Items[InventoryID] += InventoryCount;
                }
                Resources.I.UpdateVisuals();

                InventoryID = Item.None;
                InventoryCount = 0;
            }
            
            State = PawnState.Idle;
            SetCooldown(1);
        }
    }

    private Pickup GetNewPickupTarget()
    {
        Pickup closest = null;
        float minDist = Single.PositiveInfinity;

        foreach (Node node in Game.I.Pickups.GetChildren())
        {
            Pickup child = (Pickup)node;

            float dist = child.PolarPos.DistanceSquaredTo(PolarPos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = child;
            }
        }

        if (closest != null)
            Target = closest.PolarPos;
        return closest;
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
    }
}