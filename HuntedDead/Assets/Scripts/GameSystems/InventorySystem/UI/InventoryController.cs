using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public PlayerInventory playerInv;
    public PlayerStateRelay stateRelay;
    public InventoryPanel panelInventory;
    public EquipmentPanel panelEquipment;
    public LootPanel panelLoot;
    public VicinityPanel panelVicinity;

    void Start()
    {
        CloseAll();
        BindTabs();
    }

    void BindTabs()
    {
        panelInventory.db = FindObjectOfType<DbRegistry>();
        panelInventory.Bind(playerInv.Pockets);
        panelLoot.inventoryPanel = panelInventory;
        panelVicinity.inventoryPanel = panelInventory;
    }

    public void ToggleInventory()
    {
        if (!ActionGate.CanMoveInUI(stateRelay.Current)) return;
        bool vis = !panelInventory.gameObject.activeSelf;
        panelInventory.gameObject.SetActive(vis);
        panelEquipment.gameObject.SetActive(vis);
        if (!vis) panelVicinity.gameObject.SetActive(false);
    }

    public void OpenLoot(ILootSource src)
    {
        if (!ActionGate.CanOpenLoot(stateRelay.Current)) return;
        panelLoot.gameObject.SetActive(true);
        panelLoot.Open(src);
    }

    public void ToggleVicinity()
    {
        if (!ActionGate.CanMoveInUI(stateRelay.Current)) return;
        bool v = !panelVicinity.gameObject.activeSelf;
        panelVicinity.gameObject.SetActive(v);
        if (v) panelVicinity.Refresh();
    }

    public void CloseAll()
    {
        panelInventory.gameObject.SetActive(false);
        panelEquipment.gameObject.SetActive(false);
        panelLoot.gameObject.SetActive(false);
        panelVicinity.gameObject.SetActive(false);
    }

    public void OnEnterCombat() { CloseAll(); }
}
