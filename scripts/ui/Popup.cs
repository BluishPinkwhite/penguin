using Godot;
using System;

public partial class Popup : Control
{
	private Vector2 _scale;

	public override void _Ready()
	{
		_scale = Scale;
	}

	public override void _Process(double delta)
	{
		Camera2D cam = GetViewport().GetCamera2D();

		if (cam == null) return;
		
		Scale = _scale / cam.Zoom;
	}
}