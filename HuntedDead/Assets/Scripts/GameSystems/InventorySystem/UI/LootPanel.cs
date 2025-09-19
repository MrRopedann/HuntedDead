using UnityEngine;

public class LootPanel : MonoBehaviour
{
    public InventoryPanel grid;

    bool HasBound()
    {
        if (!grid) return false;
        var b = grid.bound;
        return b != null && b.def != null;
    }

    public void Bind(ContainerInstance c)
    {
        if (!grid || c == null || c.def == null) return;
        if (!grid.gameObject.activeSelf) grid.gameObject.SetActive(true);
        grid.Bind(c);
        grid.RedrawItems();
    }

    public void Unbind()
    {

        if (grid && !grid.gameObject.activeSelf) grid.gameObject.SetActive(true);
    }

    public void Redraw()
    {
        if (!grid) return;
        if (!HasBound()) return;
        grid.RedrawItems();
    }
}
