using UnityEngine;

public class LootInteractor : MonoBehaviour
{
    public PlayerStateRelay stateRelay;
    public InventoryController invUI;
    public Camera cam;
    public InteractUI ui;

    public float range = 3f;
    public float holdSeconds = 0.8f;

    bool holding;
    float t;
    LootSourceChest focus;

    void Update()
    {

        LootSourceChest hit = null;
        if (cam && Physics.Raycast(cam.transform.position, cam.transform.forward, out var h, range, ~0, QueryTriggerInteraction.Collide))
            hit = h.collider.GetComponentInParent<LootSourceChest>();


        bool can = ActionGate.CanOpenLoot(stateRelay.Current);
        if (hit && can)
        {
            focus = hit;
            ui?.Show(hit.chestDef ? hit.chestDef.displayName : hit.name);
        }
        else
        {
            focus = null;
            holding = false; t = 0f;
            ui?.Hide();
        }


        if (holding)
        {
            t += Time.deltaTime;
            ui?.SetProgress(t / holdSeconds);
            if (t >= holdSeconds && focus)
            {
                holding = false; t = 0f; ui?.SetProgress(0f);
                invUI.OpenLootFrom(focus);
            }
        }
    }

    public void BeginHold()
    {
        if (focus && ActionGate.CanOpenLoot(stateRelay.Current)) { holding = true; t = 0f; }
    }
    public void CancelHold() { holding = false; t = 0f; ui?.SetProgress(0f); }
}
