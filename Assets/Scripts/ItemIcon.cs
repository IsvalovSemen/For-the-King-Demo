using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ItemIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler //Need to add for OnPointerEnter and OnPointerExit to work
{
    public Image iconImg;
    [HideInInspector] public Transform originalParent;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    [SerializeField] public TextMeshProUGUI stacksCounter;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        UpdateStackView(1, 1);
    }
    /// <summary>
    /// Updates stack UI visibility and value.
    /// Shows stack counter only if currentStacks > 1.
    /// </summary>
    public void UpdateStackView(int currentStacks, int maxStacks)
    {
        if (currentStacks > 1)
        {
            stacksCounter.gameObject.SetActive(true);
            stacksCounter.text = currentStacks.ToString();
        }
        else
        {
            stacksCounter.gameObject.SetActive(false);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;

        transform.SetParent(canvas.transform, true);

        canvasGroup.blocksRaycasts = false;

        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        canvasGroup.alpha = 1f;

        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent);

            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}