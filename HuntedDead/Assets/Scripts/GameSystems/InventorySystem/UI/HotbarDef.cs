using UnityEngine;

[CreateAssetMenu(menuName = "DB/HotbarDef")]
public class HotbarDef : ScriptableObject
{
    public int slots = 10;
    public float ringCooldownSeconds = 0.6f;
}
