using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Progress;
using System.Runtime.CompilerServices;
using UnityEditor.Overlays;

public class ItemIcon : ItemPreview, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler //Need to add for OnPointerEnter and OnPointerExit to work
{
    [SerializeField] private Item _linkedItem;
    [SerializeField] private InventoryCell _relatedCell;
    [SerializeField] private Transform _originalParent;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _frameRect;
    [SerializeField] private CanvasGroup _canvasGroup;

    private bool _isHoveringOver;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        _frameRect = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (_isHoveringOver == true && Input.GetMouseButtonDown(0) && DoubleClick.IsDoubleClick() == true)
        {
            InventoryManager.instance.DropItem(_linkedItem, false);
        }
    }

    public void BindItem(Item item)
    {
        _linkedItem = item;

        _iconImg.sprite = item.Data.iconSprite;
    }

    public void UpdateVisuals()
    {
        _iconImg.rectTransform.anchoredPosition = _frameRect.sizeDelta / 2;

        _iconImg.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (_linkedItem.rotated == true)
        {
            _iconImg.rectTransform.localRotation = Quaternion.Euler(0, 0, 90);

            _iconImg.rectTransform.sizeDelta = new Vector2(_frameRect.sizeDelta.y, _frameRect.sizeDelta.x);
        }
        else
        {
            _iconImg.rectTransform.localRotation = Quaternion.identity;

            _iconImg.rectTransform.sizeDelta = _frameRect.sizeDelta;
        }
    }

    public void ConnectCell(InventoryCell cell)
    {
        _relatedCell = cell;
    }

    public void MakeInteractable()
    {
        _canvasGroup.blocksRaycasts = true;
    }

    public void MakeTransparent()
    {
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.instance.ShowItemTooltip(Player.instance.inventory.items[_relatedCell.RelatedSlotIndex]);

        _isHoveringOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.instance.HideItemTooltip();

        _isHoveringOver = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalParent = transform.parent;

        transform.SetParent(_canvas.transform, true);

        MakeTransparent();

        _canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _frameRect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GameObject target = eventData.pointerEnter;

        if (target != null)
        {
            if (target.GetComponent<ItemIcon>() != null)
            {
                //InventoryManager.instance.TakeFromSlot(_originalParent.GetComponent<InventoryCell>().RelatedSlotIndex);

                //InventoryManager.instance.StoreInSlot(Player.instance.inventory.items[_relatedCell.RelatedSlotIndex], target.GetComponentInParent<InventoryCell>().RelatedSlotIndex);
            }
        }
        else
        {
            InventoryManager.instance.DropItem(_linkedItem, true);
        }

        MakeInteractable();

        _canvasGroup.alpha = 1f;

        if (transform.parent == _canvas.transform)
        {
            transform.SetParent(_originalParent);

            _frameRect.anchoredPosition = Vector2.zero;
        }
    }

}