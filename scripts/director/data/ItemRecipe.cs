using System.Collections.Generic;
using Godot;

namespace Incremental.scripts.director.data;

public class ItemRecipe
{
    public bool Unlocked;
    public readonly List<Ingredient> Ingredients;
    public readonly List<Product> Products;
    
    
    private RecipeID ID;
    public ItemRecipe(RecipeID id, List<Ingredient> ingredients, List<Product> products, bool unlocked = false)
    {
        ID = id;
        Unlocked = unlocked;
        
        Ingredients = ingredients;
        Products = products;
        
        Inventory.Recipes[ID] = this;
    }
    
    public RecipeID GetID() => ID;
    
    
    public class Ingredient
    {
        public Item Item;
        public double Cost;

        public double CostMult;
        public double CostAdd;
        public double MaxCostChange;

        public Ingredient(Item item, double cost, double costMult, double costAdd, double maxCostChange)
        {
            Item = item;
            Cost = cost;
            CostMult = costMult;
            CostAdd = costAdd;
            MaxCostChange = maxCostChange;
        }
        
        public int RenderCost => Mathf.CeilToInt(Cost);
    }
    public class Product
    {
        public Item Item;
        public int Amount;
        public double Chance;

        public Product(Item item, int amount, double chance = 1)
        {
            Amount = amount;
            Item = item;
            Chance = chance;
        }
    }
}
