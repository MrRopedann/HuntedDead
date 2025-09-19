using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("Refs")]
    public PlayerInventory playerInv;
    public PlayerStateRelay stateRelay;
    public InventoryPanel panelInventory;
    public EquipmentPanel panelEquipment;
    public LootPanel panelLoot;
    public VicinityPanel panelVicinity;

    [Header("UI root / control lock")]
    [SerializeField] GameObject inventoryRoot;
    [SerializeField] CameraController cam;
    [SerializeField] ThirdPersonController playerCtrl;

    [Header("Опционально: пустой деф для центра при Tab")]
    public ContainerDef lootEmptyDef;

    public bool IsOpen { get; private set; }

    ContainerInstance _lootPlaceholder;

    void Start()
    {
        BindTabs();

        if (playerInv && panelInventory)
            panelInventory.Bind(playerInv.Pockets);

        EnsureLootPlaceholderBound();

        HideAllAndUnblock();
    }

    void BindTabs()
    {
        if (panelInventory && !panelInventory.db)
            panelInventory.db = FindObjectOfType<DbRegistry>();

        if (panelLoot && !panelLoot.grid)
        {
            var ownGrid = panelLoot.GetComponent<InventoryPanel>();
            if (ownGrid) panelLoot.grid = ownGrid;
        }
    }

    void EnsureLootPlaceholderBound()
    {
        if (!panelLoot) return;

        var grid = panelLoot.grid ? panelLoot.grid : panelLoot.GetComponent<InventoryPanel>();
        if (!grid) return;


        if (grid.bound != null && grid.bound.def != null) return;


        if (!lootEmptyDef) return;


        if (_lootPlaceholder == null || _lootPlaceholder.def == null)
        {
            _lootPlaceholder = new ContainerInstance();
            _lootPlaceholder.Init(lootEmptyDef);
        }
        grid.Bind(_lootPlaceholder);
    }


    public void ToggleInventory()
    {
        if (stateRelay && !ActionGate.CanMoveInUI(stateRelay.Current)) return;

        BindTabs();
        EnsureLootPlaceholderBound();

        if (IsOpen) HideAllAndUnblock();
        else ShowAllAndBlock();
    }

    public void ToggleVicinity()
    {
        if (stateRelay && !ActionGate.CanMoveInUI(stateRelay.Current)) return;
        if (!panelVicinity) return;

        bool v = !panelVicinity.gameObject.activeSelf;
        panelVicinity.gameObject.SetActive(v);
        if (v) panelVicinity.Refresh();
    }

    public void OpenLoot(ILootSource src)
    {
        if (stateRelay && !ActionGate.CanOpenLoot(stateRelay.Current)) return;
        var uo = src as UnityEngine.Object; if (uo == null) return;

        BindTabs();

        var c = src.Open();
        if (panelLoot) panelLoot.Bind(c);

        ShowAllAndBlock();
    }

    public void OpenLootFrom(LootSourceChest src)
    {
        if (stateRelay && !ActionGate.CanOpenLoot(stateRelay.Current)) return;
        if (!src) return;

        BindTabs();

        var c = src.Open();
        if (panelLoot) panelLoot.Bind(c);

        ShowAllAndBlock();
    }

    public void CloseAll() => HideAllAndUnblock();
    public void OnEnterCombat() => HideAllAndUnblock();

    void ShowAllAndBlock()
    {
        IsOpen = true;

        if (inventoryRoot) inventoryRoot.SetActive(true);
        else
        {
            if (panelInventory) panelInventory.gameObject.SetActive(true);
            if (panelEquipment) panelEquipment.gameObject.SetActive(true);
            if (panelLoot) panelLoot.gameObject.SetActive(true);
        }

        if (panelInventory) panelInventory.RedrawItems();

        if (panelLoot && panelLoot.grid && panelLoot.grid.bound != null && panelLoot.grid.bound.def != null)
            panelLoot.Redraw();

        cam?.SetUiBlock(true);
        if (playerCtrl) playerCtrl.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HideAllAndUnblock()
    {
        IsOpen = false;

        if (inventoryRoot) inventoryRoot.SetActive(false);
        else
        {
            if (panelInventory) panelInventory.gameObject.SetActive(false);
            if (panelEquipment) panelEquipment.gameObject.SetActive(false);
            if (panelLoot) panelLoot.gameObject.SetActive(false);
            if (panelVicinity) panelVicinity.gameObject.SetActive(false);
        }

        cam?.SetUiBlock(false);
        if (playerCtrl) playerCtrl.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
