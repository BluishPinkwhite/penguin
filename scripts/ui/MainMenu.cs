using Godot;
using Incremental.scripts.saving;

namespace Incremental.scripts.ui;

public partial class MainMenu : Control
{
    [Export] private Control CreditsUI;
    [Export] private PackedScene PlayScene;

    private enum MainMenuHoverState
    {
        None,
        Continue,
        NewGame,
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
            case MainMenuHoverState.Continue:
                SaveFileManager.TryLoad = true;
                Play();
                break;
            case MainMenuHoverState.NewGame:
                SaveFileManager.TryLoad = false;
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
        _hoverState = MainMenuHoverState.Continue;
    }

    public void OnPlayUnhover()
    {
        _hoverState = MainMenuHoverState.None;
    }

    public void OnNewGameHover()
    {
        _hoverState = MainMenuHoverState.NewGame;
    }

    public void OnNewGameUnhover()
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