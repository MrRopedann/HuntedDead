using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VicinityPanel : MonoBehaviour
{
    public Transform listRoot;
    public GameObject rowPrefab;
    public VicinityScanner scanner;
    public InventoryPanel inventoryPanel;

    public void Refresh()
    {
        foreach (Transform ch in listRoot) Destroy(ch.gameObject);
        var agg = scanner.ScanAggregated();
        foreach (var kv in agg)
        {
            var go = Instantiate(rowPrefab, listRoot);
            var row = go.GetComponent<LootRowView>();
            row.icon.sprite = FindObjectOfType<DbRegistry>().ItemByGuid(kv.Key.itemGuid).icon;
            row.nameTxt.text = kv.Key.itemGuid + " " + kv.Key.tier;
            row.qtyTxt.text = kv.Value.ToString();
            row.takeBtn.onClick.AddListener(() => Pickup(kv.Key, 1));
            row.takeAllBtn.onClick.AddListener(() => Pickup(kv.Key, kv.Value));
        }
    }

    void Pickup(VariantKey key, int want)
    {
        int got = scanner.PickupInto(inventoryPanel.bound, key, want);
        if (got > 0) inventoryPanel.RedrawItems();
        Refresh();
    }
}
