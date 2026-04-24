using Godot;

namespace Incremental.scripts.ui;

public partial class GameTimeControl : Control
{
    [Export] private TextureButton pauseButton;
    [Export] private TextureButton resumeButton;
    [Export] private TextureButton speedUpButton;
    [Export] private TextureButton reallySpeedUpButton;

    private double _defaultTimeScale;
    private int _defaultPhysicsTicksPerSecond;
    private int _defaultMaxPhysicsStepsPerFrame;

    public override void _Ready()
    {
        pauseButton.Pressed += OnPauseButtonPressed;
        resumeButton.Pressed += OnResumeButtonPressed;
        speedUpButton.Pressed += OnSpeedUpButtonPressed;
        reallySpeedUpButton.Pressed += OnReallySpeedUpButtonPressed;

        _defaultTimeScale = Engine.TimeScale;
        _defaultPhysicsTicksPerSecond = Engine.PhysicsTicksPerSecond;
        _defaultMaxPhysicsStepsPerFrame = Engine.MaxPhysicsStepsPerFrame;
    }

    private void OnPauseButtonPressed()
    {
        GetTree().Paused = true;
    }

    private void OnResumeButtonPressed()
    {
        GetTree().Paused = false;
        SetSpeed(1);
    }

    private void OnSpeedUpButtonPressed()
    {
        GetTree().Paused = false;
        SetSpeed(2);
    }

    private void OnReallySpeedUpButtonPressed()
    {
        GetTree().Paused = false;
        SetSpeed(8);
    }

    private void SetSpeed(float mult)
    {
        Engine.TimeScale = _defaultTimeScale * mult;
        Engine.PhysicsTicksPerSecond = (int)(_defaultPhysicsTicksPerSecond * mult);
        Engine.MaxPhysicsStepsPerFrame = (int)(_defaultMaxPhysicsStepsPerFrame * mult);
    }
}