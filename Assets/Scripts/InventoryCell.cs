using System;
using TMPro;
using UnityEngine;

public class InventoryCell : MonoBehaviour
{
    [SerializeField] private ItemIcon _icon;
    [SerializeField] private Sprite _defaultIcon;
    [SerializeField] private ItemSlot _relatedSlot;

    private void Start()
    {
        _icon.ConnectCell(this);
    }

    public void SetRelatedSlot(ItemSlot slot)
    {
        _relatedSlot = slot;
    }

    public ItemSlot RelatedSlot => _relatedSlot;

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

        if (InventoryManager.instance.IsDragging == true && InventoryManager.instance.GetSelectedSlot() == _relatedSlot)
        {
            _icon.SetIconImage(InventoryManager.instance.DraggedItem.Stats.iconSprite);

            _icon.UpdateStacksCounter(InventoryManager.instance.DraggedItem.Count);
        }
        else
        {
            if (_relatedSlot.IsOccupied == true)
            {
                _icon.UpdateStacksCounter(_relatedSlot.StoredItem.Count);

                _icon.SetIconImage(_relatedSlot.StoredItem.Stats.iconSprite);
            }
            else
            {
                _icon.UpdateStacksCounter(0);

                _icon.SetIconImage(_defaultIcon);
            }
        }
    }
}
