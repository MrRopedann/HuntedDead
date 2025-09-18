// TestSeeder.cs
using UnityEngine;

[DefaultExecutionOrder(2000)]               // запускаем ПОСЛЕ PlayerInventory/LootSourceChest
public class TestSeeder : MonoBehaviour
{
    public PlayerInventory inv;
    public DbRegistry db;

    void Start()
    {
        EnsureInit();

        Seed(inv.Pockets, "wood", 12);
        Seed(inv.Pockets, "water_bottle", 3);
        Seed(inv.Pockets, "medkit", 1);

        // Форс-перерисовка открываемой вкладки
        var panel = FindObjectOfType<InventoryPanel>(includeInactive: true);
        if (panel && panel.bound != null) panel.RedrawItems();
    }

    void EnsureInit()
    {
        if (inv.Pockets.def == null) inv.Pockets.Init(inv.pocketsDef);
        if (inv.Backpack.def == null) inv.Backpack.Init(inv.backpackDef);
        if (inv.vestDef && inv.Vest.def == null) inv.Vest.Init(inv.vestDef);
    }

    void Seed(ContainerInstance c, string id, int qty)
    {
        var def = db.ItemByGuid(id);
        if (!def) { Debug.LogError($"ItemDef '{id}' не найден"); return; }

        var gi = new GridItem
        {
            def = def,
            size = def.is3D ? def.size3D : def.size2D,
            rotated = false,
            stack = new ItemStack
            {
                key = new VariantKey { itemGuid = def.id.id, tier = QualityTier.Common },
                qty = qty
            }
        };

        if (!Placement.TryPlace(c, ref gi, out _, out _))
            Debug.LogWarning($"Нет места для {id} x{qty} в {c.def?.displayName}");
    }
}
