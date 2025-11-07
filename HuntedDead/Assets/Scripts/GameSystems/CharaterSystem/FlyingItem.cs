using UnityEngine;

public class FlyingItem : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private float duration = 1f; // время полета
    private float timer = 0f;
    private float height = 2f;   // высота дуги
    private Vector3 initialScale;

    void Start()
    {
        startPos = transform.position;
        initialScale = transform.localScale;
        transform.localScale = Vector3.zero; // начинаем с нуля
    }

    public void Launch(Vector3 target, float flightDuration = 1f, float arcHeight = 2f)
    {
        targetPos = target;
        duration = flightDuration;
        height = arcHeight;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        // Параболическая дуга
        float yOffset = 4f * height * t * (1f - t); // простая парабола
        Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
        pos.y += yOffset;
        transform.position = pos;

        // Плавное увеличение масштаба
        transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, t);

        if (t >= 1f)
        {
            // Полёт завершен — включаем физику, если нужно
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            Destroy(this); // удаляем скрипт после завершения
        }
    }
}
