using System;

namespace Incremental.scripts.planet.data;

public class PlanetTile
{
    public float Integrity = 1.0f;   // 1 = intact, 0 = destroyed
    public TileMaterial Material;
    public bool Destroyed;
}