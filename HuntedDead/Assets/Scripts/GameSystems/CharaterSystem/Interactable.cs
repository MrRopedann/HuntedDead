using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    public string interactionLabel = "Использовать";
    public bool isUsed = false;

    public abstract void Interact(Inventory playerInventory);
}
