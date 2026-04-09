using System;
using Incremental.scripts.director;
using Incremental.scripts.planet.rendering;

namespace Incremental.scripts.planet.data;

public class PlanetTile
{
    public const int LightReach = 8;
    public const float LightMax = 1f + 1f/LightReach;
    
    
    public float Integrity = 1.0f;   // 1 = intact, 0 = destroyed
    public TileMaterial Material;
    public bool Regrowing = false;
    public int OwnerID = -1;    // -1 = no owner, >=0 owned by pawn with that ID

    private float _light = 0;
    public float Light
    {
        get => _light;
        set => _light = Math.Clamp(value, 0, LightMax);
    }

    public Item Destroy()
    {
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
        Material = TileMaterial.Unknown;
        Light = 0;

        return item;
    }
    
    public void Renew(TileMaterial material)
    {
        Material = material;
        Light = 0;
        Regrowing = true;
        Integrity = 1;
        OwnerID = -1;
    }
    
    public bool IsEmpty() => Integrity <= 0f || Regrowing;
}