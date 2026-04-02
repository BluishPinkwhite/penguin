using System;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.ui.clickable;

public partial class Purchasable : Clickable
{
    [Export] private Label _costLabel;
    [Export] private TextureRect _costIcon;
    [Export] private Label _purchasableNameLabel;

    public override void _Ready()
    {
        base._Ready();

        UpdateVisuals();
    }

    public override void OnClick()
    {
        base.OnClick();

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (Inventory.Roles.TryGetValue((Role)Param, out RoleData roleData))
        {
            // TODO set icon
            
            _costLabel.Text = ((int)Math.Ceiling(roleData.NewCost)).ToString();
            _purchasableNameLabel.Text = Param.ToString().Split("_")[^1];
        }
    }
}