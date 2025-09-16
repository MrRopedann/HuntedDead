using UnityEngine;

public class DragController : MonoBehaviour
{
    public DragGhost ghost;
    public PlayerStateRelay stateRelay;
    public EquipmentPanel equipPanel;

    public bool Active => _active;
    public InventoryPanel SourcePanel => _srcPanel;

    bool _active;
    GridItem _dragItem;
    Sprite _icon;
    ContainerInstance _srcContainer;
    InventoryPanel _srcPanel;

    public void BeginDragFromContainer(InventoryPanel srcPanel, ContainerInstance src, int srcIndex, GridItem item, Sprite icon, bool split)
    {
        if (!ActionGate.CanMoveInUI(stateRelay.Current)) return;
        _active = true;
        _srcPanel = srcPanel;
        _srcContainer = src;
        _dragItem = item;
        _icon = icon;
        ghost.Show(icon);
    }

    public void Rotate()
    {
        if (!_active) return;
        if (!_dragItem.def.canRotate) return;
        _dragItem.size = GridAlgo.Rotated(_dragItem.size, _dragItem.def.is3D);
        // визуал остаЄтс€ иконкой, размеры примен€тс€ при Drop
    }

    public void DropToContainerAt(ContainerInstance dst, int cx, int cy, InventoryPanel dstPanel)
    {
        if (!_active) return;
        var tmp = _dragItem;
        if (dst.TryPlaceAt(ref tmp, cx, cy, 0, out _))
        {
            ghost.Hide();
            _active = false;
            dstPanel.RedrawItems();
        }
        else CancelReturnToSource();
    }

    public void TryDropToEquipment(Vector2 screen, PlayerInventory pinv)
    {
        if (!_active) return;
        if (!equipPanel || !equipPanel.TryGetSlotUnderScreen(screen, out var slot))
        { CancelReturnToSource(); return; }

        if (!pinv.Equipment.CanEquip(_dragItem.def, slot)) { CancelReturnToSource(); return; }
        pinv.Equipment.TryEquip(slot, _dragItem);
        ghost.Hide(); _active = false;
        _srcPanel.RedrawItems();
    }

    public void CancelReturnToSource()
    {
        if (!_active) return;
        // положить обратно авто-плейсом в исходный контейнер
        var tmp = _dragItem;
        _srcContainer.TryAutoPlace(ref tmp, out _, out _);
        ghost.Hide();
        _active = false;
        _srcPanel.RedrawItems();
    }
}
