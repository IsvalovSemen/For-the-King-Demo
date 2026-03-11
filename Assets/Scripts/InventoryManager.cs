using System;
using System.Collections.Generic;
using UnityEngine;
using static UIManager;
using static UnityEngine.Rendering.DebugUI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public List<ItemStats> itemDatabase;
    // Events for UI.
    public event Action<Inventory> OnInventoryChanged;
    public event Action OnItemPickUpConfirmation;

    private int _currentRow = 0;
    private int _currentColumn = 0;

    [Header("Grid settings:")]
    [SerializeField] private int equipRows;
    [SerializeField] private int equipColumns;
    [SerializeField] private int bagRows;
    [SerializeField] private int bagColumns;
    [Header("Dragging data:")]
    private ItemInstance _draggedItem;
    private bool _isDragging = false;
    private ItemSlot _prevSlot; // Stores the slot from which item was removed at the start of the drag.

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("More than one InventoryManager.", this); // Proper way to throw an error with a link to the cause.

            Destroy(gameObject);
        }

        UIManager.instance.OnInventoryClosure += CancelDrag;
    }

    private void Update()
    {
        if (UIManager.instance.GetCurrentMenu == MenuState.Inventory)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) MovePointer(Vector2Int.up);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) MovePointer(Vector2Int.down);
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) MovePointer(Vector2Int.left);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) MovePointer(Vector2Int.right);

            ItemSlot selectedSlot = GetSelectedSlot();

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!_isDragging)
                {
                    TakeFromSlot(selectedSlot);
                }
                else
                {
                    StoreInSlot(selectedSlot);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) && _isDragging)
            {
                CancelDrag();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (selectedSlot.IsOccupied == true)
                {
                    selectedSlot.Inventory.DropItem(selectedSlot.Index, false);
                }
                else
                {
                    Debug.Log("No item in this cell to drop.");

                    return;
                }
            }
        }
    }
    #region UTILITY METHODS
    private void MovePointer(Vector2Int dir)
    {
        int newRow = _currentRow - dir.y;
        int newColumn = _currentColumn + dir.x;

        if (_currentRow < equipRows) // When in equipment section.
        {
            if (newRow >= equipRows)
            {
                newColumn = 0; // If transfer to bag section, snap to the left column.
            }

            newRow = Mathf.Clamp(newRow, 0, (bagRows + equipRows) - 1); // Clamp on vertical.

            newColumn = Mathf.Clamp(newColumn, 0, equipColumns - 1); // Clamp on horizontal.
        }
        else // When in bag section
        {
            if (newRow < equipRows)
            {
                newColumn = 0;
            }

            newRow = Mathf.Clamp(newRow, 0, (bagRows + equipRows) - 1);

            newColumn = Mathf.Clamp(newColumn, 0, bagColumns - 1);
        }

        _currentRow = newRow;

        _currentColumn = newColumn;

        ItemSlot slot = GetSelectedSlot();

        if (slot != null)
        {
            UIManager.instance.UpdateInventoryPointerLocation(GetSelectedSlot().RelatedCell);

            if (slot.IsOccupied == true)
            {
                UIManager.instance.ShowItemTooltip(slot.StoredItem);
            }
            else if (_isDragging == true)
            {
                UIManager.instance.ShowItemTooltip(_draggedItem);
            }
            else
            {
                UIManager.instance.HideItemTooltip();
            }

            UpdateInventoryUI(GetSelectedSlot().Inventory);
        }
    }

    public ItemInstance DraggedItem => _draggedItem;

    public bool IsDragging => _isDragging;

    public void UpdateInventoryUI(Inventory inventory)
    {
        OnInventoryChanged?.Invoke(inventory);
    }

    public ItemSlot GetSelectedSlot()
    {
        int index;

        Inventory inventory = Player.instance.inventory; // FIXME: make it work with any inventory.

        if (inventory == null || inventory.cells.Count == 0)
        {
            throw new Exception("Wrong inventory capacity or no inventory detected.");
        }

        if (_currentRow < equipRows)
        {
            index = _currentRow * equipColumns + _currentColumn;
        }
        else
        {
            index = equipRows * equipColumns + (_currentRow - equipRows) * bagColumns + _currentColumn;
        }

        if (index >= 0 && index < inventory.cells.Count)
        {
            ItemSlot slot = inventory.slots[index];

            return slot;
        }

        return null;
    }

    public ItemStats GetItemByID(string id)
    {
        return itemDatabase.Find(item => item.ID == id);
    }
    #endregion

    #region ITEM PLACEMENT
    public void PickUpItem(Inventory inventory, Item item)
    {
        if (_isDragging == false)
        {
            _draggedItem = new ItemInstance(item);
        }
        else
        {
            Debug.LogWarning("Already dragging something.");

            return;
        }

        ItemSlot freeSlot = inventory.TryFindAppropriateSlot(item);

        if (freeSlot != null)
        {
            StoreInSlot(freeSlot);
        }
        else
        {
            UIManager.instance.OpenMenu(MenuState.Inventory);

            _isDragging = true;

            Debug.Log($"Begun dragging {_draggedItem.Stats.itemTitle}.");
        }

        UpdateInventoryUI(inventory);
    }

    public void TakeFromSlot(ItemSlot slot)
    {
        if (slot.IsOccupied == false)
        {
            Debug.LogWarning("No item in this cell.");

            return;
        }

        _prevSlot = slot;

        _draggedItem = slot.Inventory.slots[slot.Index].StoredItem;

        slot.FreeSlot();

        _isDragging = true;

        Debug.Log($"Now dragging {_draggedItem.Stats.itemTitle}.");

        UpdateInventoryUI(slot.Inventory);
    }

    public void StoreInSlot(ItemSlot slot)
    {
        if (slot.IsOccupied == false) // If the slot is free just place dragged item in there.
        {
            if (_draggedItem.Stats.itemType == slot.SlotType) // If target slot accepted item type is the same as dragged item.
            {
                if (_prevSlot == null)
                {
                    slot.Inventory.AddItem(_draggedItem, slot.Index);

                    OnItemPickUpConfirmation?.Invoke();
                }
                else
                {
                    slot.Inventory.StoreInSlot(_draggedItem, slot.Index);
                }

                ClearDragData();

                UpdateInventoryUI(slot.Inventory);
            }
            else// But if dragged item type is incorrect.
            {
                SwapBack(_draggedItem);

                Debug.Log("Invalid item type. Try different slot.");
            }
        }
        else // But if slot isn't free.
        {
            if (_draggedItem.Stats.itemType == slot.SlotType) // If target slot accepted item type is the same as dragged item.
            {
                if (slot.StoredItem.Stats.ID == _draggedItem.Stats.ID) // If stored item and dragged item both have the same ID.
                {
                    if (slot.StoredItem.Stats.isStackable == true) // And this type of item is able to stack, then add stacks to target slot and clear dragged item data.
                    {
                        int excess = (_draggedItem.Count + slot.StoredItem.Count) - slot.StoredItem.Stats.maxStacksAmount;

                        if (excess > 0)
                        {
                            slot.StoredItem.SetCount(slot.StoredItem.Stats.maxStacksAmount);

                            _draggedItem.SetCount(excess);

                            if (_prevSlot == null)
                            {
                                slot.Inventory.ChangeCarryWeight((slot.StoredItem.Stats.maxStacksAmount - slot.StoredItem.Count) * _draggedItem.Stats.weight);

                                OnItemPickUpConfirmation?.Invoke();
                            }
                            else
                            {
                                SwapBack(_draggedItem);
                            }
                        }
                        else
                        {
                            slot.StoredItem.SetCount(slot.StoredItem.Count + _draggedItem.Count);

                            if (_prevSlot == null)
                            {
                                slot.Inventory.ChangeCarryWeight(_draggedItem.Count * _draggedItem.Stats.weight);

                                OnItemPickUpConfirmation?.Invoke();
                            }

                            ClearDragData();
                        }

                        UpdateInventoryUI(slot.Inventory);
                    }
                    else // But if this item isn't stackable
                    {
                        SwapBack(_draggedItem);

                        Debug.Log("This item isn't capable of stacking.");
                    }
                }
                else // But if IDs don't match, swap them.
                {
                    SwapItems(slot);
                }
            }
            else
            {
                SwapBack(_draggedItem);

                Debug.Log("Invalid item type. Try different slot.");
            }
        }
    }

    private void SwapItems(ItemSlot targetSlot)
    {
        ItemInstance tempItem = targetSlot.StoredItem;

        targetSlot.Inventory.StoreInSlot(_draggedItem, targetSlot.Index);

        Debug.Log($"Swapped {tempItem.Stats.itemTitle} and {_draggedItem.Stats.itemTitle} ");

        if (_prevSlot != null)
        {
            SwapBack(tempItem);
        }
        else
        {
            _draggedItem = tempItem;
        }

        UpdateInventoryUI(targetSlot.Inventory);
    }

    public void CancelDrag()
    {
        if (_isDragging == false)
        {
            Debug.LogWarning("No dragged item.");

            return;
        }

        SwapBack(_draggedItem);

        ClearDragData();

        UpdateInventoryUI(_prevSlot.Inventory);

        Debug.Log("Drag cancelled.");
    }

    private void ClearDragData()
    {
        _draggedItem = null;
        _prevSlot = null;
        _isDragging = false;
    }
    /// <summary>
    /// Returns item to slot, where it was taken from.
    /// </summary>
    private void SwapBack(ItemInstance item)
    {
        if (_prevSlot != null)
        {
            _prevSlot.Inventory.StoreInSlot(item, _prevSlot.Index);

            int index = _prevSlot.Index;

            if (index < equipRows * equipColumns)
            {
                _currentRow = index / equipColumns;
                _currentColumn = index % equipColumns;
            }
            else
            {
                int bagIndex = index - equipRows * equipColumns;
                _currentRow = equipRows + ((index - equipRows * equipColumns) / bagColumns);
                _currentColumn = (index - equipRows * equipColumns) % bagColumns;
            }

            if (GetSelectedSlot() != null)
            {
                UIManager.instance.UpdateInventoryPointerLocation(GetSelectedSlot().RelatedCell);

                OnInventoryChanged?.Invoke(GetSelectedSlot().Inventory);
            }

            Debug.Log($"{item.Stats.itemTitle} swapped back to {index} slot.");

            UpdateInventoryUI(_prevSlot.Inventory);

            ClearDragData();

        }
        return;
    }
    #endregion
}

