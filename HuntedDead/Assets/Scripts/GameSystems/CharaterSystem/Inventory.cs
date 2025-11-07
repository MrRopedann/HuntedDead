using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public CharacterStats stats;
    public List<Item> items = new();

    [Header("Resources")]
    public int scrap = 0; // Количество скрапа у игрока

    public void AddItem(Item item)
    {
        items.Add(item);
        item.Apply(stats);
        Debug.Log($"Поднят предмет: {item.itemName}");
    }

    // Добавить скрап
    public void AddScrap(int amount)
    {
        scrap += amount;
        Debug.Log($"Получено {amount} скрапа. Всего: {scrap}");
    }

    // Потратить скрап
    public bool SpendScrap(int amount)
    {
        if (scrap >= amount)
        {
            scrap -= amount;
            Debug.Log($"Потрачено {amount} скрапа. Осталось: {scrap}");
            return true;
        }
        else
        {
            Debug.Log("Недостаточно скрапа!");
            return false;
        }
    }


    public void RemoveItem(Item item)
    {
        if (items.Remove(item))
            item.Remove(stats);
    }
}
