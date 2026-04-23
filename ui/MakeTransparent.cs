using Godot;
using System;

public partial class MakeTransparent : ColorRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		T();
	}

	private void T()
	{
		Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
		
		Tween tween = CreateTween();
			
		tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);

		tween.Finished += QueueFree;
	}
}
