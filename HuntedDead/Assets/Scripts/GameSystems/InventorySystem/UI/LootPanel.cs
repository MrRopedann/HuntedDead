using UnityEngine;

public class LootPanel : MonoBehaviour
{
    public Transform listRoot;
    public GameObject rowPrefab;
    public InventoryPanel inventoryPanel; // куда класть
    ILootSource _src;

    public void Open(ILootSource s) { _src = s; Redraw(); }

    public void Redraw()
    {
        foreach (Transform ch in listRoot) Destroy(ch.gameObject);
        var c = _src.Open();
        for (int i = 0; i < c.count; i++)
        {
            var gi = c.items[i];
            var go = Instantiate(rowPrefab, listRoot);
            var row = go.GetComponent<LootRowView>();
            row.index = i;
            row.icon.sprite = gi.def.icon;
            row.nameTxt.text = gi.def.name;
            row.qtyTxt.text = gi.stack.qty.ToString();
            row.takeBtn.onClick.AddListener(() => TakeOne(row.index, 1));
            row.takeAllBtn.onClick.AddListener(() => TakeOne(row.index, gi.stack.qty));
        }
    }

    void TakeOne(int index, int qty)
    {
        var c = _src.Open();
        if (index < 0 || index >= c.count) { Redraw(); return; }
        var gi = c.items[index];
        var put = gi; put.stack.qty = qty;
        if (Placement.TryPlace(inventoryPanel.bound, ref put, out _, out _))
        {
            _src.TakeStackAt(index, qty);
            inventoryPanel.RedrawItems();
            Redraw();
        }
    }
}
