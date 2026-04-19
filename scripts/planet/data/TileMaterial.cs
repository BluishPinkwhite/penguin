using System;

namespace Incremental.scripts.planet.data;

public enum TileMaterial
{
    Grass = 0,
    Dirt = 1,
    Stone = 2,
    Basalt = 3,
    Magma = 4,
    Lava = 5,
    Core = 6,
    Unknown = 15,
}

public static class TileMaterialExtensions
{
    public static float BreakTime(this TileMaterial m)
    {
        return (float)Math.Pow(Math.Max(1, (int)m), 1.5f);
    }
}