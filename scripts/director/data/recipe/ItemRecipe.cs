using System.Collections.Generic;
using Incremental.ui;

namespace Incremental.scripts.director.data.recipe;

public class ItemRecipe
{
    public bool Unlocked;
    public readonly List<Ingredient> Ingredients;
    public readonly List<Product> Products;
    
    protected readonly RecipeID ID;

    public ItemRecipe(RecipeID id, List<Ingredient> ingredients, List<Product> products, bool unlocked = false)
    {
        ID = id;
        Unlocked = unlocked;

        Ingredients = ingredients;
        Products = products;
        
        Inventory.Recipes[ID] = this;
    }


    public RecipeID GetID() => ID;


    public static bool GetRecipe(RecipeID id, out ItemRecipe recipe)
    {
        return Inventory.Recipes.TryGetValue(id, out recipe);
    }


    public bool HasIngredients()
    {
        foreach (Ingredient ingredient in Ingredients)
        {
            if (Inventory.Items[ingredient.Item].Amount < ingredient.RenderCost)
                return false;
        }

        return true;
    }


    public static List<(Item item, int amount)> TryGetOutput(RecipeID id)
    {
        List<(Item item, int amount)> list = new();

        if (!GetRecipe(id, out ItemRecipe recipe))
            return list;

        if (recipe.HasIngredients())
        {
            // remove ingredients
            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                Inventory.Items[ingredient.Item].Amount -= ingredient.RenderCost;

                ingredient.IncreaseCost();
            }

            Resources.I.UpdateVisuals();

            if (recipe.Products != null)
            {
                // add products
                foreach (Product product in recipe.Products)
                {
                    if (product.Chance >= 1 || Game.RandomTo(1) < product.Chance)
                    {
                        list.Add((product.Item, product.Amount));
                    }
                }
            }
        }

        return list;
    }


    public static bool TryApplyRecipe(RecipeID id)
    {
        if (!GetRecipe(id, out ItemRecipe recipe))
            return false;

        if (recipe.HasIngredients())
        {
            // remove ingredients
            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                Inventory.Items[ingredient.Item].Amount -= ingredient.RenderCost;

                ingredient.IncreaseCost();
            }

            recipe.Apply();
            if (recipe.ID == RecipeID.Unlock_Research)
            {
                recipe.Unlocked = false;
            }

            if (recipe.ID == RecipeID.Tougher_Pickaxes && Inventory.Items[Item.Tougher_Pickaxes].Amount > 4)
            {
                recipe.Unlocked = false;
            }

            if (recipe.ID == RecipeID.Finer_Brushes)
            {
                if(Inventory.Items[Item.Finer_Brushes].Amount <= 2)
                    Inventory.UnlockRecipe(RecipeID.Finer_Brushes);
                else
                    recipe.Unlocked = false;
            }
            
            if (recipe.ID == RecipeID.Precise_Pickaxes)
            {
                if(Inventory.Items[Item.Higher_Crit_Chance].Amount <= 2)
                    Inventory.UnlockRecipe(RecipeID.Precise_Pickaxes);
                else
                    recipe.Unlocked = false;
            }
            
            if (recipe.ID == RecipeID.Faster_Running)
            {
                if(Inventory.Items[Item.Faster_Running].Amount <= 4)
                    Inventory.UnlockRecipe(RecipeID.Faster_Running);
                else
                    recipe.Unlocked = false;
            }
            
            if (recipe.ID == RecipeID.Better_Jetpacks)
            {
                if(Inventory.Items[Item.Better_Jetpacks].Amount <= 2)
                    Inventory.UnlockRecipe(RecipeID.Better_Jetpacks);
                else
                    recipe.Unlocked = false;
            }
            
            if (recipe.ID == RecipeID.Bigger_Baskets)
            {
                if(Inventory.Items[Item.Bigger_Baskets].Amount <= 3)
                    Inventory.UnlockRecipe(RecipeID.Bigger_Baskets);
                else
                    recipe.Unlocked = false;
            }
            
            if (recipe.ID == RecipeID.Orbital_Core_Extractor)
            {
                // TODO: END THE GAME
            }
            
            return true;
        }

        return false;
    }

    protected virtual void Apply()
    {
        // add products
        foreach (Product product in Products)
        {
            if (product.Chance >= 1 || Game.RandomTo(1) < product.Chance)
            {
                Inventory.Items[product.Item].Amount += product.Amount;
            }
        }

        Resources.I.UpdateVisuals();
        Game.I.Pawns.UpdatePawnCounts();
    }
}