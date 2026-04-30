using Godot;
using System;

public partial class EndScreen : Control
{
	[Signal]
	public delegate void SequenceFinishedEventHandler();

	[Export] public RichTextLabel DisplayLabel;
	[Export] public float FadeInTime = .5f;
	[Export] public float DisplayTime = 3.0f;
	[Export] public float FadeOutTime = .5f;
	[Export] public float DelayBetweenTexts = 0.3f;
	string[] texts = new []
	{
		"Congratulations! You have reached the end of the demo.",
		"Please let us know how you enjoyed it.",
		"Thank you for playing!"
	};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SequenceFinished += LoadMainMenu;
		
		if (DisplayLabel != null)
		{
			// Start with the label being transparent
			DisplayLabel.Modulate = new Color(1, 1, 1, 0);
		}
		
		PlayTextSequence(texts);
	}

	/// <summary>
	/// Animates a sequence of texts by fading them in and out one by one.
	/// </summary>
	/// <param name="t">Array of texts to display sequentially.</param>
	public async void PlayTextSequence(string[] t)
	{
		try
		{
			if (DisplayLabel == null)
			{
				GD.PrintErr("DisplayLabel is not assigned in EndScreen script.");
				return;
			}

			if (t == null || t.Length == 0)
			{
				EmitSignal(SignalName.SequenceFinished);
				return;
			}

			// Ensure the label starts transparent
			DisplayLabel.Modulate = new Color(DisplayLabel.Modulate.R, DisplayLabel.Modulate.G, DisplayLabel.Modulate.B, 0);

			await ToSignal(GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);
		
			Tween tween = CreateTween();

			foreach (string text in t)
			{
				string currentText = text;

				tween.TweenCallback(Callable.From(() => DisplayLabel.Text = currentText));
				tween.TweenProperty(DisplayLabel, "modulate:a", 1.0f, FadeInTime);
				tween.TweenInterval(DisplayTime);
				tween.TweenProperty(DisplayLabel, "modulate:a", 0.0f, FadeOutTime);
				tween.TweenInterval(DelayBetweenTexts);
			}

			tween.Finished += () => EmitSignal(SignalName.SequenceFinished);
		}
		catch (Exception e)
		{
			GD.PrintErr($"Error in PlayTextSequence: {e.Message}");
		}
	}

	private void LoadMainMenu()
	{
		GetTree().ChangeSceneToFile("res://ui/main_menu/main_menu.tscn");
	}
}
