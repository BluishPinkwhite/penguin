using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Incremental.scripts.entity.pawn;
using Incremental.scripts.planet.data;
using Incremental.scripts.saving;

namespace Incremental.scripts.director;

public partial class Game : Node2D
{
    public static Game I { private set; get; }
    public readonly PlanetData _data = new(100, 16);
    private static Random _r = new();

    [Export] public PawnManager Pawns;
    [Export] public Node2D Stations;
    [Export] public Node2D Pickups;
    [Export] public Control ResearchWindow;
    [Export] public PackedScene EndScene;

    [Export] public PackedScene PickupScene;

    public double GameTime = 0;
    private int _prevGameTime = 0;

    public Game()
    {
        I = this;
        Inventory.Setup();
    }

    public override void _Ready()
    {
        GetTree().SetAutoAcceptQuit(false);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest || what == NotificationApplicationPaused)
        {
            SaveFileManager.Save();
            GetTree().Quit();
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        // ON Escape, end
        if (@event.IsActionPressed("ui_cancel"))
        {
            SaveFileManager.Save();
            GetTree().Quit();
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

    public override void _PhysicsProcess(double delta)
    {
        GameTime += delta;

        if ((int)GameTime != _prevGameTime)
        {
            _prevGameTime = (int)GameTime;
            Pawns.ShiftDamageList();
        }
    }

    public void EndGame()
    {
        GetTree().ChangeSceneToPacked(EndScene);   
    }
}