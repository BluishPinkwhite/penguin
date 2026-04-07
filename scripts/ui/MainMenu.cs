using Godot;
using System;
using Incremental.scripts.director;

public partial class MainMenu : Control
{
    [Export] private Control CreditsUI;
    [Export] private PackedScene PlayScene;

    private enum MainMenuHoverState
    {
        None,
        Play,
        Credits,
        Quit
    }

    private MainMenuHoverState _hoverState = MainMenuHoverState.None;
    
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed("press"))
        {
            OnClick();
        }
    }

    public void OnClick()
    {
        if (_hoverState == MainMenuHoverState.None) return;

        switch (_hoverState)
        {
            case MainMenuHoverState.Play:
                Play();
                break;
            case MainMenuHoverState.Credits:
                ShowCredits();
                break;
            case MainMenuHoverState.Quit:
                GetTree().Quit();
                break;
        }
    }

    private void Play()
    {
        GetTree().ChangeSceneToPacked(PlayScene);
    }

    private void ShowCredits()
    {
        CreditsUI.Visible = true;
    }

    public void CloseCredits()
    {
        CreditsUI.Visible = false;
    }

    public void OnPlayHover()
    {
        _hoverState = MainMenuHoverState.Play;
    }

    public void OnPlayUnhover()
    {
        _hoverState = MainMenuHoverState.None;
    }

    public void OnQuitHover()
    {
        _hoverState = MainMenuHoverState.Quit;
    }

    public void OnQuitUnhover()
    {
        _hoverState = MainMenuHoverState.None;
    }

    public void OnCreditsHover()
    {
        _hoverState = MainMenuHoverState.Credits;
    }

    public void OnCreditsUnhover()
    {
        _hoverState = MainMenuHoverState.None;
    }
}