using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.ui.clickable;

public partial class ResourcesCopyCuzMeStoopid : BoxContainer
{
	public static ResourcesCopyCuzMeStoopid I;

	[Export] private Array<BoxContainer> _containers;
	[Export] private Array<Label> _labels = new();
	public List<Purchasable> _purchasables = new();
	
	public ResourcesCopyCuzMeStoopid()
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

		UpdateVisuals();
	}

	public void UpdateVisuals()
	{
		foreach (ItemData data in Inventory.Items.Values)
		{
			int index = -1;
			if (data.Item == Item.Dirt)
				index = 0;
			else if (data.Item == Item.Stone)
				index = 1;
			else if (data.Item == Item.Basalt)
				index = 2;
			else if (data.Item == Item.Magma)
				index = 3;
			else if (data.Item == Item.Gem)
				index = 4;
			else if (data.Item == Item.Component)
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
	}
}
