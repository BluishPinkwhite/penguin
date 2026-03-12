using System;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.director;

public class Game
{
    public static readonly Game I = new Game();

    public readonly PlanetData _data = new(100, 16);


    private static Random _r = new();

    public static float RandomAround(float value, float difference)
    {
        return value + (_r.NextSingle() * 2 - 1) * difference;
    }
    
    public static float RandomTo(float value)
    {
        return value * _r.NextSingle();
    }
}