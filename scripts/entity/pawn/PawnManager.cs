using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.entity.pawn;

public partial class PawnManager : Node2D
{
    private Dictionary<Role, int> _spawnedRoles = new();
    
    [Export] PackedScene[] PawnScenes = new PackedScene[2];
    
    
    public override void _Ready()
    {
        Inventory.Roles[Role.Unemployed] = new RoleData(0, 2, Item.Dirt);
        Inventory.Roles[Role.Miner] = new RoleData(8, 1, Role.Unemployed);
        Inventory.Roles[Role.Hauler] = new RoleData(5, 1, Role.Unemployed);
        
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
                    int index = -1;
                    if (pair.Key == Role.Miner) index = 0;
                    else if (pair.Key == Role.Hauler) index = 1;

                    if (index == -1)
                        break;
                    
                    Pawn p = PawnScenes[index].Instantiate<Pawn>();
                    p.Role = pair.Key;
                    AddChild(p);
                }

                _spawnedRoles[pair.Key] = pair.Value.BoughtAmount;
            }
        }
    }
}