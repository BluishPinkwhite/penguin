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
    public int InventorySize = 2;

    private Pickup _pickupTarget;


    protected override void DoBehaviour(float d)
    {
        InventorySize = 2 + Inventory.Items[Item.Bigger_Baskets].Amount;

        float gravityY = PolarPos.Y - d * Gravity;

        if (State is PawnState.Idle or PawnState.GiveUp)
        {
            if (State == PawnState.GiveUp)
            {
                visual.Rotate(Game.RandomAround(0.28f, 0.1f));
            }

            // wait till the pawn is on the ground
            PlanetTile below = Game.I._data.GetTileAtPolarCoords(PolarPos.X, gravityY);
            if (below != null && !below.IsEmpty())
            {
                if (State == PawnState.Idle)
                {
                    _pickupTarget = GetNewPickupTarget();
                    if (_pickupTarget != null)
                        State = PawnState.Move;
                    else if (InventoryCount > 0)
                    {
                        _pickupTarget = null;
                        Target = new Vector2(ResourceStation.I.Surface.X, ResourceStation.I.Surface.Y);
                        State = PawnState.ReturnH;
                    }
                }
                else
                {
                    BreakTile(below, Mathf.FloorToInt(PolarPos.X), Mathf.FloorToInt(gravityY));
                    Retire();
                }
            }
            else if (State == PawnState.Idle)
            {
                SetCooldown(2);
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

                Target = new Vector2(ResourceStation.I.Surface.X, ResourceStation.I.Surface.Y);
                Counter++;

                State = InventoryCount < InventorySize ? PawnState.Idle : PawnState.ReturnH;
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

                if (InventoryID == Item.Gem)
                {
                    if (Inventory.Items[Item.Research_Station].Amount == 0)
                        Inventory.UnlockRecipe(RecipeID.Unlock_Research);
                    Inventory.UnlockRecipe(RecipeID.AssignRole_Archeologist);
                }

                if (InventoryID == Item.Stone
                    && Inventory.Items[Item.Tougher_Pickaxes].Amount < 4
                    && Inventory.IsResearchUnlocked(RecipeID.Unlock_Research))
                {
                    Inventory.UnlockRecipe(RecipeID.Tougher_Pickaxes);
                }

                InventoryID = Item.None;
                InventoryCount = 0;
                PickupSprite.Visible = false;
            }

            if (Counter > Consts.Pawns[Role.Hauler].RetirementCycles)
            {
                State = PawnState.RetireH;
                Pickup pickup = GetNewPickupTarget();

                float x = pickup?.PolarPos.X ?? Game.RandomAround(PolarPos.X, 5);
                float y = Game.RandomAround(ResourceStation.I.Surface.Y + 50, 5);

                Target = new Vector2(x, y);
            }
            else
            {
                State = PawnState.Idle;
                SetCooldown(1);
            }
        }
    }

    private Pickup GetNewPickupTarget()
    {
        Pickup closest = null;
        float minDist = Single.PositiveInfinity;

        IList<Node> children = Game.I.Pickups.GetChildren();

        int count = Math.Min(16, Mathf.CeilToInt(children.Count / 10f));
        foreach (Node node in Game.TakeRandom(children, count))
        {
            Pickup child = (Pickup)node;

            if (InventoryID != Item.None && child.Item != InventoryID)
                continue;

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
        else if (State is PawnState.Move or PawnState.ReturnH or PawnState.RetireH or PawnState.GiveUp)
        {
            visual.Animation = Flying ? "fly" : "walk";
        }
        else if (State is PawnState.ReturnV or PawnState.RetireV)
        {
            visual.Animation = "fly";
        }
    }
}