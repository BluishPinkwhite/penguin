using Godot;
using Incremental.ui.util;

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
        
        UpdateState(1);
    }

    private void OnPauseButtonPressed()
    {
        GetTree().Paused = true;
        UpdateState(0);
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
        SetSpeed(10);
    }

    private void SetSpeed(int mult)
    {
        Engine.TimeScale = _defaultTimeScale * mult;
        Engine.PhysicsTicksPerSecond = (int)(_defaultPhysicsTicksPerSecond * mult);
        Engine.MaxPhysicsStepsPerFrame = (int)(_defaultMaxPhysicsStepsPerFrame * mult);
        
        UpdateState(mult);
    }

    private void UpdateState(int speed)
    {
        pauseButton.Modulate = speed == 0 ? UIConsts.active : UIConsts.inactive;
        resumeButton.Modulate = speed == 1 ? UIConsts.active : UIConsts.inactive;
        speedUpButton.Modulate = speed == 2 ? UIConsts.active : UIConsts.inactive;
        reallySpeedUpButton.Modulate = speed > 2 ? UIConsts.active : UIConsts.inactive;
    }
}