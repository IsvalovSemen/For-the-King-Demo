using UnityEngine;

[System.Serializable] public class Item
{
    [SerializeField] private ItemStats _stats;
    [SerializeField] private int _count;
    [SerializeField] private float _currentDurability;

    public ItemStats Stats => _stats;
    public int Count => _count;
    public float CurrentDurability => _currentDurability;
    public void SetCount(int value) { _count = value; }
}

public enum EquipmentSlotType
{
    None,
    HandRight1,
    HandLeft1,
    HandRight2,
    HandLeft2,
    Head,
    Torso,
    Arms,
    Legs,
    Feet,
    RingRight,
    RingLeft,
    Necklace
}

public enum ItemSlotType
{
    Misc,
    Weapon,
    Head,
    Torso,
    Arms,
    Legs,
    Feet,
    Ring,
    Necklace
}