using UnityEngine;

[System.Serializable] public class Slot
{
    [SerializeField] private int _index;
    private Inventory _assignedInventory;
    [SerializeField] private Item _storedItem;
    [SerializeField] private bool _isOccupied;
    [SerializeField] private ItemType _itemType;

    public void SetupSlot(Inventory inventory, int index)
    {
        _assignedInventory = inventory;

        this._index = index;
    }

    public int Index => _index;
    public Inventory Inventory => _assignedInventory;
    public Item StoredItem => _storedItem;
    public bool IsOccupied => _isOccupied;
    public ItemType SlotType => _itemType;

    public void AssignItem(Item item)
    {
        _storedItem = item;

        _isOccupied = true;

        Debug.Log($"{_storedItem.Data.itemTitle} was placed in {_index} slot.");
    }

    public void FreeSlot()
    {
        Debug.Log($"{_storedItem.Data.itemTitle} was taken from {_index} slot.");

        _storedItem = null;

        _isOccupied = false;
    }
}