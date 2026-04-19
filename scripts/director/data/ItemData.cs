namespace Incremental.scripts.director.data;

public class ItemData
{
    public bool Obtained;
    public int Amount = 0;
    public int RenderIndex;
    
    protected Item _item;
    public ItemData(Item item, int renderIndex, bool obtained = false)
    {
        _item = item;
        RenderIndex = renderIndex;
        Obtained = obtained || Amount > 0;

        Inventory.Items[item] = this;
    }
    
    public Item Item => _item;
}
