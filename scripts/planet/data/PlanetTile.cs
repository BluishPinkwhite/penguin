using System;
using System.Collections.Generic;
using Incremental.scripts.director.data;
using Incremental.scripts.director.data.recipe;
using Incremental.scripts.entity.pawn;
using Incremental.scripts.entity.pawn.roles;
using Incremental.scripts.planet.rendering;

namespace Incremental.scripts.planet.data;

public class PlanetTile
{
    public const int LightReach = 7;
    public const float LightMax = 1f + 1f/LightReach;

    public static int MaxOwners = 3; 
    
    public float Integrity = 1.0f;   // 1 = intact, 0 = destroyed
    public TileMaterial Material;
    public bool Regrowing = false;
    
    private List<int> OwnerIDs = [];    // -1 = no owner, >=0 owned by pawn with that ID
    private Role OwnerRole = Role.Unemployed;

    public readonly int PolarX;
    public readonly int PolarY;

    private float _light = 0;
    public float Light
    {
        get => _light;
        set => _light = Math.Clamp(value, 0, LightMax);
    }

    public PlanetTile(int polarX, int polarY)
    {
        PolarX = polarX;
        PolarY = polarY;
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
        
        ClearOwners();

        return recipe;
    }
    
    public void Renew(TileMaterial material)
    {
        Material = material;
        Light = 0;
        Regrowing = true;
        Integrity = 1;
        
        ClearOwners();
    }
    
    public bool IsEmpty() => Integrity <= 0f;

    public bool IsOwnedBy(Pawn pawn)
    {
        return OwnerRole != Role.Unemployed && OwnerIDs.Contains(pawn.ID);
    }
    
    public bool CanBeOwnedBy(Pawn pawn)
    {
        return (OwnerRole == Role.Unemployed || OwnerRole == pawn.Role) && OwnerIDs.Count < MaxOwners;
    }

    public void AddOwner(Pawn pawn)
    {
        OwnerIDs.Add(pawn.ID);
        OwnerRole = pawn.Role;
    }
    
    public void RemoveOwner(Pawn pawn)
    {
        OwnerIDs.Remove(pawn.ID);
        
        if (OwnerIDs.Count == 0)
            OwnerRole = Role.Unemployed;
    }

    public void ClearOwners()
    {
        OwnerIDs.Clear();
        OwnerRole = Role.Unemployed;
    }

    public bool IsOwned()
    {
        return OwnerIDs.Count > 0;
    }
}