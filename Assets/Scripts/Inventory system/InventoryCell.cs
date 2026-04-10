using System;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class InventoryCell : MonoBehaviour
{
    [SerializeField] private ItemIcon _icon;
    [SerializeField] private Sprite _defaultIcon;
    [SerializeField] private int _relatedSlotIndex;
    public int index {  get; private set; }

    private void Start()
    {
        _icon.ConnectCell(this);
    }

    public void SetRelatedSlot(int index)
    {
        _relatedSlotIndex = index;
    }

    public int RelatedSlotIndex => _relatedSlotIndex;

    public void UpdateCellView()
    {
        /*
        if (InventoryManager.instance.IsDraggingFrom(_assignedInventory, index)) // If dragging item from this cell, still shows that item icon on this cell.
        {
            ShowIcon(InventoryManager.instance.GetDraggedItem());

            return;
        }
        
        if (InventoryManager.instance.IsDragging() == true && InventoryManager.instance.GetSelectedCell() == this) // Shows dragged item icon in the cell where pointer is located.
        {
            ShowIcon(InventoryManager.instance.GetDraggedItem());

            return;
        }
        */
        /*
        if (InventoryManager.instance.IsDragging == true && InventoryManager.instance.GetSelectedSlot() == _relatedSlotIndex)
        {
            _icon.SetIconImage(InventoryManager.instance.DraggedItem.Data.iconSprite);

            _icon.UpdateStacksCounter(InventoryManager.instance.DraggedItem.Count);
        }
        else
        {
            if (Player.instance.inventory.items[_relatedSlotIndex].Count > 0)
            {
                _icon.UpdateStacksCounter(Player.instance.inventory.items[_relatedSlotIndex].Count);

                _icon.SetIconImage(Player.instance.inventory.items[_relatedSlotIndex].Data.iconSprite);
            }
            else
            {
                _icon.UpdateStacksCounter(0);

                _icon.SetIconImage(_defaultIcon);
            }
        }*/
    }
}
