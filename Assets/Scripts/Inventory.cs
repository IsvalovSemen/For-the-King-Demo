using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor.PackageManager;
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
    /// <summary>
    /// Returns first free appropriate slot in inventory for this item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ItemSlot TryFindAppropriateSlot(IItem item)
    {
        for (int i = 0; i < slots.Count;i++)
        {
            if (slots[i].SlotType == item.Stats.itemType && slots[i].IsOccupied == false)
            {
                return slots[i];
            }
        }

        return null;
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

        if (slots[slotIndex].EquipmentType != EquipmentSlotType.None)
        {
            EquipItem(slots[slotIndex].EquipmentType, item, slotIndex);
        }
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
        if (slots[slotIndex].EquipmentType != EquipmentSlotType.None)
        {
            UnequipItem(slots[slotIndex].EquipmentType, slots[slotIndex].StoredItem);
        }

        slots[slotIndex].FreeSlot();
    }

    private void UnequipItem(EquipmentSlotType EquipmentType, ItemInstance item)
    {
        Debug.Log($"{item.Stats.itemTitle} was unequipped from {EquipmentType} slot.");

        equipment[EquipmentType] = null;
    }

    public void DropItem(int slotIndex, bool viaCursor)
    {
        RaycastHit hitInfo;

        if (viaCursor == true)
        {
            Ray ray = CameraControl.instance.mainCam.ScreenPointToRay(Input.mousePosition);

            Physics.Raycast(ray, out hitInfo, GameMaster.instance.interactionDistance, LayerMask.GetMask("Environment"));

        }
        else
        {
            Ray ray = new Ray(CameraControl.instance.mainCam.transform.position, CameraControl.instance.mainCam.transform.forward);

            Physics.Raycast(ray, out hitInfo, GameMaster.instance.interactionDistance, LayerMask.GetMask("Environment"));

        }

        GameObject createdItem = Instantiate(slots[slotIndex].StoredItem.Stats.prefab, hitInfo.point, Quaternion.identity); // Spawns a prefab of dropped item in the point camera facing.


        createdItem.GetComponent<Item>().SetCount(slots[slotIndex].StoredItem.Count);

        Debug.Log($"{slots[slotIndex].StoredItem.Stats.itemTitle} was dropped from {transform.GetComponentInParent<Creature>().gameObject.name} inventory.");

        ChangeCarryWeight(-slots[slotIndex].StoredItem.Stats.weight * slots[slotIndex].StoredItem.Count);

        RemoveFromSlot(slotIndex);

        InventoryManager.instance.UpdateInventoryUI(this);
    }

    public bool IsEnoughSpace()
    {
        if (slots.Count >= _maxSlots) return false;
        else return true;
    }
}