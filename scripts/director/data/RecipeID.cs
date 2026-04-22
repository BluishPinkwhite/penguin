namespace Incremental.scripts.director.data;

public enum RecipeID
{
    None = 0,
    
    Mine_Dirt = 1,
    Mine_Stone = 2,
    Mine_Basalt = 3,
    Mine_Magma = 4,
    
    Gather_Component = 10,
    
    NewPenguinFor_Dirt = 1000 + Item.Dirt,
    NewPenguinFor_Stone = 1000 + Item.Stone,
    NewPenguinFor_Basalt = 1000 + Item.Basalt,
    NewPenguinFor_Magma = 1000 + Item.Magma,
    
    Penguin_Retirement = -1,
    
    AssignRole_Miner = 2000 + Item.Miner,
    AssignRole_Hauler = 2000 + Item.Hauler,
    AssignRole_Archeologist = 2000 + Item.Archeologist,
    
    Unlock_Research = 2500,
    
    Research_FirstResearch = 3000,
    Research_BiggerZoomLens = 3001,
    Research_FinerBrushes = 3002,
    Research_PrecisePickaxes = 3003,
    Research_BasaltUpgrade = 3004,
    Research_MagmaReinforcement = 3005,
    Research_EnergyDrinks = 3006,
    Research_AncientMiningTechnology = 3007,
    Research_Running = 3008,
    Research_JetpackShoes = 3009,
    Research_FasterJetpackAscent = 3010,
    Research_BiggerBaskets = 3011,
    Research_AncientCollectorKnowledge = 3012,
    Research_OrbitalCoreExtractor = 3013,
    Research_ErgonomicHandles = 3014,
    
    Tougher_Pickaxes = 4000,
    Finer_Brushes = 4001,
    Precise_Pickaxes = 4002,
    Faster_Running = 4003,
    Better_Jetpacks = 4004,
    Bigger_Baskets = 4005,
    Orbital_Core_Extractor = 4006,
}