using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionTrigger : MonoBehaviour
{
    private Interactable nearbyObject;
    public InputActionReference interactAction;
    public Inventory playerInventory;

    void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.Enable();
    }

    void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.Disable();
    }

    void Update()
    {
        if (nearbyObject != null && interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            nearbyObject.Interact(playerInventory);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable))
            nearbyObject = interactable;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable) && interactable == nearbyObject)
            nearbyObject = null;
    }
}
