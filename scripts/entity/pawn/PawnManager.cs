using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.pawn.roles;
using Incremental.scripts.entity.station;
using Incremental.ui;

namespace Incremental.scripts.entity.pawn;

public partial class PawnManager : Node2D
{
    private readonly Dictionary<Role, int> _spawnedRoles = new();
    
    [Export] PackedScene[] PawnScenes = new PackedScene[3];

    private readonly Dictionary<Role, DamageEntry> _damages = new();
    private int _damageIndex = 0;
    private float _damageTotal = 0;

    private const int dataCount = 300;
    
    
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
                        p.PolarPos = ResearchStation.I.PolarPos;
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

    public void RecordDamage(Role role, float damage)
    {
        if (!_damages.ContainsKey(role))
            _damages.Add(role, new DamageEntry());
        
        _damages[role].data[_damageIndex] += damage;
    }

    public void ShiftDamageList()
    {
        // recalc total
        _damageTotal = 0;
        foreach (DamageEntry entry in _damages.Values)
        {
            entry.total -= entry.data[_damageIndex];
            _damageTotal += entry.total;
        }

        // shift index
        _damageIndex++;
        if (_damageIndex >= dataCount)
            _damageIndex = 0;
        
        // reset at new index
        foreach (DamageEntry entry in _damages.Values)
            entry.data[_damageIndex] = 0;

        // display
        DamageBar.Invoke();
    }

    // returns [0, 100]
    public float GetDamagePercent(Role role)
    {
        if (!_damages.TryGetValue(role, out DamageEntry damage))
            return 0;
        
        return damage.total * 100f / _damageTotal;
    }

    private class DamageEntry
    {
        public float[] data = new float[dataCount];
        public float total = 0;
    }
}