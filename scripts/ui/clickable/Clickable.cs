using Godot;
using System;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.pawn;
using Incremental.scripts.entity.pawn.roles;
using Incremental.scripts.ui.clickable;

public partial class Clickable : Node
{
    private bool _isFocused;

    [Export] private ClickType _type;
    [Export] private ClickParam _param;


    public void OnMouseEnter()
    {
        _isFocused = true;
    }

    public void OnMouseExit()
    {
        _isFocused = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isFocused) return;

        base._Input(@event);
        if (@event.IsActionPressed("press"))
        {
            OnClick();
        }
    }


    public void OnClick()
    {
        if (_type == ClickType.BuyRole)
        {
            RoleData roleData = Inventory.Roles[(Role)_param];

            if (Inventory.Items[roleData.CostMaterial] >= roleData.NewCost)
            {
                roleData.BoughtAmount++;
                Inventory.Items[roleData.CostMaterial] -= (int)roleData.NewCost;
                roleData.NewCost *= 1.2;
            }
        }
        else if (_type == ClickType.ChangeDebug)
        {
            if (_param == ClickParam.Debug_ShowState)
            {
                foreach (Node child in Game.I.Pawns.GetChildren())
                {
                    if (child is Pawn pawn)
                        pawn.DebugText.Visible = !pawn.DebugText.Visible;
                }
            }
            else if (_param == ClickParam.Debug_ShowTargets)
            {
                Game.I.Debug.Visible = !Game.I.Debug.Visible;
            }
        }
        else
        {
            GD.Print($"Clickable not implemented - {_type} : {_param}");
        }

        Resources.I.UpdateVisuals();
    }
}