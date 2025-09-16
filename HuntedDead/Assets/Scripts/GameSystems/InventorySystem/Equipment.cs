using System.Collections.Generic;

public class Equipment
{
    readonly Dictionary<string, GridItem> _slots = new(8);

    public bool CanEquip(ItemDef def, string slot)
    {
        if (!def.equippable) return false;
        var arr = def.validEquipSlots;
        if (arr == null) return false;
        for (int i = 0; i < arr.Length; i++) if (arr[i] == slot) return true;
        return false;
    }

    public bool TryEquip(string slot, GridItem item)
    {
        if (_slots.ContainsKey(slot)) return false;
        _slots[slot] = item; return true;
    }

    public bool TryUnequip(string slot, out GridItem item)
    {
        if (_slots.TryGetValue(slot, out item)) { _slots.Remove(slot); return true; }
        return false;
    }

    public bool TryGet(string slot, out GridItem item) => _slots.TryGetValue(slot, out item);
}
