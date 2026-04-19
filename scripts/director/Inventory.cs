using System;
using System.Collections.Generic;
using Incremental.scripts.director.data;
using Incremental.scripts.director.data.recipe;
using Incremental.ui;

namespace Incremental.scripts.director;

public static class Inventory
{
    public static readonly Dictionary<Item, ItemData> Items = new();
    public static readonly Dictionary<RecipeID, ItemRecipe> Recipes = new();
    public static readonly Dictionary<RecipeID, bool> Research = new();

    public static void Setup()
    {
        Items.Clear();
        Recipes.Clear();
        Research.Clear();

        new ItemData(Item.None, 0);
        new ItemData(Item.Penguin, 8, true);
        new ItemData(Item.Miner, 8) { Amount = 8 };
        new ItemData(Item.Hauler, 8) { Amount = 6 };
        new ItemData(Item.Archeologist, 8);

        new ItemData(Item.Dirt, 0, true);
        new ItemData(Item.Stone, 1);
        new ItemData(Item.Basalt, 2);
        new ItemData(Item.Magma, 3);
        new ItemData(Item.Gem, 4);
        new ItemData(Item.Component, 5);

        new ItemRecipe(RecipeID.None, [], []);

        new ItemRecipe(RecipeID.Mine_Dirt, [], [
            new Product(Item.Dirt, 1), new Product(Item.Gem, 1, 0.01)
        ], true);
        new ItemRecipe(RecipeID.Mine_Stone, [], [
            new Product(Item.Stone, 1), new Product(Item.Gem, 1, 0.05)
        ], true);
        new ItemRecipe(RecipeID.Mine_Basalt, [], [
            new Product(Item.Basalt, 1), new Product(Item.Gem, 1, 0.15)
        ], true);
        new ItemRecipe(RecipeID.Mine_Magma, [], [
            new Product(Item.Magma, 1), new Product(Item.Gem, 1, 0.25)
        ], true);
            
        new ItemRecipe(RecipeID.Gather_Component, [], [
            new Product(Item.Component, 1), new Product(Item.Gem, 1, 0.2)
        ], true);

        new ItemRecipe(RecipeID.NewPenguinFor_Dirt,
            [new Ingredient(Item.Dirt, 1, 1.1, 0, 8)],
            [new Product(Item.Penguin, 1)], true);
        new ItemRecipe(RecipeID.NewPenguinFor_Stone,
            [new Ingredient(Item.Stone, 2, 1.2, 0, 7)],
            [new Product(Item.Penguin, 1)]);
        new ItemRecipe(RecipeID.NewPenguinFor_Basalt,
            [new Ingredient(Item.Basalt, 4, 1.1, 0, 6)],
            [new Product(Item.Penguin, 1)]);
        new ItemRecipe(RecipeID.NewPenguinFor_Magma,
            [new Ingredient(Item.Magma, 6, 1.1, 0, 5)],
            [new Product(Item.Penguin, 1)]);

        new ItemRecipe(RecipeID.Penguin_Retirement, [],
            [new Product(Item.Penguin, 1)], true);

        new ItemRecipe(RecipeID.AssignRole_Miner,
            [new Ingredient(Item.Penguin, 1)],
            [new Product(Item.Miner, 1)], true);
        new ItemRecipe(RecipeID.AssignRole_Hauler,
            [new Ingredient(Item.Penguin, 1)],
            [new Product(Item.Hauler, 1)], true);
        new ItemRecipe(RecipeID.AssignRole_Archeologist,
            [
                new Ingredient(Item.Penguin, 1),
                new Ingredient(Item.Gem, 1)
            ],
            [new Product(Item.Archeologist, 1)]);

        
        new ResearchRecipe(RecipeID.Research_FirstResearch,
            [new Ingredient(Item.Stone, 20)]);
        new ResearchRecipe(RecipeID.Research_BiggerZoomLens,
            [new Ingredient(Item.Gem, 5)]);
        new ResearchRecipe(RecipeID.Research_FinerBrushes,
            [new Ingredient(Item.Gem, 20)]);
        new ResearchRecipe(RecipeID.Research_PrecisePickaxes,
            [new Ingredient(Item.Component, 5), new Ingredient(Item.Stone, 100)]);
        new ResearchRecipe(RecipeID.Research_BasaltUpgrade,
            [new Ingredient(Item.Component, 25), new Ingredient(Item.Basalt, 100)]);
        new ResearchRecipe(RecipeID.Research_MagmaReinforcement,
            [new Ingredient(Item.Component, 50), new Ingredient(Item.Magma, 25)]);
        new ResearchRecipe(RecipeID.Research_EnergyDrinks,
            [new Ingredient(Item.Component, 15)]);
        new ResearchRecipe(RecipeID.Research_AncientMiningTechnology,
            [new Ingredient(Item.Component, 100)]);
        new ResearchRecipe(RecipeID.Research_Running,
            [new Ingredient(Item.Component, 3)]);
        new ResearchRecipe(RecipeID.Research_JetpackShoes,
            [new Ingredient(Item.Component, 10)]);
        new ResearchRecipe(RecipeID.Research_ErgonomicHandles,
            [new Ingredient(Item.Component, 10)]);
        new ResearchRecipe(RecipeID.Research_FasterJetpackAscent,
            [new Ingredient(Item.Component, 40)]);
        new ResearchRecipe(RecipeID.Research_BiggerBaskets,
            [new Ingredient(Item.Dirt, 1000), new Ingredient(Item.Component, 25)]);
        new ResearchRecipe(RecipeID.Research_AncientCollectorKnowledge,
            [new Ingredient(Item.Component, 100)]);
        new ResearchRecipe(RecipeID.Research_OrbitalCoreExtractor,
            [new Ingredient(Item.Component, 250), new Ingredient(Item.Gem, 50)]);
    }

    

    public static void UnlockRecipe(RecipeID id)
    {
        if (Recipes.TryGetValue(id, out ItemRecipe recipe))
        {
            recipe.Unlocked = true;

            Resources.I.UpdateVisuals();
        }
    }
    
    public static bool IsResearchUnlocked(RecipeID id)
    {
        return Research.ContainsKey(id) && Research[id];
    }
}