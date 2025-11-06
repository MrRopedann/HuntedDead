using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public CharacterStats stats;
    public List<Item> items = new();

    public void AddItem(Item item)
    {
        items.Add(item);
        item.Apply(stats);
        Debug.Log($"Поднят предмет: {item.itemName}");
    }

    public void RemoveItem(Item item)
    {
        if (items.Remove(item))
            item.Remove(stats);
    }
}
