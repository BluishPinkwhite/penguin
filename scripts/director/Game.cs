using System;
using Godot;
using Incremental.scripts.debug;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.director;

public partial class Game : Node2D
{
    public static Game I { private set; get; }
    public readonly PlanetData _data = new(100, 16);
    private static Random _r = new();
    
    [Export] public Node2D Pawns;
    [Export] public Node2D Stations;
    [Export] public Node2D Pickups;
    [Export] public DebugDraw Debug;

    [Export] public PackedScene PawnScene;
    [Export] public PackedScene PickupScene;

    public Game()
    {
        I = this;
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
}