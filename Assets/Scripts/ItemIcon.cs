using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemIcon : ItemPreview, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler //Need to add for OnPointerEnter and OnPointerExit to work
{
    [SerializeField] private InventoryCell _relatedCell;
    [SerializeField] private Transform _originalParent;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private CanvasGroup _canvasGroup;
    private bool _isHoveringOver;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        _rectTransform = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (_isHoveringOver == true && Input.GetMouseButtonDown(0) && DoubleClick.IsDoubleClick() == true && _relatedCell.RelatedSlot.IsOccupied == true)
        {
            Debug.LogWarning("test");

            _relatedCell.RelatedSlot.Inventory.DropItem(_relatedCell.RelatedSlot.Index, false);
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
        if (_relatedCell != null && _relatedCell.RelatedSlot.IsOccupied == true)
        {
            UIManager.instance.ShowItemTooltip(_relatedCell.RelatedSlot.StoredItem);
        }

        _isHoveringOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_relatedCell != null && _relatedCell.RelatedSlot.IsOccupied == true)
        {
            UIManager.instance.HideItemTooltip();
        }

        _isHoveringOver = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_relatedCell.RelatedSlot.IsOccupied == true)
        {
            _originalParent = transform.parent;

            transform.SetParent(_canvas.transform, true);

            MakeTransparent();

            _canvasGroup.alpha = 0.8f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_relatedCell.RelatedSlot.IsOccupied == true)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_relatedCell.RelatedSlot.IsOccupied == true)
        {
            GameObject target = eventData.pointerEnter;

            if (target != null)
            {
                if (target.GetComponent<ItemIcon>() != null)
                {
                    InventoryManager.instance.TakeFromSlot(_originalParent.GetComponent<InventoryCell>().RelatedSlot);

                    InventoryManager.instance.StoreInSlot(target.GetComponentInParent<InventoryCell>().RelatedSlot);
                }
            }
            else
            {
                _originalParent.GetComponent<InventoryCell>().RelatedSlot.Inventory.DropItem(_originalParent.GetComponent<InventoryCell>().RelatedSlot.Index, true);
            }

            MakeInteractable();

            _canvasGroup.alpha = 1f;

            if (transform.parent == _canvas.transform)
            {
                transform.SetParent(_originalParent);

                _rectTransform.anchoredPosition = Vector2.zero;
            }
        }

    }
}