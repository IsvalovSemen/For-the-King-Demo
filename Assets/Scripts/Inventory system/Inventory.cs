using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int _capacity = 10;
    public List<Item> items = new List<Item>();
    public event Action<float> OnItemAdd;
    public event Action<float> OnItemRemove;

    private void Awake()
    {
        //items = new List<Item>(new Item[_capacity]);
    }

    public bool SearchForItem(string itemID)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].Data.ID == itemID)
            {
                return true;
            }
        }

        return false;
    }

    public void AddItem(Item item, int index)
    {
        if (index <= _capacity)
        {
            items[index] = item;

            OnItemAdd?.Invoke(item.Data.weight);

            Debug.Log($"{item.Data.itemTitle} was stored in {index} slot of {transform.GetComponentInParent<Creature>().gameObject.name}'s inventory.");
        }
        else Debug.LogWarning("Wrong slot index.", this.gameObject);
    }

    public void RemoveItem(int index)
    {
        Item item = items[index];

        items[index] = null;

        OnItemRemove.Invoke(item.Data.weight);

        Debug.Log($"{item.Data.itemTitle} was removed from {index} slot of {transform.GetComponentInParent<Creature>().gameObject.name}'s inventory.");
    }
}