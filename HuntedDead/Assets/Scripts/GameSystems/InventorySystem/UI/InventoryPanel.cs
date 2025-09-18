using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Bind")]
    public RectTransform cellsRoot;
    public RectTransform itemsRoot;
    public GameObject cellPrefab;
    public GameObject itemPrefab;
    public UiPool pool;
    public DbRegistry db;
    public DragController drag;

    [Header("State")]
    public ContainerInstance bound;
    GridCellView[] _cells;
    int _cellCount;
    GridLayoutGroup _grid;
    Vector2 _cellSize, _spacing;
    bool _isBound; // ← ключевой флаг

    void Awake()
    {
        if (cellsRoot) _grid = cellsRoot.GetComponent<GridLayoutGroup>();
        if (_grid) { _cellSize = _grid.cellSize; _spacing = _grid.spacing; }
    }

    void OnEnable()
    {
        if (!_isBound) return;                          // ← не трогаем без Bind
        if (_grid == null && cellsRoot) _grid = cellsRoot.GetComponent<GridLayoutGroup>();
        if (_grid == null || itemsRoot == null || cellPrefab == null || itemPrefab == null) return;

        BuildCellsOnce();
        RedrawItems();
    }

    void Update()
    {
        if (drag != null && drag.Active && Input.GetKeyDown(KeyCode.R)) drag.Rotate();
    }

    public void Bind(ContainerInstance c)
    {
        bound = c;
        _isBound = (bound.def != null);                // ← отмечаем привязку
        if (!_isBound) return;

        if (_grid == null && cellsRoot) _grid = cellsRoot.GetComponent<GridLayoutGroup>();
        BuildCellsOnce();
        RedrawItems();
    }

    void BuildCellsOnce()
    {
        if (!_isBound || cellsRoot == null || cellPrefab == null) return;

        int mx = bound.occ.GetLength(0), my = bound.occ.GetLength(1);
        int need = mx * my;
        if (_cells != null && _cellCount == need) return;

        foreach (Transform ch in cellsRoot) Destroy(ch.gameObject);

        _cells = new GridCellView[need];
        for (int i = 0; i < need; i++)
        {
            var go = Instantiate(cellPrefab, cellsRoot);
            _cells[i] = go.GetComponent<GridCellView>();
        }
        _cellCount = need;

        if (itemsRoot)
        {
            itemsRoot.pivot = new Vector2(0, 1);
            itemsRoot.anchoredPosition = Vector2.zero;
        }
    }

    public void RedrawItems()
    {
        if (!_isBound || itemsRoot == null || itemPrefab == null || _grid == null) return;

        foreach (Transform ch in itemsRoot) Destroy(ch.gameObject);

        for (int i = 0; i < bound.count; i++)
        {
            var gi = bound.items[i];
            var p = bound.positions[i];

            var go = Instantiate(itemPrefab, itemsRoot);
            var rt = go.GetComponent<RectTransform>();
            var icon = go.GetComponent<ItemIcon>();

            var qcol = FindQualityColor(gi);
            if (icon)
            {
                icon.Bind(gi.def ? gi.def.icon : null, gi.stack.qty, qcol);

                var baseSz = gi.def.is3D ? gi.def.size3D : gi.def.size2D;
                bool rotated =
                    baseSz.x != baseSz.y &&
                    gi.size.x == baseSz.y &&
                    gi.size.y == baseSz.x &&
                    gi.size.z == baseSz.z;

                icon.SetRotated(rotated);
            }

            SetItemRect(rt, p, gi.size);
        }
    }

    Color FindQualityColor(GridItem gi)
    {
        var arr = gi.def ? gi.def.qualities : null;
        if (arr != null)
        {
            var t = gi.stack.key.tier;
            for (int i = 0; i < arr.Length; i++)
                if (arr[i].tier == t) return arr[i].color;
        }
        return Color.white;
    }

    void SetItemRect(RectTransform rt, CellRef pos, Vector3Int size)
    {
        if (!rt || _grid == null) return;
        float w = _grid.cellSize.x, h = _grid.cellSize.y;
        float sx = _grid.spacing.x, sy = _grid.spacing.y;

        float x = pos.x * (w + sx);
        float y = -pos.y * (h + sy);

        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(
            size.x * w + (size.x - 1) * sx,
            size.y * h + (size.y - 1) * sy
        );
    }

    public bool ScreenToCell(Vector2 screen, out int cx, out int cy)
    {
        cx = cy = -1;
        if (cellsRoot == null || _grid == null || !_isBound) return false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(cellsRoot, screen, null, out var local);
        Vector2 size = cellsRoot.rect.size;

        if (local.x < 0 || local.y > 0 || local.x > size.x || -local.y > size.y) return false;

        float w = _grid.cellSize.x + _grid.spacing.x;
        float h = _grid.cellSize.y + _grid.spacing.y;
        cx = Mathf.FloorToInt(local.x / w);
        cy = Mathf.FloorToInt(-local.y / h);

        int mx = bound.occ.GetLength(0), my = bound.occ.GetLength(1);
        if (cx < 0 || cy < 0 || cx >= mx || cy >= my) { cx = cy = -1; return false; }
        return true;
    }

    public bool ScreenToCell(Vector2 screen, out int cx, out int cy, Vector3Int size)
    {
        if (!ScreenToCell(screen, out cx, out cy)) return false;
        int mx = bound.occ.GetLength(0), my = bound.occ.GetLength(1);
        cx = Mathf.Clamp(cx, 0, Mathf.Max(0, mx - size.x));
        cy = Mathf.Clamp(cy, 0, Mathf.Max(0, my - size.y));
        return true;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (!_isBound) return;
        if (!ScreenToCell(e.position, out var cx, out var cy)) return;
        if (!bound.TryFindAtCell(cx, cy, 0, out var idx)) return;

        var gi = bound.items[idx];
        var icon = gi.def.icon;

        drag.BeginDragFromContainer(this, bound, idx, gi, icon, false);
        bound.RemoveAt(idx);
        RedrawItems();
    }

    public void OnDrag(PointerEventData e) { }

    public void OnPointerUp(PointerEventData e)
    {
        if (drag == null || !drag.Active) return;

        var sz = drag.CurrentSize;
        if (ScreenToCell(e.position, out var cx, out var cy, sz))
        { drag.DropToContainerAt(bound, cx, cy, this); return; }

        if (drag.TryDropToAnyInventory(e)) return;
        if (drag.TryDropToHotbar(e.position)) return;
        drag.TryDropToEquipment(e.position);
    }
}
