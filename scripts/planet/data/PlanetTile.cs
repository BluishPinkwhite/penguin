using System;
using Incremental.scripts.director.data;
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

    public RecipeID Destroy()
    {
        PlanetRenderer.isDirty = true;

        RecipeID recipe = Material switch
        {
            TileMaterial.Grass or TileMaterial.Dirt => RecipeID.Mine_Dirt,
            TileMaterial.Stone => RecipeID.Mine_Stone,
            TileMaterial.Basalt => RecipeID.Mine_Basalt,
            TileMaterial.Magma => RecipeID.Mine_Magma,
            _ => RecipeID.None
        };
        
        Integrity = 0;
        Material = TileMaterial.Unknown;
        Light = 0;

        return recipe;
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