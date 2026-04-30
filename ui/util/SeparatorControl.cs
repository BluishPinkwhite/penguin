using Godot;

namespace Incremental.ui.util;

public partial class SeparatorControl : Control
{
    public override void _Ready()
    {
        GetParent<Control>().ItemRectChanged += OnItemRectChanged;
        OnItemRectChanged();
    }

    private void OnItemRectChanged()
    {
        bool active = false;
        foreach (Node child in GetParent().GetChildren())
        {
            if (child is Control control)
            {
                if (active && control.Visible)
                {
                    Visible = true;
                    return;
                }

                if (child == this)
                    active = true;
            }
        }

        Visible = false;
    }
}