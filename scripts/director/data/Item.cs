using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.director.data;

public enum Item
{
    None = -1,

    Penguin = 16,
    
    Miner = 1000 + Role.Miner,
    Hauler = 1000 + Role.Hauler,
    Archeologist = 1000 + Role.Archeologist,

    Dirt = 0,
    Stone = 1,
    Basalt = 2,
    Magma = 3,
    Gem = 8,
    Component = 9,
    
    Research_Station = 2000,
    
    Tougher_Pickaxes = 3000,
    Finer_Brushes = 3001,
    Higher_Crit_Chance = 3002,
    Faster_Running = 3003,
    Better_Jetpacks = 3004,
    Bigger_Baskets = 3005,
    Orbital_Core_Extractor = 3006,
}


public static class ItemExtensions
{
    public static bool Renderable(this Item r) => (int)r >= 0;
    public static bool IsRole(this Item r) => (int)r >= 1000;
    public static Role AsRole(this Item r) => (Role)(r - 1000);
    public static int RenderIndex(this Item r) => (int)r;
}