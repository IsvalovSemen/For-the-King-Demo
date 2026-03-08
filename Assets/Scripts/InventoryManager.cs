using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UIManager;
using static UnityEditor.Progress;
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
    private ItemInstance draggedItem;
    private bool isDragging = false;
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
        if (UIManager.instance.GetCurrentMenu() == MenuState.Inventory)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) MovePointer(Vector2Int.up);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) MovePointer(Vector2Int.down);
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) MovePointer(Vector2Int.left);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) MovePointer(Vector2Int.right);

            ItemSlot selectedCell = GetSelectedSlot();

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!isDragging)
                {
                    TakeFromSlot(selectedCell);
                }
                else
                {
                    StoreInSlot(selectedCell);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) && isDragging)
            {
                CancelDrag();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (selectedCell.IsOccupied() == true)
                {
                    DropItem(selectedCell.GetItem());

                    selectedCell.ClearSLot();

                    OnInventoryChanged?.Invoke(selectedCell.GetInventory());
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

        if (GetSelectedSlot() != null)
        {
            UIManager.instance.UpdateInventoryPointerLocation(GetSelectedSlot().GetRelatedCell());

            OnInventoryChanged?.Invoke(GetSelectedSlot().GetInventory());
        }
    }

    public ItemSlot GetSelectedSlot()
    {
        int index;

        Inventory inventory = Player.instance.inventory; // FIXME: make it work with any inventory.

        if (inventory == null || inventory.cells.Count == 0)
        {
            throw new System.Exception("Wrong inventory capacity or no inventory detected.");
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

            if (isDragging == true)
            {
                slot.GetRelatedCell().ChangeIcon(draggedItem.GetStats().sprite); // Shows dragged item icon in the cell where pointer is located.
            }

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
        if (isDragging == false)
        {
            draggedItem = new ItemInstance(item);
        }
        else
        {
            Debug.LogWarning("Already dragging something.");

            return;
        }

        UIManager.instance.OpenMenu(MenuState.Inventory);

        isDragging = true;

        Debug.Log($"Begun dragging {draggedItem.GetStats().itemTitle}.");

        OnInventoryChanged?.Invoke(inventory);
    }

    private void TakeFromSlot(ItemSlot slot)
    {
        if (slot.IsOccupied() == false)
        {
            Debug.LogWarning("No item in this cell.");

            return;
        }

        _prevSlot = slot;

        draggedItem = slot.GetInventory().slots[slot.GetIndex()].GetItem();

        slot.ClearSLot();

        isDragging = true;

        Debug.Log($"Now dragging {draggedItem.GetStats().itemTitle}.");

        OnInventoryChanged?.Invoke(slot.GetInventory());
    }

    private void StoreInSlot(ItemSlot slot)
    {
        if (!isDragging) // If no dragged item, than abort placement.
        {
            Debug.LogWarning("Failed to place item in cell: no item dragged.");

            return;
        }

        if (slot.IsOccupied() == false) // If the slot is free just place dragged item in there.
        {
            if (draggedItem.GetStats().itemType == slot.GetSlotType()) // Check if target slot accepted item type is the same as dragged item.
            {
                slot.AssignItem(draggedItem);

                OnItemPickUpConfirmation?.Invoke();

                ClearDragData();

                OnInventoryChanged?.Invoke(slot.GetInventory());
            }
            else// But if dragged item type is incorrect.
            {
                SwapBack(draggedItem);

                Debug.Log("Invalid item type. Try different slot.");
            }
        }
        else // But if slot isn't free.
        {
            if (draggedItem.GetStats().itemType == slot.GetSlotType()) // If target slot accepted item type is the same as dragged item.
            {
                if (slot.GetItem().GetStats().ID == draggedItem.GetStats().ID) // If stored item and dragged item both have the same ID.
                {
                    if (slot.GetItem().GetStats().isStackable == true) // And this type of item is able to stack, then add stacks to target slot and clear dragged item data.
                    {
                        int excess = (draggedItem.GetAmount() + slot.GetItem().GetAmount()) - slot.GetItem().GetStats().maxStacksAmount;
                        Debug.LogWarning("test: " + excess);
                        if (excess > 0)
                        {
                            slot.GetItem().SetAmount(slot.GetItem().GetStats().maxStacksAmount);

                            draggedItem.SetAmount(excess);
                        }
                        else
                        {
                            slot.GetItem().SetAmount(slot.GetItem().GetAmount() + draggedItem.GetAmount());

                            if (_prevSlot != null) OnItemPickUpConfirmation?.Invoke();

                            ClearDragData();
                        }

                        OnInventoryChanged?.Invoke(slot.GetInventory());
                    }
                    else // But if this item isn't stackable
                    {
                        SwapBack(draggedItem);

                        Debug.Log("This item isn't capable of stacking.");

                        OnItemPickUpConfirmation?.Invoke();
                    }
                }
                else // But if IDs don't match, swap them.
                {
                    SwapItems(slot);

                    OnItemPickUpConfirmation?.Invoke();
                }
            }
            else
            {
                SwapBack(draggedItem);

                Debug.Log("Invalid item type. Try different slot.");
            }
        }
    }

    private void SwapItems(ItemSlot targetSLot)
    {
        ItemInstance tempItem = targetSLot.GetItem();

        targetSLot.AssignItem(draggedItem);

        Debug.Log($"Swapped {tempItem.GetStats().itemTitle} and {draggedItem.GetStats().itemTitle} ");

        if (_prevSlot != null)
        {
            SwapBack(tempItem);
        }
        else
        {
            draggedItem = tempItem;
        }

        OnInventoryChanged?.Invoke(targetSLot.GetInventory());
    }

    public void CancelDrag()
    {
        if (isDragging == false)
        {
            Debug.LogWarning("No dragged item.");

            return;
        }

        SwapBack(draggedItem);

        ClearDragData();

        OnInventoryChanged?.Invoke(_prevSlot.GetInventory());

        Debug.Log("Drag cancelled.");
    }

    private void ClearDragData()
    {
        draggedItem = null;
        _prevSlot = null;
        isDragging = false;
    }
    /// <summary>
    /// Returns item to slot, where it was taken from.
    /// </summary>
    private void SwapBack(ItemInstance item)
    {
        if (_prevSlot != null)
        {
            _prevSlot.AssignItem(item);

            int index = _prevSlot.GetIndex();

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
                UIManager.instance.UpdateInventoryPointerLocation(GetSelectedSlot().GetRelatedCell());

                OnInventoryChanged?.Invoke(GetSelectedSlot().GetInventory());
            }

            Debug.Log($"{item.GetStats().itemTitle} swapped back to {index} slot.");

            OnInventoryChanged?.Invoke(_prevSlot.GetInventory());

            ClearDragData();

        }
        return;
    }
    /// <summary>
    /// Creates prefab of dropped item in the scene.
    /// </summary>
    /// <param name="item"></param>
    private void DropItem(ItemInstance item)
    {
        GameObject droppedGO = Instantiate(item.GetStats().prefab, Player.instance.transform.position + Player.instance.transform.forward, Quaternion.identity);

        Debug.Log($"{item.GetStats().itemTitle} was dropped on the ground.");
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

    public InventoryCell GetRelatedCell() => _assignedInventory.cells[_index];
    public int GetIndex() => _index;
    public Inventory GetInventory() => _assignedInventory;
    public ItemInstance GetItem() => _storedItem;
    public bool IsOccupied() => _isOccupied;
    public ItemSlotType GetSlotType() => _itemType;
    public EquipmentSlotType GetEquipmentSlotType() => _equipSlotType;

    public void AssignItem(ItemInstance item)
    {
        _storedItem = item;

        _isOccupied = true;

        if (_equipSlotType != EquipmentSlotType.None)
        {
            EquipItem();
        }

        Debug.Log($"{_storedItem.GetStats().itemTitle} was placed in {_index} slot.");
    }

    private void EquipItem()
    {
        _assignedInventory.equipment[_equipSlotType] = _storedItem;

        Debug.Log($"{_storedItem.GetStats().itemTitle} was equipped in {_equipSlotType} slot.");
    }

    public void ClearSLot()
    {
        if (_equipSlotType != EquipmentSlotType.None)
        {
            UnequipItem();
        }

        Debug.Log($"{_storedItem.GetStats().itemTitle} was taken from {_index} slot.");

        _storedItem = null;

        _isOccupied = false;
    }

    private void UnequipItem()
    {
        _assignedInventory.equipment[_equipSlotType] = null;

        Debug.Log($"{_storedItem.GetStats().itemTitle} was unequipped from {_equipSlotType} slot.");
    }
}

[System.Serializable]
public class ItemInstance
{
    [SerializeField] private ItemStats _stats;
    [SerializeField] private int _amount;
    [SerializeField] private float _currentDurability;

    public ItemInstance(Item reference)
    {
        _stats = reference.stats;
        _amount = reference.amount;
        _currentDurability = reference.currentDurability;
    }

    public ItemStats GetStats() => _stats;
    public int GetAmount() => _amount;
    public float GetDurability() => _currentDurability;
    public void SetAmount(int value)
    {
        _amount = value;
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