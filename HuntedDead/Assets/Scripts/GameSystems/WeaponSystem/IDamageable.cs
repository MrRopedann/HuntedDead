using UnityEngine;
using UnityEngine.Events;

public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
}

[System.Serializable] public class FloatEvent : UnityEvent<float> { }
[System.Serializable] public class VoidEvent : UnityEvent { }

public class Damageable : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] bool destroyOnDeath = true;

    [Header("Effects")]
    [SerializeField] GameObject hitVFX;
    [SerializeField] GameObject deathVFX;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip hitSfx;
    [SerializeField] AudioClip deathSfx;
    [SerializeField] float impactImpulse = 2f;

    [Header("Events")]
    [SerializeField] FloatEvent onHealth01Changed;
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

    public float CurrentHealth => health;
    public float MaxHealth => maxHealth;
}
