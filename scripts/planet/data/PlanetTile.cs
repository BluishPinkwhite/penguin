using System;

namespace Incremental.scripts.planet.data;

public class PlanetTile
{
    public float Integrity = 1.0f;   // 1 = intact, 0 = destroyed
    public TileMaterial Material;
    public bool Destroyed;
    public int OwnerID = -1;    // -1 = no owner, >=0 owned by pawn with that ID
}