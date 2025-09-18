using UnityEngine;

/// Панель «Окрестности» = обычный InventoryPanel c временным контейнером.
public class VicinityPanel : MonoBehaviour
{
    public InventoryPanel grid;        // InventoryPanel на этом же объекте
    public VicinityScanner scanner;    // сканер мира
    public DbRegistry db;              // база ItemDef

    [Header("Размер грида для отображения")]
    public Vector3Int gridSize = new Vector3Int(6, 4, 1); // подгоняй под UI

    ContainerDef _defRuntime;          // временный деф
    ContainerInstance _view;           // контейнер для отображения

    void Awake()
    {
        if (!db) db = FindObjectOfType<DbRegistry>();
        // создаём деф на лету
        _defRuntime = ScriptableObject.CreateInstance<ContainerDef>();
        _defRuntime.displayName = "Vicinity";
        _defRuntime.is3D = false;
        _defRuntime.gridSize = gridSize;

        _view = new ContainerInstance();
        _view.Init(_defRuntime);

        if (grid) grid.Bind(_view);
    }

    /// Вызови при открытии панели и по клавише V
    public void Refresh()
    {
        if (_defRuntime == null) return;

        // заново инициализируем пустой контейнер-представление
        _view = new ContainerInstance();
        _view.Init(_defRuntime);

        // собираем агрегат из сканера и раскладываем в грид
        var agg = scanner.ScanAggregated(); // Dictionary<VariantKey,int>
        foreach (var kv in agg)
        {
            var key = kv.Key;
            int qty = kv.Value;

            var def = db.ItemByGuid(key.itemGuid);
            if (!def) continue;

            var gi = new GridItem
            {
                def = def,
                size = def.is3D ? def.size3D : def.size2D,
                rotated = false,
                stack = new ItemStack { key = key, qty = qty }
            };

            // просто попытка автоплейса для визуализации
            Placement.TryPlace(_view, ref gi, out _, out _);
        }

        if (grid)
        {
            grid.Bind(_view);
            grid.RedrawItems();
        }
    }
}
