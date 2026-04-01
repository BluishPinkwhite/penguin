using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.entity.pawn;

public partial class PawnManager : Node2D
{
    public override void _Ready()
    {
        Inventory.Roles[Role.Miner] = 8;
        Inventory.Roles[Role.Hauler] = 5;

        foreach (KeyValuePair<Role, int> pair in Inventory.Roles)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                Pawn p = Game.I.PawnScene.Instantiate<Pawn>();
                p.SetRole(pair.Key);
                AddChild(p);
            }
        }
    }
}