using Godot;
using Incremental.scripts.director;
using Incremental.scripts.entity.pawn;
using Incremental.ui;

namespace Incremental.scripts.ui.clickable;

public partial class Clickable : Control
{
    protected bool IsFocused;

    [Export] protected ClickType Type;
    [Export] protected ClickParam Param;


    public override void _Ready()
    {
        base._Ready();
        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;
    }

    public void OnMouseEnter()
    {
        IsFocused = true;
    }

    public void OnMouseExit()
    {
        IsFocused = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsFocused) return;

        base._Input(@event);
        if (@event.IsActionPressed("press"))
        {
            OnClick();
        }
        else if (@event.IsActionReleased("press_secondary"))
        {
            for(int i = 0; i < 5; i++)
                OnClick();
        }
    }


    public virtual void OnClick()
    {
        if (Type == ClickType.BuyRole)
        {
            // do nothing - Purchasable handles this
        }
        else if (Type == ClickType.ToggleWindow)
        {
            if (Param == ClickParam.Window_Research)
            {
                Game.I.ResearchWindow.Visible = !Game.I.ResearchWindow.Visible;

                if (Game.I.ResearchWindow.Visible)
                {
                    Game.I.ResearchWindow.GetNode<TreeViewControl>("Control").UpdateVisuals();
                }
            }
        }
        else if (Type == ClickType.ChangeDebug)
        {
            if (Param == ClickParam.Debug_ShowState)
            {
                foreach (Node child in Game.I.Pawns.GetChildren())
                {
                    if (child is Pawn pawn)
                        pawn.DebugText.Visible = !pawn.DebugText.Visible;
                }
            }
            else if (Param == ClickParam.Debug_ShowTargets)
            {
                Game.I.Debug.Visible = !Game.I.Debug.Visible;
            }
        }
        else
        {
            GD.Print($"Clickable not implemented - {Type} : {Param}");
        }

        Resources.I.UpdateVisuals();
    }
}