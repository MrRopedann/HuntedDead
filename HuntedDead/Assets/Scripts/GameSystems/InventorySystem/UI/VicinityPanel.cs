using UnityEngine;

public class VicinityPanel : MonoBehaviour
{
    public InventoryPanel grid;
    public VicinityScanner scanner;
    public DbRegistry db;

    [Header("Размер грида для отображения")]
    public Vector3Int gridSize = new Vector3Int(6, 4, 1);

    ContainerDef _defRuntime;
    ContainerInstance _view

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

    public void Refresh()
    {
        if (_defRuntime == null) return;


        _view = new ContainerInstance();
        _view.Init(_defRuntime);


        var agg = scanner.ScanAggregated();
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


            Placement.TryPlace(_view, ref gi, out _, out _);
        }

        if (grid)
        {
            grid.Bind(_view);
            grid.RedrawItems();
        }
    }
}
