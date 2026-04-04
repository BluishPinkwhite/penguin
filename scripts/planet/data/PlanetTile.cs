using Incremental.scripts.director;
using Incremental.scripts.planet.rendering;

namespace Incremental.scripts.planet.data;

public class PlanetTile
{
    public float Integrity = 1.0f;   // 1 = intact, 0 = destroyed
    public TileMaterial Material;
    public bool Destroyed;
    public int OwnerID = -1;    // -1 = no owner, >=0 owned by pawn with that ID
    public float Light = Game.RandomAround(0.75f, 0.25f);
    

    public Item Destroy()
    {
        if (Destroyed)
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
        OwnerID = other.OwnerID;
        Light = other.Light;
    }
}