[System.Serializable]
public class ItemSlot
{
    [SerializeField] private int _index;
    private Inventory _assignedInventory;
    [SerializeField] private ItemInstance _storedItem;
    [SerializeField] private bool _isOccupied;
    [SerializeField] private ItemSlotType _itemType;
    [SerializeField] private EquipmentSlotType _equipSlotType;

    public void SetupSlot(Inventory inventory, int index)
    {
        _assignedInventory = inventory;

        this._index = index;

        _assignedInventory.cells[_index].SetRelatedSlot(this);
    }

    public InventoryCell RelatedCell => _assignedInventory.cells[_index];
    public int Index => _index;
    public Inventory Inventory => _assignedInventory;
    public ItemInstance StoredItem => _storedItem;
    public bool IsOccupied => _isOccupied;
    public ItemSlotType SlotType => _itemType;
    public EquipmentSlotType EquipmentType => _equipSlotType;

    public void AssignItem(ItemInstance item)
    {
        _storedItem = item;

        _isOccupied = true;

        Debug.Log($"{_storedItem.Stats.itemTitle} was placed in {_index} slot.");
    }

    public void FreeSlot()
    {
        Debug.Log($"{_storedItem.Stats.itemTitle} was taken from {_index} slot.");

        _storedItem = null;

        _isOccupied = false;
    }
}

[System.Serializable]
public class ItemInstance: IItem
{
    [SerializeField] private ItemStats _stats;
    [SerializeField] private int _count;
    [SerializeField] private float _currentDurability;

    public ItemInstance(Item reference)
    {
        _stats = reference.Stats;
        _count = reference.Count;
        _currentDurability = reference.CurrentDurability;
    }

    public ItemStats Stats => _stats;
    public int Count => _count;
    public float CurrentDurability => _currentDurability;
    public void SetCount(int value)
    {
        _count = value;
    }
}
public enum EquipmentSlotType
{
    None,
    HandRight1,
    HandLeft1,
    HandRight2,
    HandLeft2,
    Head,
    Torso,
    Arms,
    Legs,
    Feet,
    RingRight,
    RingLeft,
    Necklace
}

public enum ItemSlotType
{
    Misc,
    Weapon,
    Head,
    Torso,
    Arms,
    Legs,
    Feet,
    Ring,
    Necklace
}