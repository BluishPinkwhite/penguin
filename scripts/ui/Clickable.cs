using Godot;
using System;

public partial class Clickable : Node
{
	private bool _isFocused;
	
	public void OnMouseEnter()
	{
		GD.Print("hover_enter " + Name);
		_isFocused = true;
	}
	
	public void OnMouseExit()
	{
		GD.Print("hover_exit " + Name);
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
		GD.Print("click " + Name);
	}
	
}

