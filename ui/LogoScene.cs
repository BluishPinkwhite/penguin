using Godot;
using System;

public partial class LogoScene : Control
{
	[Signal]
	public delegate void SequenceFinishedEventHandler();

	[Export] public TextureRect Logo;
	[Export] public float FadeInTime = .5f;
	[Export] public float DisplayTime = 2.0f;
	[Export] public float FadeOutTime = .5f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SequenceFinished += LoadMainMenu;
		
		if (Logo != null)
		{
			// Start with the label being transparent
			Logo.Modulate = new Color(1, 1, 1, 0);
		}
		
		PlayTextSequence();
	}

	/// <summary>
	/// Animates a sequence of texts by fading them in and out one by one.
	/// </summary>
	/// <param name="t">Array of texts to display sequentially.</param>
	public async void PlayTextSequence()
	{
		try
		{
			Logo.Modulate = new Color(Logo.Modulate.R, Logo.Modulate.G, Logo.Modulate.B, 0);

			await ToSignal(GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);
		
			Tween tween = CreateTween();
			
			tween.TweenProperty(Logo, "modulate:a", 1.0f, FadeInTime);
			tween.TweenInterval(DisplayTime);
			tween.TweenProperty(Logo, "modulate:a", 0.0f, FadeOutTime);

			tween.Finished += () => EmitSignal(SignalName.SequenceFinished);
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error in PlayTextSequence: {e.Message}");
		}
	}

	private void LoadMainMenu()
	{
		GetTree().ChangeSceneToFile("res://ui/main_menu.tscn");
	}
}
