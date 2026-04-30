using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.pawn.roles;
using Incremental.scripts.entity.station;

namespace Incremental.scripts.entity.pawn;

public partial class PawnManager : Node2D
{
    private readonly Dictionary<Role, int> _spawnedRoles = new();
    
    [Export] PackedScene[] PawnScenes = new PackedScene[3];
    
    
    public override void _Ready()
    {
        UpdatePawnCounts();
    }

    public void UpdatePawnCounts()
    {
        foreach (ItemData data in Inventory.Items.Values)
        {
            if (!data.Item.IsRole())
                continue;

            Role role = data.Item.AsRole();
            _spawnedRoles.TryGetValue(role, out int i);

            if (i >= data.Amount)
            {
                // clamp
                _spawnedRoles[role] = data.Amount;
            }
            else
            {
                int index = -1;
                if (role == Role.Miner) index = 0;
                else if (role == Role.Hauler) index = 1;
                else if (role == Role.Archeologist) index = 2;

                if (index == -1)
                    continue;
                
                // spawn more
                for (; i < data.Amount; i++)
                {
                    Pawn p = PawnScenes[index].Instantiate<Pawn>();
                    p.Role = role;
                    if (role == Role.Archeologist)
                    {
                        p.PolarPos = station.ResearchStation.I.PolarPos;
                    }
                    else
                    {
                        p.PolarPos = ResourceStation.I.GetParent().GetChild<OrbitEntity>(1).PolarPos;
                    }
                    p.Target = p.PolarPos;
                    AddChild(p);
                }

                _spawnedRoles[role] = data.Amount;
            }
        }
    }
}