using System.Collections.Generic;

namespace Incremental.scripts.director;

public class Inventory
{
    private static Dictionary<Item, int> _inventory = new();

    public static int GetCount(Item id)
    {
        return _inventory.GetValueOrDefault(id, 0);
    }
}