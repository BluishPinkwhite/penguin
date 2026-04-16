using System;
using System.Collections.Generic;
using Incremental.scripts.director.data;
using Incremental.ui;

namespace Incremental.scripts.director;

public static class Inventory
{
    public static readonly Dictionary<Item, ItemData> Items = new();
    public static readonly Dictionary<RecipeID, ItemRecipe> Recipes = new();

    public static void Setup()
    {
        Items.Clear();
        Recipes.Clear();

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
            new ItemRecipe.Product(Item.Dirt, 1), new ItemRecipe.Product(Item.Gem, 1, 0.0001)
        ], true);
        new ItemRecipe(RecipeID.Mine_Stone, [], [
            new ItemRecipe.Product(Item.Stone, 1), new ItemRecipe.Product(Item.Gem, 1, 0.0005)
        ], true);
        new ItemRecipe(RecipeID.Mine_Basalt, [], [
            new ItemRecipe.Product(Item.Basalt, 1), new ItemRecipe.Product(Item.Gem, 1, 0.0015)
        ], true);
        new ItemRecipe(RecipeID.Mine_Magma, [], [
            new ItemRecipe.Product(Item.Magma, 1), new ItemRecipe.Product(Item.Gem, 1, 0.0025)
        ], true);
            
        new ItemRecipe(RecipeID.Gather_Component, [], [
            new ItemRecipe.Product(Item.Component, 1), new ItemRecipe.Product(Item.Gem, 1, 0.2)
        ], true);

        new ItemRecipe(RecipeID.NewPenguinFor_Dirt,
            [new ItemRecipe.Ingredient(Item.Dirt, 1, 1.1, 0, 8)],
            [new ItemRecipe.Product(Item.Penguin, 1)], true);
        new ItemRecipe(RecipeID.NewPenguinFor_Stone,
            [new ItemRecipe.Ingredient(Item.Stone, 2, 1.2, 0, 7)],
            [new ItemRecipe.Product(Item.Penguin, 1)]);
        new ItemRecipe(RecipeID.NewPenguinFor_Basalt,
            [new ItemRecipe.Ingredient(Item.Basalt, 4, 1.1, 0, 6)],
            [new ItemRecipe.Product(Item.Penguin, 1)]);
        new ItemRecipe(RecipeID.NewPenguinFor_Magma,
            [new ItemRecipe.Ingredient(Item.Magma, 6, 1.1, 0, 5)],
            [new ItemRecipe.Product(Item.Penguin, 1)]);

        new ItemRecipe(RecipeID.Penguin_Retirement, [],
            [new ItemRecipe.Product(Item.Penguin, 1)], true);

        new ItemRecipe(RecipeID.AssignRole_Miner,
            [new ItemRecipe.Ingredient(Item.Penguin, 1, 1, 0, 0)],
            [new ItemRecipe.Product(Item.Miner, 1)], true);
        new ItemRecipe(RecipeID.AssignRole_Hauler,
            [new ItemRecipe.Ingredient(Item.Penguin, 1, 1, 0, 0)],
            [new ItemRecipe.Product(Item.Hauler, 1)], true);
        new ItemRecipe(RecipeID.AssignRole_Archeologist,
            [
                new ItemRecipe.Ingredient(Item.Penguin, 1, 1, 0, 0),
                new ItemRecipe.Ingredient(Item.Gem, 1, 1, 0, 0)
            ],
            [new ItemRecipe.Product(Item.Archeologist, 1)]);
    }


    public static List<(Item item, int amount)> TryGetRecipe(RecipeID id)
    {
        List<(Item item, int amount)> list = new();
        
        if (Recipes.TryGetValue(id, out ItemRecipe recipe))
        {
            bool can = true;

            foreach (ItemRecipe.Ingredient ingredient in recipe.Ingredients)
            {
                if (Items[ingredient.Item].Amount < ingredient.RenderCost)
                {
                    can = false;
                    break;
                }
            }

            if (can)
            {
                // remove ingredients
                foreach (ItemRecipe.Ingredient ingredient in recipe.Ingredients)
                {
                    Items[ingredient.Item].Amount -= ingredient.RenderCost;

                    // increase price
                    if (ingredient.MaxCostChange > 0)
                    {
                        double newCost = ingredient.Cost * ingredient.CostMult + ingredient.CostAdd;
                        ingredient.Cost = Math.Min(newCost, ingredient.Cost + ingredient.MaxCostChange);
                    }
                }

                // add products
                foreach (ItemRecipe.Product product in recipe.Products)
                {
                    if (product.Chance >= 1 || Game.RandomTo(1) < product.Chance)
                    {
                        list.Add((product.Item, product.Amount));
                    }
                }
                
                Resources.I.UpdateVisuals();
            }
        }

        return list;
    }
    
    public static void ApplyRecipe(RecipeID id)
    {
        if (Recipes.TryGetValue(id, out ItemRecipe recipe))
        {
            bool can = true;

            foreach (ItemRecipe.Ingredient ingredient in recipe.Ingredients)
            {
                if (Items[ingredient.Item].Amount < ingredient.RenderCost)
                {
                    can = false;
                    break;
                }
            }

            if (can)
            {
                // remove ingredients
                foreach (ItemRecipe.Ingredient ingredient in recipe.Ingredients)
                {
                    Items[ingredient.Item].Amount -= ingredient.RenderCost;

                    // increase price
                    if (ingredient.MaxCostChange > 0)
                    {
                        double newCost = ingredient.Cost * ingredient.CostMult + ingredient.CostAdd;
                        ingredient.Cost = Math.Min(newCost, ingredient.Cost + ingredient.MaxCostChange);
                    }
                }

                // add products
                foreach (ItemRecipe.Product product in recipe.Products)
                {
                    if (product.Chance >= 1 || Game.RandomTo(1) < product.Chance)
                    {
                        Items[product.Item].Amount += product.Amount;
                    }
                }

                Resources.I.UpdateVisuals();
                Game.I.Pawns.UpdatePawnCounts();
            }
        }
    }

    public static void UnlockRecipe(RecipeID id)
    {
        if (Recipes.TryGetValue(id, out ItemRecipe recipe))
        {
            recipe.Unlocked = true;

            Resources.I.UpdateVisuals();
        }
    }
}