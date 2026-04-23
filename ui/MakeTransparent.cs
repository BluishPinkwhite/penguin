using Godot;
using System;

public partial class MakeTransparent : ColorRect
{
	public override void _Ready()
	{
		Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 1);
		T();
	}

	private async void T()
	{
		await ToSignal(GetTree().CreateTimer(1f), "timeout");
		Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0);
		
		Tween tween = CreateTween();
			
		tween.TweenProperty(this, "modulate:a", 0.0f, 1f);

		tween.Finished += QueueFree;
	}
}
