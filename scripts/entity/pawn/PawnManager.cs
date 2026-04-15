using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.pawn.roles;
using Incremental.scripts.entity.station;

namespace Incremental.scripts.entity.pawn;

public partial class PawnManager : Node2D
{
    private Dictionary<Role, int> _spawnedRoles = new();
    
    [Export] PackedScene[] PawnScenes = new PackedScene[2];
    
    
    public override void _Ready()
    {
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
                    p.PolarPos = ResourceStation.I.GetParent().GetChild<OrbitEntity>(1).PolarPos;
                    p.Target = p.PolarPos;
                    AddChild(p);
                }

                _spawnedRoles[pair.Key] = pair.Value.BoughtAmount;
            }
        }
    }
}