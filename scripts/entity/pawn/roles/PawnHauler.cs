using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.station;
using Incremental.scripts.planet.data;
using Incremental.ui;

namespace Incremental.scripts.entity.pawn.roles;

public partial class PawnHauler : Pawn
{
    [Export] private Sprite2D PickupSprite;
    
    public Item InventoryID = Item.None;
    public int InventoryCount;
    
    private Pickup _pickupTarget;
    
    
    protected override void DoBehaviour(float d)
    {
        float gravityY = PolarPos.Y - d * Gravity;

        if (State == PawnState.Idle)
        {
            // wait till the pawn is on the ground
            PlanetTile below = Game.I._data.GetTileAtPolarCoords(PolarPos.X, gravityY);
            if (below != null && !below.IsEmpty())
            {
                _pickupTarget = GetNewPickupTarget();
                if (_pickupTarget != null)
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
            if (IsInstanceValid(_pickupTarget)
                && !_pickupTarget.IsQueuedForDeletion()
                && _pickupTarget.PolarPos.DistanceSquaredTo(PolarPos) < 0.2f)
            {
                InventoryID = _pickupTarget.Item;
                InventoryCount += 1;

                PickupSprite.RegionRect = _pickupTarget.GetPickupCoords();
                PickupSprite.Visible = true;
                
                _pickupTarget.QueueFree();
                _pickupTarget = null;
                
                State = PawnState.ReturnH;
                Target = new Vector2(ResourceStation.I.Surface.X, ResourceStation.I.Surface.Y);
            }
            else if (IsInstanceValid(_pickupTarget))
            {
                Target = _pickupTarget.PolarPos;
                State = PawnState.Move;
            }
            else
            {
                State = PawnState.Idle;
                _pickupTarget = null;
            }

            SetCooldown(1);
        }
        else if (State == PawnState.DropOff)
        {
            if (InventoryID != Item.None)
            {
                Inventory.Items[InventoryID].Amount += InventoryCount;
                Resources.I.UpdateVisuals();

                Inventory.Items[InventoryID].Obtained = true;
                if (Inventory.Recipes.ContainsKey((RecipeID)InventoryID + 1000))
                    Inventory.Recipes[(RecipeID)InventoryID + 1000].Unlocked = true;

                InventoryID = Item.None;
                InventoryCount = 0;
                PickupSprite.Visible = false;
            }
            
            State = PawnState.Idle;
            SetCooldown(1);
        }
    }

    private Pickup GetNewPickupTarget()
    {
        Pickup closest = null;
        float minDist = Single.PositiveInfinity;

        IList<Node> children = Game.I.Pickups.GetChildren();

        int count = Math.Min(8, Mathf.CeilToInt(children.Count / 10f));
        foreach (Node node in Game.TakeRandom(children, count))
        {
            Pickup child = (Pickup)node;

            float dist = Game.I._data.PolarDistanceSquared(child.PolarPos, PolarPos);
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