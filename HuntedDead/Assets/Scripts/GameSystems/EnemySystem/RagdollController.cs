using UnityEngine;

[DisallowMultipleComponent]
public class RagdollController : MonoBehaviour
{
    [Header("Ragdoll parts")]
    public Rigidbody[] ragdollBodies;
    public Collider[] ragdollColliders;
    public Animator animator;

    void Reset()
    {
        // автосбор частей
        ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
        ragdollColliders = GetComponentsInChildren<Collider>(true);
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (ragdollBodies == null || ragdollBodies.Length == 0)
            ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
        if (ragdollColliders == null || ragdollColliders.Length == 0)
            ragdollColliders = GetComponentsInChildren<Collider>(true);
        if (animator == null) animator = GetComponentInChildren<Animator>();
        DisableRagdoll();
    }

    public void EnableRagdoll()
    {
        if (animator) animator.enabled = false;
        foreach (var rb in ragdollBodies)
        {
            if (rb == null) continue;
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
        foreach (var c in ragdollColliders)
        {
            if (c == null) continue;
            c.enabled = true;
        }
    }

    public void DisableRagdoll()
    {
        foreach (var rb in ragdollBodies)
        {
            if (rb == null) continue;
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
        foreach (var c in ragdollColliders)
        {
            if (c == null) continue;
            // NOTE: убедись, что у меша есть основной коллайдер, который должен остатьс€ включенным
            // если нужно Ч отключаем все child-коллайдеры кроме root
            //c.enabled = false;
        }
        if (animator) animator.enabled = true;
    }

    public void ApplyImpulse(Vector3 impulse, Vector3 worldPoint)
    {
        // добавл€ем силу к ближайшему Rigidbody
        Rigidbody nearest = null;
        float best = float.MaxValue;
        foreach (var rb in ragdollBodies)
        {
            if (rb == null) continue;
            float d = Vector3.Distance(rb.worldCenterOfMass, worldPoint);
            if (d < best) { best = d; nearest = rb; }
        }
        if (nearest != null) nearest.AddForceAtPosition(impulse, worldPoint, ForceMode.Impulse);
    }
}
