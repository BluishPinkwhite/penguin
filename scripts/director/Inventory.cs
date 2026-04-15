using System.Collections.Generic;
using Incremental.scripts.director.data;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.director;

public static class Inventory
{
    public static readonly Dictionary<Item, int> Items = new();
    public static readonly Dictionary<Role, RoleData> Roles = new();
    
    

    public static int GetItemCount(Item id)
    {
        return Items.GetValueOrDefault(id, 0);
    }
    
    public static int GetRoleCount(Role id)
    {
        if (Roles.TryGetValue(id, out RoleData role))
            return role.BoughtAmount;

        return 0;
    }
    
    public static int GetTotalPawnCount()
    {
        int count = 0;
        foreach (KeyValuePair<Role, RoleData> pair in Roles)
        {
            count += pair.Value.BoughtAmount;
        }

        return count;
    }
}