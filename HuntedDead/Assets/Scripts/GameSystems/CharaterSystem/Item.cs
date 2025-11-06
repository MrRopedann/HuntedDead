using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(menuName = "Roguelike/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemRarity rarity;

    [Header("Бонусы к статам")]
    public float bonusWalkSpeed;
    public float bonusSprintSpeed;
    public float bonusRunSpeed;
    public float bonusDamage;
    public float bonusDefense;
    public float bonusHealth;

    public void Apply(CharacterStats stats)
    {
        stats.walkSpeed.AddModifier(bonusWalkSpeed);
        stats.sprintSpeed.AddModifier(bonusSprintSpeed);
        stats.runSpeed.AddModifier(bonusRunSpeed);
        stats.damage.AddModifier(bonusDamage);
        stats.defense.AddModifier(bonusDefense);
        stats.maxHealth.AddModifier(bonusHealth);
        stats.Heal(bonusHealth);
    }

    public void Remove(CharacterStats stats)
    {
        stats.walkSpeed.RemoveModifier(bonusWalkSpeed);
        stats.sprintSpeed.RemoveModifier(bonusSprintSpeed);
        stats.runSpeed.RemoveModifier(bonusRunSpeed);
        stats.damage.RemoveModifier(bonusDamage);
        stats.defense.RemoveModifier(bonusDefense);
        stats.maxHealth.RemoveModifier(bonusHealth);
        if (stats.currentHealth > stats.maxHealth.Value)
            stats.currentHealth = stats.maxHealth.Value;
    }

    public Color GetRarityColor()
    {
        return rarity switch
        {
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => Color.green,
            ItemRarity.Rare => Color.blue,
            ItemRarity.Epic => new Color(0.6f, 0f, 1f),
            ItemRarity.Legendary => new Color(1f, 0.6f, 0f),
            _ => Color.white
        };
    }
}
