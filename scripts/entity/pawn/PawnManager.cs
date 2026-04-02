using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.entity.pawn;

public partial class PawnManager : Node2D
{
    private Dictionary<Role, int> _spawnedRoles = new();
    
    public override void _Ready()
    {
        Inventory.Roles[Role.Miner] = new RoleData(8, 10, Item.Dirt);
        Inventory.Roles[Role.Hauler] = new RoleData(5, 7, Item.Dirt);
        
        UpdatePawnCounts();
    }

    public void UpdatePawnCounts()
    {
        foreach (KeyValuePair<Role, RoleData> pair in Inventory.Roles)
        {
            _spawnedRoles.TryGetValue(pair.Key, out int i);

            if (i >= pair.Value.BoughtAmount)
            {
                _spawnedRoles[pair.Key] = pair.Value.BoughtAmount;
            }
            else
            {
                for (; i < pair.Value.BoughtAmount; i++)
                {
                    Pawn p = Game.I.PawnScene.Instantiate<Pawn>();
                    p.Role = pair.Key;
                    AddChild(p);
                }

                _spawnedRoles[pair.Key] = pair.Value.BoughtAmount;
            }
        }
    }
}