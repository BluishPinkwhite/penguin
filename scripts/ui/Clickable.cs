using Godot;
using System;
using Incremental.scripts.director;
using Incremental.scripts.entity.pawn;
using Incremental.scripts.entity.pawn.roles;

public partial class Clickable : Node
{
    private bool _isFocused;

    [Export] private ClickableType _type;

    public void OnMouseEnter()
    {
        // GD.Print("hover_enter " + Name);
        _isFocused = true;
    }

    public void OnMouseExit()
    {
        // GD.Print("hover_exit " + Name);
        _isFocused = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isFocused) return;

        base._Input(@event);
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                OnClick();
            }
        }
    }


    public void OnClick()
    {
        // GD.Print("click " + Name);
        if (_type == ClickableType.MinerInc)
        {
            Inventory.Roles[Role.Miner]++;
            Inventory.Items[Item.Dirt] -= 10;
        }
        else if (_type == ClickableType.CollectorInc)
        {
            Inventory.Roles[Role.Hauler]++;
            Inventory.Items[Item.Dirt] -= 7;
        }
        Resources.I.UpdateVisuals();
        
    }
}

public enum ClickableType
{
    MinerInc,
    CollectorInc
}