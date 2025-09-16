using UnityEngine;

public enum QualityTier { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(menuName = "DB/ItemQuality")]
public class ItemQuality : ScriptableObject
{
    public QualityTier tier;
    public Color color = Color.white;
    public int maxStack = 99;
}
