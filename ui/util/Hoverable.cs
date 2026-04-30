using Godot;

namespace Incremental.ui.util;

public partial class Hoverable : Control
{
    [Export] public bool OnlySelf = false;
    
    public override void _Ready()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        
        OnMouseExited();
    }

    private void OnMouseEntered()
    {
        if (OnlySelf)
            SelfModulate = UIConsts.active;
        else 
            Modulate = UIConsts.active;
    }
    
    private void OnMouseExited()
    {
        if (OnlySelf)
            SelfModulate = UIConsts.inactive;
        else 
            Modulate = UIConsts.inactive;
    }
}