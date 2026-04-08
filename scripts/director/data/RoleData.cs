using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.director.data;

public class RoleData
{
    public int BoughtAmount;
    public double NewCost;
    public Item CostMaterial = Item.None;
    public Role RoleCost;

    public RoleData(int boughtAmount, double newCost, Item costMaterial)
    {
        BoughtAmount = boughtAmount;
        NewCost = newCost;
        CostMaterial = costMaterial;
    }
    
    public RoleData(int boughtAmount, double newCost, Role roleCost)
    {
        BoughtAmount = boughtAmount;
        NewCost = newCost;
        RoleCost = roleCost;
    }
}
