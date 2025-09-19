using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragController : MonoBehaviour
{
    public DragGhost ghost;
    public PlayerStateRelay stateRelay;
    public EquipmentPanel equipPanel;
    public HotbarController hotbar;
    public PlayerInventory playerInv;

    public bool Active => _active;
    public InventoryPanel SourcePanel => _srcPanel;
    public Vector3Int CurrentSize => _active ? _dragItem.size : new Vector3Int(1, 1, 1);

    bool _active;
    GridItem _dragItem;
    ContainerInstance _srcContainer;
    InventoryPanel _srcPanel;

    Vector2 _cellSize;
    Vector2 _spacing;


    CellRef _srcCell;
    bool _hasSrcCell;

    public void BeginDragFromContainer(InventoryPanel srcPanel, ContainerInstance src, int srcIndex, GridItem item, Sprite icon, bool split)
    {
        if (!ActionGate.CanMoveInUI(stateRelay.Current)) return;

        _srcPanel = srcPanel;
        _srcContainer = src;
        _dragItem = item;
        _active = true;


        _hasSrcCell = false;
        if (srcIndex >= 0 && srcIndex < src.count)
        {
            _srcCell = src.positions[srcIndex];
            _hasSrcCell = true;
        }


        _cellSize = new Vector2(64, 64);
        _spacing = Vector2.zero;
        GridLayoutGroup grid = null;
        if (srcPanel != null && srcPanel.cellsRoot != null)
            grid = srcPanel.cellsRoot.GetComponent<GridLayoutGroup>();
        if (grid) { _cellSize = grid.cellSize; _spacing = grid.spacing; }


        var srcCanvas = srcPanel ? srcPanel.GetComponentInParent<Canvas>() : null;
        if (ghost && srcCanvas) ghost.AttachTo(srcCanvas);

        ghost?.Show(icon, new Vector2Int(item.size.x, item.size.y), _cellSize, _spacing);
    }

    public void Rotate()
    {
        if (!_active) return;
        if (_dragItem.def && !_dragItem.def.canRotate) return;

        _dragItem.size = GridAlgo.Rotated(_dragItem.size, _dragItem.def && _dragItem.def.is3D);
        ghost?.Rotate();
    }

    public void DropToContainerAt(ContainerInstance dst, int cx, int cy, InventoryPanel dstPanel)
    {
        if (!_active || dst == null) return;

        var tmp = _dragItem;

 
        bool samePlace = dst.Equals(_srcContainer) && _hasSrcCell && cx == _srcCell.x && cy == _srcCell.y;
        if (samePlace)
        {
            dst.TryPlaceAt(ref tmp, cx, cy, 0, out _);
            ghost?.Hide(); _active = false;
            dstPanel.RedrawItems(); _srcPanel?.RedrawItems();
            return;
        }


        Placement.MergeIntoExisting(dst, ref tmp);
        if (tmp.stack.qty <= 0)
        {
            ghost?.Hide(); _active = false;
            dstPanel.RedrawItems(); _srcPanel?.RedrawItems();
            return;
        }


        bool placed = dst.TryPlaceAt(ref tmp, cx, cy, 0, out _);


        if (!placed) placed = Placement.TryPlace(dst, ref tmp, out _, out _);

        if (placed)
        {
            ghost?.Hide(); _active = false;
            dstPanel.RedrawItems(); _srcPanel?.RedrawItems();
        }
        else
        {
            CancelReturnToSource();
        }
    }

    public bool TryDropToAnyInventory(PointerEventData e)
    {
        if (EventSystem.current == null) return false;

        var results = new List<RaycastResult>(16);
        EventSystem.current.RaycastAll(e, results);

        for (int i = 0; i < results.Count; i++)
        {
            var panel = results[i].gameObject.GetComponentInParent<InventoryPanel>();
            if (panel == null || panel.bound.def == null) continue;

            var sz = CurrentSize;
            if (panel.ScreenToCell(e.position, out int cx, out int cy, sz))
            {
                DropToContainerAt(panel.bound, cx, cy, panel);
                return true;
            }
        }
        return false;
    }

    public bool TryDropToHotbar(Vector2 screen)
    {
        if (!_active || hotbar == null || hotbar.slotIcons == null) return false;

        for (int i = 0; i < hotbar.slotIcons.Length; i++)
        {
            var img = hotbar.slotIcons[i]; if (!img) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(img.rectTransform, screen))
            {
                hotbar.Assign(i, _dragItem);
                ghost?.Hide();
                _active = false;
                _srcPanel?.RedrawItems();
                return true;
            }
        }
        return false;
    }

    public void TryDropToEquipment(Vector2 screen)
    {
        if (!_active) return;
        if (!equipPanel || !equipPanel.TryGetSlotUnderScreen(screen, out var slot))
        { CancelReturnToSource(); return; }


        if (!playerInv || !playerInv.Equipment.CanEquip(_dragItem.def, slot))
        { CancelReturnToSource(); return; }


        playerInv.Equipment.TryEquip(slot, _dragItem);

        ghost?.Hide();
        _active = false;
        _srcPanel?.RedrawItems();
    }

    public void CancelReturnToSource()
    {
        if (!_active) return;

        var tmp = _dragItem;


        bool placedBack = false;
        if (_hasSrcCell)
            placedBack = _srcContainer.TryPlaceAt(ref tmp, _srcCell.x, _srcCell.y, 0, out _);

        if (!placedBack)
            _srcContainer.TryAutoPlace(ref tmp, out _, out _);

        ghost?.Hide();
        _active = false;
        _srcPanel?.RedrawItems();
    }
}
