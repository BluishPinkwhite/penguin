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
    }


    public virtual void OnClick()
    {
        if (Type == ClickType.BuyRole)
        {
            // do nothing - Purchasable handles this
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