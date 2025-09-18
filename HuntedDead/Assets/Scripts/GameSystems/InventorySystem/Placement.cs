using UnityEngine;

public static class Placement
{
    /// Доложить в уже существующие стаки контейнера. Возвращает, сколько штук смержено.
    public static int MergeIntoExisting(ContainerInstance c, ref GridItem item)
    {
        if (c.items == null || c.count <= 0 || item.stack.qty <= 0) return 0;
        int merged = 0;

        for (int i = 0; i < c.count && item.stack.qty > 0; i++)
        {
            var t = c.items[i];
            if (!CanStack(t, item)) continue;

            // Если лимита стека в проекте нет — считаем "практически бесконечный".
            const int MAX_STACK = 999999;

            int space = Mathf.Max(0, MAX_STACK - t.stack.qty);
            if (space <= 0) continue;

            int move = Mathf.Min(space, item.stack.qty);
            t.stack.qty += move;
            item.stack.qty -= move;
            merged += move;
            c.items[i] = t;
        }
        return merged;
    }

    /// Универсальная укладка: сначала стекуем, потом авторазмещение.
    public static bool TryPlace(ContainerInstance c, ref GridItem item, out CellRef pos, out int idx)
    {
        // 1) стекуем в существующие
        MergeIntoExisting(c, ref item);
        if (item.stack.qty <= 0) { pos = default; idx = -1; return true; }

        // 2) остаток — автоплейс по свободным клеткам
        if (c.TryAutoPlace(ref item, out idx, out pos))
            return true;

        // частичный успех считается успехом (qty уже уменьшили MergeIntoExisting)
        return item.stack.qty <= 0;
    }

    static bool CanStack(in GridItem a, in GridItem b)
    {
        if (a.def != b.def) return false;
        return a.stack.key.Equals(b.stack.key); // VariantKey полностью совпадает
    }
}
