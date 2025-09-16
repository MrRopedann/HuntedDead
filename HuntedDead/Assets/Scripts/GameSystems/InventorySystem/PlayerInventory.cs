using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public ContainerDef pocketsDef;
    public ContainerDef backpackDef;
    public ContainerDef vestDef;
    public ContainerInstance Pockets = new();
    public ContainerInstance Backpack = new();
    public ContainerInstance Vest = new();
    public Equipment Equipment = new();

    void Awake()
    {
        if (pocketsDef) Pockets.Init(pocketsDef);
        if (backpackDef) Backpack.Init(backpackDef);
        if (vestDef) Vest.Init(vestDef);
    }
}
