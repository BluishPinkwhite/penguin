namespace Incremental.scripts.director.data;

public enum RecipeID
{
    None = 0,
    
    Mine_Dirt = 1,
    Mine_Stone = 2,
    Mine_Basalt = 3,
    Mine_Magma = 4,
    
    NewPenguinFor_Dirt = 1000 + Item.Dirt,
    NewPenguinFor_Stone = 1000 + Item.Stone,
    NewPenguinFor_Basalt = 1000 + Item.Basalt,
    NewPenguinFor_Magma = 1000 + Item.Magma,
    
    Penguin_Retirement = -1,
    
    AssignRole_Miner = 2000 + Item.Miner,
    AssignRole_Hauler = 2000 + Item.Hauler,
    AssignRole_Archeologist = 2000 + Item.Archeologist,
}