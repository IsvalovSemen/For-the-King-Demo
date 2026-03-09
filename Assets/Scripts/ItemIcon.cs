using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler //Need to add for OnPointerEnter and OnPointerExit to work
{
    [SerializeField] private Image _iconImg;
    [SerializeField] private TextMeshProUGUI _stacksCounter;
    [SerializeField] private InventoryCell _relatedCell;
    [SerializeField] private Transform _originalParent;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _isHoveringOver = false;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        _rectTransform = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (_relatedCell.RelatedSlot.IsOccupied == true && Input.GetMouseButtonDown(1))
        {
            _relatedCell.RelatedSlot.Inventory.DropItem(_relatedCell.RelatedSlot.Index);
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

    public void SetIconImage(Sprite sprite)
    {
        _iconImg.sprite = sprite;
    }
    /// <summary>
    /// Updates stack UI visibility and value.
    /// Shows stack counter only if currentStacks > 1.
    /// </summary>
    public void UpdateStacksCounter(int amount)
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHoveringOver = true;

        if (_relatedCell != null && _relatedCell.RelatedSlot.IsOccupied == true) UIManager.instance.ShowItemTooltip(_relatedCell.RelatedSlot.StoredItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHoveringOver = false;

        UIManager.instance.HideItemTooltip();
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
        GameObject target = eventData.pointerEnter;

        if (target != null && target.layer == 5 && target.GetComponent<ItemIcon>() != null)
        {
            InventoryManager.instance.TakeFromSlot(_originalParent.GetComponent<InventoryCell>().RelatedSlot);

            InventoryManager.instance.StoreInSlot(target.GetComponentInParent<InventoryCell>().RelatedSlot);
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