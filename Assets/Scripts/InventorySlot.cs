public class InventorySlot
{
    private InventoryItemData _item;
    private int _count;

    public InventoryItemData Item
    {
        get { return _item; }
    }

    public int Count
    {
        get { return _count; }
    }

    public InventorySlot(InventoryItemData item)
    {
        _item = item;
        _count = 1;
    }

    public void AddOne()
    {
        _count++;
    }

    public void RemoveOne()
    {
        if (_count <= 0) return;
        _count--;
    }
}
