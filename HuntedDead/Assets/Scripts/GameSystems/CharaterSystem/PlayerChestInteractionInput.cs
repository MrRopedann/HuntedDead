using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerChestInteractionTrigger : MonoBehaviour
{
    private Chest nearbyChest;
    public InputActionReference openLootAction;

    void OnEnable()
    {
        if (openLootAction != null)
            openLootAction.action.Enable();
    }

    void OnDisable()
    {
        if (openLootAction != null)
            openLootAction.action.Disable();
    }

    void Update()
    {
        if (nearbyChest != null && openLootAction != null && openLootAction.action.WasPressedThisFrame())
        {
            nearbyChest.OpenChest();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Chest chest = other.GetComponent<Chest>();
        if (chest != null)
            nearbyChest = chest;
    }

    private void OnTriggerExit(Collider other)
    {
        Chest chest = other.GetComponent<Chest>();
        if (chest != null && chest == nearbyChest)
            nearbyChest = null;
    }
}
