using UnityEngine;
using UnityEngine.InputSystem;

public class InputRouter : MonoBehaviour
{
    public InventoryController inv;
    public DragController drag;
    public HotbarInput hotbar;
    public PlayerInventory pinv;
    public LootInteractor loot;

    public void OnToggleInventory(InputAction.CallbackContext ctx) { if (!ctx.performed) return; inv.ToggleInventory(); }
    public void OnRotate(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Debug.Log($"OnRotate PERFORMED, drag={(drag ? drag.name : "null")}");
        drag.Rotate();
    }
    public void OnVicinity(InputAction.CallbackContext ctx) { if (!ctx.performed) return; inv.ToggleVicinity(); }

    public void OnOpenLoot(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (ctx.started) loot.BeginHold();
        if (ctx.canceled) loot.CancelHold();
    }
    public void OnHotbarNext(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.Next(); }
    public void OnHotbarPrev(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.Prev(); }
    public void OnSlot1(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.SelectIndex(0); }
    public void OnSlot2(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.SelectIndex(1); }
    public void OnSlot3(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.SelectIndex(2); }
    public void OnSlot4(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.SelectIndex(3); }
    public void OnSlot5(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.SelectIndex(4); }
    public void OnSlot6(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.SelectIndex(5); }
    public void OnSlot7(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.SelectIndex(6); }
    public void OnSlot8(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.SelectIndex(7); }
    public void OnSlot9(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.SelectIndex(8); }
    public void OnSlot0(InputAction.CallbackContext ctx) { if (!ctx.performed) return; hotbar.SelectIndex(9); }
}
