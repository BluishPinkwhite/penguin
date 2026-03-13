using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.entity.pawn;

public partial class PawnManager : Node2D
{
    [Export] private PackedScene _pawnScene;
    
    public override void _Ready()
    {
        foreach (Role role in Enum.GetValues(typeof(Role)))
        {
            Inventory.Roles[role] = (int)(Game.RandomTo(2) + 2);
        }

        foreach (KeyValuePair<Role, int> pair in Inventory.Roles)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                Pawn p = _pawnScene.Instantiate<Pawn>();
                p.SetRole(pair.Key);
                AddChild(p);
            }
        }
    }
}