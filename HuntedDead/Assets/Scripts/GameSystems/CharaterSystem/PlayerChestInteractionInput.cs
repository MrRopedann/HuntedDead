using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PlayerChestInteractionTrigger : MonoBehaviour
{
    private Interactable nearbyInteractable;
    private Inventory playerInventory;
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

    void Start()
    {
        playerInventory = GetComponent<Inventory>();
    }

    void Update()
    {
        if (nearbyInteractable != null && openLootAction != null && openLootAction.action.WasPressedThisFrame())
        {
            nearbyInteractable.Interact(playerInventory);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<Interactable>();
        if (interactable != null && !interactable.isUsed)
            nearbyInteractable = interactable;
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<Interactable>();
        if (interactable != null && interactable == nearbyInteractable)
            nearbyInteractable = null;
    }
}
