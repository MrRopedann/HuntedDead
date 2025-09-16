using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Bind")]
    public RectTransform cellsRoot;   // контейнер клеток (с GridLayoutGroup)
    public RectTransform itemsRoot;   // слой иконок (без Layout)
    public GameObject cellPrefab;     // Cell_Grid
    public GameObject itemPrefab;     // Cell_Item
    public UiPool pool;
    public DbRegistry db;
    public DragController drag;

    [Header("State")]
    public ContainerInstance bound;
    GridCellView[] _cells;
    RectTransform[] _icons;
    int _cellCount;
    GridLayoutGroup _grid;
    Vector2 _cellSize; Vector2 _spacing;

    void Awake()
    {
        _grid = cellsRoot.GetComponent<GridLayoutGroup>();
        _cellSize = _grid.cellSize;
        _spacing = _grid.spacing;
    }

    public void Bind(ContainerInstance c)
    {
        bound = c;
        BuildCellsOnce();
        RedrawItems();
    }

    void BuildCellsOnce()
    {
        int mx = bound.occ.GetLength(0), my = bound.occ.GetLength(1), mz = bound.occ.GetLength(2);
        int need = mx * my; // слой Z показываем выбранный; для 2D z=1
        if (_cells != null && _cellCount == need) return;
        // очистка старых
        foreach (Transform ch in cellsRoot) Destroy(ch.gameObject);
        _cells = new GridCellView[need];
        for (int i = 0; i < need; i++)
        {
            var go = Instantiate(cellPrefab, cellsRoot);
            _cells[i] = go.GetComponent<GridCellView>();
        }
        _cellCount = need;
        itemsRoot.pivot = new Vector2(0, 1);
        (itemsRoot as RectTransform).anchoredPosition = Vector2.zero;
    }

    public void RedrawItems()
    {
        foreach (Transform ch in itemsRoot) Destroy(ch.gameObject);
        for (int i = 0; i < bound.count; i++)
        {
            var gi = bound.items[i];
            var p = bound.positions[i];
            var go = Instantiate(itemPrefab, itemsRoot);
            var rt = go.GetComponent<RectTransform>();
            var icon = go.GetComponent<ItemIcon>();
            var qcol = FindQualityColor(gi);
            icon.Bind(gi.def.icon, gi.stack.qty, qcol);
            SetItemRect(rt, p, gi.size);
        }
    }

    Color FindQualityColor(GridItem gi)
    {
        var def = gi.def;
        var tier = gi.stack.key.tier;
        var arr = def.qualities;
        if (arr != null)
            for (int i = 0; i < arr.Length; i++) if (arr[i].tier == tier) return arr[i].color;
        return Color.white;
    }

    void SetItemRect(RectTransform rt, CellRef pos, Vector3Int size)
    {
        float w = _cellSize.x, h = _cellSize.y, sx = _spacing.x, sy = _spacing.y;
        var x = pos.x * (w + sx);
        var y = -pos.y * (h + sy);
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(size.x * w + (size.x - 1) * sx, size.y * h + (size.y - 1) * sy);
    }

    public bool ScreenToCell(Vector2 screen, out int cx, out int cy)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(cellsRoot, screen, null, out var local);
        var size = (cellsRoot.rect.size);
        if (local.x < 0 || local.y > 0 || local.x > size.x || -local.y > size.y) { cx = cy = -1; return false; }
        float w = _grid.cellSize.x + _grid.spacing.x;
        float h = _grid.cellSize.y + _grid.spacing.y;
        cx = Mathf.FloorToInt(local.x / w);
        cy = Mathf.FloorToInt(-local.y / h);
        int mx = bound.occ.GetLength(0), my = bound.occ.GetLength(1);
        if (cx < 0 || cy < 0 || cx >= mx || cy >= my) { cx = cy = -1; return false; }
        return true;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (bound == null) return;
        if (!ScreenToCell(e.position, out var cx, out var cy)) return;
        if (!bound.TryFindAtCell(cx, cy, 0, out var idx)) return;
        // Shift-сплит
        bool split = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        var gi = bound.items[idx];
        var icon = gi.def.icon;
        if (split && gi.stack.qty > 1)
        {
            int half = gi.stack.qty / 2;
            gi.stack.qty -= half;              // остаётся в контейнере
            bound.items[idx] = gi;
            var giDrag = gi;
            giDrag.stack.qty = half;
            drag.BeginDragFromContainer(this, bound, idx, giDrag, icon, split: true);
        }
        else
        {
            drag.BeginDragFromContainer(this, bound, idx, gi, icon, split: false);
            bound.RemoveAt(idx); // временно убрали для свободного размещения
            RedrawItems();
        }
    }

    public void OnDrag(PointerEventData e) { /* пусто: DragGhost сам следует */ }

    public void OnPointerUp(PointerEventData e)
    {
        if (!drag.Active || drag.SourcePanel != this) return;
        if (ScreenToCell(e.position, out var cx, out var cy))
        {
            drag.DropToContainerAt(bound, cx, cy, this);
        }
        else
        {
            drag.CancelReturnToSource(); // возврат
        }
    }
}
