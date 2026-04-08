using Godot;
using System;

public partial class ShowOnHover : Control
{
	[Export] private Control _objectToShow;
	
	private bool _isHovering;
	
	public override void _Ready()
	{
		_isHovering = false;
		_objectToShow.Hide();
	}
	
	public override void _Process(double delta)
	{
		Vector2 mousePos = GetLocalMousePosition();

		Rect2 rect = new Rect2(Vector2.Zero, Size);

		bool inside = rect.HasPoint(mousePos);

		if (inside && !_isHovering)
		{
			_isHovering = true;
			OnMouseEnter();
		}
		else if (!inside && _isHovering)
		{
			_isHovering = false;
			OnMouseExit();
		}
	}

	public void OnMouseEnter()
	{
		_objectToShow.Show();
	}

	public void OnMouseExit()
	{
		_objectToShow.Hide();
	}
}
