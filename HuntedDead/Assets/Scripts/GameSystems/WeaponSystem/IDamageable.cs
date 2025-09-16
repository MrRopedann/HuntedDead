using UnityEngine;
using UnityEngine.Events;

public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
}

[System.Serializable] public class FloatEvent : UnityEvent<float> { }   // 0..1 здоровь€
[System.Serializable] public class VoidEvent : UnityEvent { }

public class Damageable : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] bool destroyOnDeath = true;

    [Header("Effects")]
    [SerializeField] GameObject hitVFX;            // спавн в точке попадани€
    [SerializeField] GameObject deathVFX;          // спавн в центре объекта
    [SerializeField] AudioSource audioSource;      // можно не заполн€ть Ч возьмЄтс€ с объекта
    [SerializeField] AudioClip hitSfx;
    [SerializeField] AudioClip deathSfx;
    [SerializeField] float impactImpulse = 2f;     // толчок по Rigidbody

    [Header("Events")]
    [SerializeField] FloatEvent onHealth01Changed; // передаЄт health/maxHealth
    [SerializeField] VoidEvent onDeath;

    float health;
    bool dead;
    Rigidbody rb;

    void Awake()
    {
        health = Mathf.Max(1f, maxHealth);
        rb = GetComponent<Rigidbody>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (dead) return;

        float dmg = Mathf.Max(0f, amount);
        health = Mathf.Max(0f, health - dmg);

        if (hitVFX)
        {
            var fx = Instantiate(hitVFX, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(fx, 2f);
        }
        if (audioSource && hitSfx) audioSource.PlayOneShot(hitSfx);
        if (rb && impactImpulse > 0f)
            rb.AddForceAtPosition(-hitNormal * impactImpulse, hitPoint, ForceMode.Impulse);

        onHealth01Changed?.Invoke(health / maxHealth);

        if (health <= 0f) Die();
    }

    void Die()
    {
        if (dead) return;
        dead = true;

        if (deathVFX)
        {
            var fx = Instantiate(deathVFX, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }
        if (audioSource && deathSfx) audioSource.PlayOneShot(deathSfx);

        onDeath?.Invoke();

        if (destroyOnDeath) Destroy(gameObject);
        else enabled = false;
    }

    // опционально: доступ к значени€м
    public float CurrentHealth => health;
    public float MaxHealth => maxHealth;
}
