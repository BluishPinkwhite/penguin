namespace Incremental.scripts.director.data;

public class ItemData
{
    public bool Obtained;
    public int Amount = 0;
    public int RenderIndex;
    
    public Item item;
    public ItemData(Item item, int renderIndex, bool obtained = false)
    {
        this.item = item;
        RenderIndex = renderIndex;
        Obtained = obtained || Amount > 0;

        Inventory.Items[item] = this;
    }
}
