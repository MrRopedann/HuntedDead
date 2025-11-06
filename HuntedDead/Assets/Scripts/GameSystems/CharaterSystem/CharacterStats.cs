using UnityEngine;
using System;

[Serializable]
public class Stat
{
    [SerializeField] private float baseValue;
    private float modifier = 0f;

    public float BaseValue => baseValue;
    public float Value => baseValue + modifier;

    public void AddModifier(float value) => modifier += value;
    public void RemoveModifier(float value) => modifier -= value;
}

public class CharacterStats : MonoBehaviour
{
    [Header("ќсновные статы")]
    public Stat walkSpeed = new Stat();     // обычна€ скорость
    public Stat sprintSpeed = new Stat();   // скорость рывка
    public Stat runSpeed = new Stat();   // скорость рывка
    public Stat damage = new Stat();
    public Stat defense = new Stat();
    public Stat maxHealth = new Stat();

    [Header("“екущее состо€ние")]
    public float currentHealth;

    public event Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth.Value;
    }

    public void TakeDamage(float amount)
    {
        float dmg = Mathf.Max(0, amount - defense.Value);
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth.Value);
    }

    void Die()
    {
        OnDeath?.Invoke();
        Debug.Log($"{name} погиб.");
    }
}
