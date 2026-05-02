using System;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.ui;

public partial class DamageBar : HBoxContainer
{
    [Export] private Role _role;
 
    public static event Action OnDataUpdate;
    
    private ProgressBar _bar;

    
    public static void Invoke()
    {
        OnDataUpdate?.Invoke();
    }
    
    
    public override void _Ready()
    {
        _bar = GetChild<ProgressBar>(1);

        OnDataUpdate += () => SetValue(Game.I.Pawns.GetDamagePercent(_role));
    }

    private void SetValue(float value)
    {
        if (value > 0)
        {
            Visible = true;

            if (value >= 99.5f)
                _bar.SetValue(100);
            else
                _bar.SetValue(Math.Max(1, value - 1));
        }
        else
        {
            Visible = false;
        }
    }
}