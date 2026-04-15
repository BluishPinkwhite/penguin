using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.ui.clickable;

namespace Incremental.ui;

public partial class Resources : BoxContainer
{
    public static Resources I;

    private List<BoxContainer> _containers = new();
    private List<Label> _labels = new();
    public List<Purchasable> _purchasables = new();

    [Export] private BoxContainer _penguinContainer;
    private Label _penguinLabel;

    public Resources()
    {
        I = this;
    }

    public override void _Ready()
    {
        base._Ready();

        foreach (Node child in GetChildren())
        {
            if (child is BoxContainer container)
            {
                _containers.Add(container);
                _labels.Add(container.GetNode<Label>("Count"));
                container.Visible = false;
            }
        }

        _penguinLabel = _penguinContainer.GetNode<Label>("Count");

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        foreach (ItemData data in Inventory.Items.Values)
        {
            int index = -1;
            if (data.item == Item.Dirt)
                index = 0;
            else if (data.item == Item.Stone)
                index = 1;
            else if (data.item == Item.Basalt)
                index = 2;
            else if (data.item == Item.Magma)
                index = 3;
            else if (data.item == Item.Gem)
                index = 4;
            else if (data.item == Item.Component)
                index = 5;
            else continue;
            
            if (data.Obtained)
            {
                _containers[index].Visible = true;
                _labels[index].Text = data.Amount.ToString();
            }
            else
            {
                _containers[index].Visible = false;
            }
        }

        foreach (Purchasable purchasable in _purchasables)
        {
            purchasable.UpdateVisuals();
        }

        _penguinLabel.Text = Inventory.Items[Item.Penguin].Amount.ToString();
    }
}