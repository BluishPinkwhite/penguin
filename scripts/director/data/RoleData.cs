namespace Incremental.scripts.director.data;

public class RoleData
{
    public int BoughtAmount;
    public double NewCost;
    public Item CostMaterial;

    public RoleData(int boughtAmount, double newCost, Item costMaterial)
    {
        BoughtAmount = boughtAmount;
        NewCost = newCost;
        CostMaterial = costMaterial;
    }
}
