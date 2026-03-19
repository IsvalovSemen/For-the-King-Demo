using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class ItemPreview : MonoBehaviour
{
    [SerializeField] protected Image _iconImg;
    public Image IconImg => _iconImg;
    [SerializeField] protected TextMeshProUGUI _stacksCounter;
    [SerializeField] protected TextMeshProUGUI _itemTitle;

    public void Init(Item item)
    {
        SetIconImage(item.Data.iconSprite);

        UpdateStacksCounter(item.Count);

        SetTitle(item.Data.itemTitle);
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

    public void SetTitle(string title)
    {
        _itemTitle.text = title;
    }

    public void DestroyPreview()
    {
        Destroy(this.gameObject);
    }
}
