namespace Incremental.scripts.director;

public enum Item
{
    Penguin = -8,   // [0,2]
    None = -1,
    
    Dirt = 0,       // [0,0]
    Stone = 1,      // [1,0]
    Basalt = 2,     // [2,0]
    Magma = 3,    // [3,0]
    Gem = 4,        // [0,1]
    
}

public static class ItemExtensions {
    public static bool IsSpawnable(this Item item) => (int)item >= 0;
}