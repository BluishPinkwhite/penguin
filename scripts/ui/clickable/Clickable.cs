using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.pawn;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.ui.clickable;

public partial class Clickable : Node
{
    protected bool IsFocused;

    [Export] protected ClickType Type;
    [Export] protected ClickParam Param;


    public void OnMouseEnter()
    {
        IsFocused = true;
    }

    public void OnMouseExit()
    {
        IsFocused = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsFocused) return;

        base._Input(@event);
        if (@event.IsActionPressed("press"))
        {
            OnClick();
        }
    }


    public virtual void OnClick()
    {
        if (Type == ClickType.BuyRole)
        {
            if (Inventory.Roles.TryGetValue((Role)Param, out RoleData roleData))
            {
                if (roleData.CostMaterial.IsSpawnable() &&
                    Inventory.Items.GetValueOrDefault(roleData.CostMaterial, 0) >= roleData.NewCost)
                {
                    roleData.BoughtAmount++;
                    Inventory.Items[roleData.CostMaterial] -= (int)roleData.NewCost;
                    roleData.NewCost *= 1.2;
                }
                else
                {
                    if (roleData.NewCost <= Inventory.Roles[roleData.RoleCost].BoughtAmount)
                    {
                        Inventory.Roles[roleData.RoleCost].BoughtAmount -= (int)roleData.NewCost;
                        roleData.BoughtAmount++;
                    }
                }

                Game.I.Pawns.UpdatePawnCounts();
            }
        }
        else if (Type == ClickType.ChangeDebug)
        {
            if (Param == ClickParam.Debug_ShowState)
            {
                foreach (Node child in Game.I.Pawns.GetChildren())
                {
                    if (child is Pawn pawn)
                        pawn.DebugText.Visible = !pawn.DebugText.Visible;
                }
            }
            else if (Param == ClickParam.Debug_ShowTargets)
            {
                Game.I.Debug.Visible = !Game.I.Debug.Visible;
            }
        }
        else
        {
            GD.Print($"Clickable not implemented - {Type} : {Param}");
        }

        Resources.I.UpdateVisuals();
    }
}