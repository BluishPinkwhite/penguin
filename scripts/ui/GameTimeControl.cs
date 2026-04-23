using Godot;
using System;

public partial class GameTimeControl : Control
{
	[Export] private TextureButton pauseButton;
	[Export] private TextureButton resumeButton;
	[Export] private TextureButton speedUpButton;
	[Export] private TextureButton reallySpeedUpButton;
	
	public override void _Ready()
	{
		pauseButton.Pressed += OnPauseButtonPressed;
		resumeButton.Pressed += OnResumeButtonPressed;
		speedUpButton.Pressed += OnSpeedUpButtonPressed;
		reallySpeedUpButton.Pressed += OnReallySpeedUpButtonPressed;
	}

	private void OnPauseButtonPressed()
	{
		GetTree().Paused = true;
	}
	
	private void OnResumeButtonPressed()
	{
		GetTree().Paused = false;
		Engine.TimeScale = 1;
	}
	
	private void OnSpeedUpButtonPressed()
	{
		GetTree().Paused = false;
		Engine.TimeScale = 2;
	}
	
	private void OnReallySpeedUpButtonPressed()
	{
		GetTree().Paused = false;
		Engine.TimeScale = 8;
	}
}
