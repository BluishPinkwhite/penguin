using System;
using Incremental.scripts.director;
using Incremental.scripts.planet.rendering;

namespace Incremental.scripts.planet.data;

public class PlanetTile
{
    public const int LightReach = 8;
    
    
    public float Integrity = 1.0f;   // 1 = intact, 0 = destroyed
    public TileMaterial Material;
    public bool Destroyed;
    public int OwnerID = -1;    // -1 = no owner, >=0 owned by pawn with that ID

    private float _light = 0;
    public float Light
    {
        get => _light;
        set => _light = Math.Clamp(value, 0, 1f + 1f / LightReach);
    }

    public Item Destroy()
    {
        if (IsEmpty())
            return Item.None;

        PlanetRenderer.isDirty = true;

        Item item = Material switch
        {
            TileMaterial.Grass or TileMaterial.Dirt => Item.Dirt,
            TileMaterial.Stone => Item.Stone,
            TileMaterial.Basalt => Item.Basalt,
            TileMaterial.Magma => Item.Magma,
            _ => Item.None
        };
        
        Integrity = 0;
        Destroyed = true;
        Material = TileMaterial.Unknown;
        Light = 1;

        return item;
    }
    
    public void CopyOver(PlanetTile other)
    {
        Integrity = other.Integrity;
        Material = other.Material;
        Destroyed = other.Destroyed;
        Light = other.Light;
        
        OwnerID = -1;
    }
    
    public bool IsEmpty() => Destroyed || Material == TileMaterial.Unknown;
}