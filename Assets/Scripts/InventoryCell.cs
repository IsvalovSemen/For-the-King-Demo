using TMPro;
using UnityEngine;

public class InventoryCell : MonoBehaviour
{
    [SerializeField] private ItemIcon _icon;
    [SerializeField] private TextMeshProUGUI _stacksCounter;
    [SerializeField] private Sprite _defaultIcon;
    [SerializeField] private ItemSlot _relatedSlot;

    public void SetRelatedSlot(ItemSlot slot)
    {
        _relatedSlot = slot;
    }

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
        if (_relatedSlot.IsOccupied() == true)
        {
            ChangeIcon(_relatedSlot.GetItem().GetStats().sprite);

            UpdateStacksCounter(_relatedSlot.GetItem().GetAmount());
        }
        else
        {
            UpdateStacksCounter(0);

            ClearCell();
        }
    }

    public void ChangeIcon(Sprite sprite)
    {
        _icon.iconImg.sprite = sprite;
    }

    private void UpdateStacksCounter(int amount)
    {
        if (amount > 1)
        {
            _stacksCounter.gameObject.SetActive(true);
        }
        else
        {
            _stacksCounter.gameObject.SetActive(false);
        }

        _stacksCounter.text = amount.ToString();
    }

    public void ClearCell()
    {
        _icon.iconImg.sprite = _defaultIcon;
    }
}
