using UnityEngine;

public class LootSourceChest : MonoBehaviour, ILootSource
{
    public ContainerDef chestDef;
    ContainerInstance _inst;

    void Awake()
    {
        _inst = new ContainerInstance();
        _inst.Init(chestDef);
        // пример тестового наполнения в Start по желанию
    }

    public ContainerInstance Open() => _inst;

    public void TakeStackAt(int index, int qty)
    {
        if (index < 0 || index >= _inst.count) return;
        var gi = _inst.items[index];
        gi.stack.qty -= qty;
        if (gi.stack.qty <= 0) _inst.RemoveAt(index);
        else _inst.items[index] = gi;
    }
}
