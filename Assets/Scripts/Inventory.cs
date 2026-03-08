using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemSlot> slots = new List<ItemSlot>();
    [SerializeField] private int _maxSlots = 100;
    public Dictionary<EquipmentSlotType, ItemInstance> equipment = new Dictionary<EquipmentSlotType, ItemInstance>();
    public List<InventoryCell> cells { get; set; }
    [SerializeField] private GameObject _inventoryCanvas;

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

    public bool IsEnoughSpace()
    {
        if (slots.Count >= _maxSlots) return false;
        else return true;
    }

    public void AddItem(ItemInstance itemInstance, int index)
    {
        slots[index].AssignItem(itemInstance);
    }
}