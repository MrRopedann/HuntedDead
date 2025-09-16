using System;

[Serializable]
public struct VariantKey : IEquatable<VariantKey>
{
    public string itemGuid;
    public QualityTier tier;
    public bool Equals(VariantKey o) => itemGuid == o.itemGuid && tier == o.tier;
    public override int GetHashCode() => (itemGuid?.GetHashCode() ?? 0) ^ (int)tier;
}
