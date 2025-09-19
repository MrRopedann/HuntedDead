using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class BulletTrail : MonoBehaviour
{
    TrailRenderer tr;
    Vector3 target;
    float speed;
    float lifeAfter;
    bool activeMove;

    public void Init(Vector3 start, Vector3 end, float speed, float lifeAfter)
    {
        if (!tr) tr = GetComponent<TrailRenderer>();
        transform.position = start;
        transform.rotation = Quaternion.LookRotation((end - start).normalized);
        target = end;
        this.speed = Mathf.Max(0.01f, speed);
        this.lifeAfter = Mathf.Max(0f, lifeAfter);
        if (tr) tr.Clear();
        activeMove = true;
    }

    void Update()
    {
        if (!activeMove) return;
        var t = transform;
        var to = target - t.position;
        float step = speed * Time.deltaTime;

        if (to.sqrMagnitude <= step * step)
        {
            t.position = target;
            activeMove = false;
            StartCoroutine(Despawn());
        }
        else
        {
            t.position += to.normalized * step;
        }
    }

    IEnumerator Despawn()
    {
        if (lifeAfter > 0f) yield return new WaitForSeconds(lifeAfter);
        Destroy(gameObject);
    }
}
