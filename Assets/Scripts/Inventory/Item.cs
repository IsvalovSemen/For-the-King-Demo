using UnityEngine;

[System.Serializable] public class Item
{
    [SerializeField] private ItemData _data;
    [SerializeField] private int _count;
    [SerializeField] private float _currentDurability;

    public ItemData Data => _data;
    public int Count => _count;
    public float CurrentDurability => _currentDurability;
    public void SetCount(int value) { _count = value; }

    public int Width => rotated ? Data.iconHeight : Data.iconWidth;
    public int Height => rotated ? Data.iconWidth : Data.iconHeight;

    public bool rotated;

    // Grid position.
    public int x;
    public int y;

    public void Rotate()
    {
        rotated = !rotated;
    }
}