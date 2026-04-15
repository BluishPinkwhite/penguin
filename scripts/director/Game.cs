using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Incremental.scripts.debug;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.pawn;
using Incremental.scripts.entity.pawn.roles;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.director;

public partial class Game : Node2D
{
    public static Game I { private set; get; }
    public readonly PlanetData _data = new(100, 16);
    private static Random _r = new();

    [Export] public PawnManager Pawns;
    [Export] public Node2D Stations;
    [Export] public Node2D Pickups;
    [Export] public DebugDraw Debug;

    [Export] public PackedScene PickupScene;

    public Game()
    {
        I = this;
        
        Inventory.Roles[Role.Unemployed] = new RoleData(0, 2, Item.Dirt);
        Inventory.Roles[Role.Miner] = new RoleData(8, 1, Role.Unemployed);
        Inventory.Roles[Role.Hauler] = new RoleData(5, 1, Role.Unemployed);
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Node child in Debug.GetChildren())
        {
            child.QueueFree();
        }
    }

    public static float RandomAround(float value, float difference)
    {
        return value + (_r.NextSingle() * 2 - 1) * difference;
    }

    public static float RandomTo(float value)
    {
        return value * _r.NextSingle();
    }

    public static IEnumerable<T> TakeRandom<T>(IEnumerable<T> source, int count)
    {
        if (source == null) yield break;
        if (count <= 0) yield break;
        
        IList<T> list = source as IList<T> ?? source.ToList();
        count = Math.Min(count, list.Count);

        for (int i = 0; i < count; i++)
        {
            int j = _r.Next(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
            yield return list[i];
        }
    }
}