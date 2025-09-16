using System.Collections.Generic;
using UnityEngine;

public class DbRegistry : MonoBehaviour
{
    public ItemDef[] allItems;
    public ContainerDef[] allContainers;
    readonly Dictionary<string, ItemDef> _id2item = new();
    readonly Dictionary<string, ContainerDef> _name2cont = new();

    void Awake()
    {
        for (int i = 0; i < allItems.Length; i++) _id2item[allItems[i].id.id] = allItems[i];
        for (int i = 0; i < allContainers.Length; i++) _name2cont[allContainers[i].name] = allContainers[i];
    }
    public ItemDef ItemByGuid(string g) => _id2item[g];
    public ContainerDef ContainerByName(string n) => _name2cont[n];
}
