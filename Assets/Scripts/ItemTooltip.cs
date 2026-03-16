using UnityEngine;
using UnityEngine.UI;

public class ItemTooltip : MonoBehaviour
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Text _ItemTitle;
    [SerializeField] private Text _itemWeight;
    [SerializeField] private Text _itemPrice;
    [SerializeField] private Slider _durabilityMeter;
    [SerializeField] private Text _durabilityRatio;
    [SerializeField] private Text _itemDescription;

    public void SetValues(Item item)
    {
        _itemIcon.sprite = item.Stats.iconSprite;
        _ItemTitle.text = item.Stats.itemTitle;
        _itemWeight.text = item.Stats.weight.ToString();
        _itemPrice.text = item.Stats.price.ToString();
        _durabilityMeter.maxValue = item.Stats.maxDurability;
        _durabilityMeter.value = item.CurrentDurability;
        _durabilityRatio.text = $"{item.CurrentDurability}/{item.Stats.maxDurability}";
        _itemDescription.text = item.Stats.description;
    }

    public void ClearValues()
    {
        _itemIcon.sprite = null;
        _ItemTitle.text = string.Empty;
        _itemWeight.text = string.Empty;
        _itemPrice.text = string.Empty;
        _durabilityMeter.maxValue = 100;
        _durabilityMeter.value = 0;
        _durabilityRatio.text = string.Empty;
        _itemDescription.text = string.Empty;
    }
}
