using UnityEngine;

[DefaultExecutionOrder(1500)]
public class ChestSeeder : MonoBehaviour
{
    public LootSourceChest chest;
    public DbRegistry db;

    [System.Serializable] public struct Entry { public string id; public int qty; }
    public Entry[] items = {
        new Entry{ id="wood", qty=10 },
        new Entry{ id="water_bottle", qty=3 }
    };

    void Start()
    {
        if (!chest) chest = GetComponent<LootSourceChest>();
        if (!chest || !db) { Debug.LogError("ChestSeeder: нет ссылок chest/db", this); return; }

        var c = chest.Open();
        for (int i = 0; i < items.Length; i++)
        {
            var e = items[i];
            var def = db.ItemByGuid(e.id);
            if (!def) { Debug.LogError($"Нет ItemDef '{e.id}'", this); continue; }

            var gi = new GridItem
            {
                def = def,
                size = def.is3D ? def.size3D : def.size2D,
                rotated = false,
                stack = new ItemStack
                {
                    key = new VariantKey { itemGuid = def.id.id, tier = QualityTier.Common },
                    qty = e.qty
                }
            };
            Placement.TryPlace(c, ref gi, out _, out _);
        }
    }
}
