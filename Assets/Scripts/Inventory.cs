using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEngine.Rendering.DebugUI;

public class Inventory : MonoBehaviour
{
    public List<ItemSlot> slots = new List<ItemSlot>();
    [SerializeField] private int _maxSlots = 100;
    public Dictionary<EquipmentSlotType, ItemInstance> equipment = new Dictionary<EquipmentSlotType, ItemInstance>();
    public List<InventoryCell> cells { get; set; }
    [SerializeField] private GameObject _inventoryCanvas;
    public event Action<float> OnEquiploadChange;

    private void Awake()
    {
        cells = _inventoryCanvas.GetComponentsInChildren<InventoryCell>(true).ToList();

        _maxSlots = cells.Count + 1;
    }
    private void Start()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            slots[i].SetupSlot(this, i);
        }
    }

    private void OnEnable()
    {
        InventoryManager.instance.OnInventoryChanged += UpdateCells;
    }

    private void OnDisable()
    {
        InventoryManager.instance.OnInventoryChanged -= UpdateCells;
    }

    private void UpdateCells(Inventory inventory)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].UpdateCellView();
        }
    }

    public void AddItem(ItemInstance item, int slotIndex)
    {
        StoreInSlot(item, slotIndex);

        ChangeCarryWeight(item.Stats.weight * item.Count);

        Debug.Log($"{item.Stats.itemTitle} was stored in {transform.GetComponentInParent<Creature>().gameObject.name} inventory.");
    }

    public void StoreInSlot(ItemInstance item, int slotIndex)
    {
        slots[slotIndex].AssignItem(item);

        if (slots[slotIndex].EquipmentType != EquipmentSlotType.None) EquipItem(slots[slotIndex].EquipmentType, item, slotIndex);
    }

    private void EquipItem(EquipmentSlotType EquipmentType, ItemInstance item, int slotIndex)
    {
        equipment[EquipmentType] = item;

        Debug.Log($"{item.Stats.itemTitle} was equipped in {EquipmentType} slot.");
    }

    public void ChangeCarryWeight(float value)
    {
        OnEquiploadChange?.Invoke(value);
    }

    private void RemoveFromSlot(int slotIndex)
    {
        if (slots[slotIndex].EquipmentType != EquipmentSlotType.None) UnequipItem(slots[slotIndex].EquipmentType, slots[slotIndex].StoredItem);

        slots[slotIndex].FreeSlot();
    }

    private void UnequipItem(EquipmentSlotType EquipmentType, ItemInstance item)
    {
        Debug.Log($"{item.Stats.itemTitle} was unequipped from {EquipmentType} slot.");

        equipment[EquipmentType] = null;
    }
    /// <summary>
    ///  Creates prefab of dropped item in the scene.
    /// </summary>
    /// <param name="slotIndex"></param>
    public void DropItem(int slotIndex)
    {
        GameObject createdItem = Instantiate(slots[slotIndex].StoredItem.Stats.prefab, Player.instance.transform.position + Player.instance.transform.forward, Quaternion.identity);

        createdItem.GetComponent<Item>().SetCount(slots[slotIndex].StoredItem.Count);

        Debug.Log($"{slots[slotIndex].StoredItem.Stats.itemTitle} was dropped from {transform.GetComponentInParent<Creature>().gameObject.name} inventory.");

        ChangeCarryWeight(-slots[slotIndex].StoredItem.Stats.weight * slots[slotIndex].StoredItem.Count);

        RemoveFromSlot(slotIndex);
    }

    public bool IsEnoughSpace()
    {
        if (slots.Count >= _maxSlots) return false;
        else return true;
    }
}