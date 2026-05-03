using Incremental.ui;

namespace Incremental.scripts.director.data;

public class ItemData
{
    public bool _obtained;
    public int Amount = 0;
    public int RenderIndex;
    
    protected Item _item;

    public bool Obtained
    {
        get => _obtained;
        set
        {
            if (value && !_obtained)
            {
                MessageLog.QueueMessage("Obtained new item: " + _item.ToString().Replace("_", " "));
            }
            
            _obtained = value;
        }
    }
    
    public ItemData(Item item, int renderIndex, bool obtained = false)
    {
        _item = item;
        RenderIndex = renderIndex;
        _obtained = obtained || Amount > 0;

        Inventory.Items[item] = this;
    }
    
    public Item Item => _item;
}
