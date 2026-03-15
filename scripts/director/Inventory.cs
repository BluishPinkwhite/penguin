using System.Collections.Generic;
using Incremental.scripts.entity.pawn.roles;

namespace Incremental.scripts.director;

public class Inventory
{
    public static readonly Dictionary<Item, int> Items = new();
    public static Dictionary<Role, int> Roles = new();

    public static int GetItemCount(Item id)
    {
        return Items.GetValueOrDefault(id, 0);
    }
    
    public static int GetRoleCount(Role id)
    {
        return Roles.GetValueOrDefault(id, 0);
    }
    
    public static int GetTotalPawnCount()
    {
        int count = 0;
        foreach (KeyValuePair<Role, int> pair in Roles)
        {
            count += pair.Value;
        }

        return count;
    }
}