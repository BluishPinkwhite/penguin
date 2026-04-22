using System;
using Godot;

namespace Incremental.scripts.director.data.recipe;

public class Ingredient
{
    public Item Item;
    public double Cost;

    public double CostMult;
    public double CostAdd;
    public double MaxCostChange;

    public Ingredient(Item item, double cost, double costMult = 1, double costAdd = 0, double maxCostChange = 0)
    {
        Item = item;
        Cost = cost;
        CostMult = costMult;
        CostAdd = costAdd;
        MaxCostChange = maxCostChange;
    }

    public int RenderCost => Mathf.CeilToInt(Cost);
    public string RenderText => Item + " x" + RenderCost;

    
    public void IncreaseCost()
    {
        if (MaxCostChange > 0)
        {
            double newCost = Cost * CostMult + CostAdd;
            Cost = Math.Min(newCost, Cost + MaxCostChange);
        }
        else if (MaxCostChange < 0)
        {
            double newCost = Cost * CostMult + CostAdd;
            Cost = newCost;
        }
    }
}