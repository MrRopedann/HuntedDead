using UnityEngine;
using UnityEngine.UI;

public class DragGhost : MonoBehaviour
{
    public Image img;
    public RectTransform dragCanvasRoot;
    public bool rotateSpriteVisual = true;

    Vector2Int _curCells = new(1, 1);
    Vector2 _cellSize = new(64, 64);
    Vector2 _spacing = Vector2.zero;

    RectTransform _selfRT;
    RectTransform _imgRT;

    void Awake()
    {
        _selfRT = (RectTransform)transform;
        _imgRT = (RectTransform)img.transform;

        if (dragCanvasRoot) _selfRT.SetParent(dragCanvasRoot, false);

        _selfRT.anchorMin = _selfRT.anchorMax = new Vector2(0.5f, 0.5f);
        _selfRT.pivot = new Vector2(0.5f, 0.5f);
        _selfRT.localScale = Vector3.one;

        img.raycastTarget = false;
        img.enabled = false;
        img.preserveAspect = false;
    }

    public void AttachTo(Canvas canvas)
    {
        if (!canvas) return;
        _selfRT.SetParent(canvas.transform, false);
        _selfRT.localScale = Vector3.one;
    }

    public void Show(Sprite s, Vector2Int sizeInCells, Vector2 cellSize, Vector2 spacing)
    {
        _curCells = new Vector2Int(Mathf.Max(1, sizeInCells.x), Mathf.Max(1, sizeInCells.y));
        _cellSize = cellSize;
        _spacing = spacing;

        img.sprite = s;
        img.enabled = true;

        _imgRT.localScale = Vector3.one;
        _imgRT.anchorMin = _imgRT.anchorMax = new Vector2(0.5f, 0.5f);
        _imgRT.pivot = new Vector2(0.5f, 0.5f);
        _imgRT.localEulerAngles = Vector3.zero;
        _imgRT.sizeDelta = CellsToPixels(_curCells);

        _selfRT.localScale = Vector3.one;

        transform.position = Input.mousePosition;
    }

    public void Hide() { if (img) img.enabled = false; }

    public void Rotate()
    {
        if (!img || !img.enabled) return;
        _curCells = new Vector2Int(_curCells.y, _curCells.x);
        _imgRT.sizeDelta = CellsToPixels(_curCells);
        if (rotateSpriteVisual)
            _imgRT.localEulerAngles = new Vector3(0, 0, (_imgRT.localEulerAngles.z + 90f) % 180f);
    }

    void LateUpdate()
    {
        if (img && img.enabled) transform.position = Input.mousePosition;
    }

    Vector2 CellsToPixels(Vector2Int sz)
    {
        float w = _cellSize.x, h = _cellSize.y, sx = _spacing.x, sy = _spacing.y;
        return new Vector2(sz.x * w + (sz.x - 1) * sx, sz.y * h + (sz.y - 1) * sy);
    }
}
