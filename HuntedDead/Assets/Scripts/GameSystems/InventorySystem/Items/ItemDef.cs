using UnityEngine;

public enum ItemKind { Material, Consumable, Weapon, Gadget, Container }

[CreateAssetMenu(menuName = "DB/ItemDef")]
public class ItemDef : ScriptableObject
{
    public ItemId id;
    public ItemKind kind;
    public bool is3D;
    public Vector3Int size2D = new(1, 1, 1);
    public Vector3Int size3D = new(1, 1, 1);
    public bool stackable = true;
    public bool canRotate = true;
    public bool equippable = false;
    public string[] validEquipSlots;
    public Sprite icon;
    public GameObject worldPrefab;
    public ItemQuality[] qualities;
}
