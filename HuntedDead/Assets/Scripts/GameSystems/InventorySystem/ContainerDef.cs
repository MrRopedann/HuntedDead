using UnityEngine;

[CreateAssetMenu(menuName = "DB/ContainerDef")]
public class ContainerDef : ScriptableObject
{
    public string displayName;
    public bool is3D = false;
    public Vector3Int gridSize = new(8, 6, 1);
    public bool allowsRotation = true;
    public ItemKind[] allowedKinds;
}
