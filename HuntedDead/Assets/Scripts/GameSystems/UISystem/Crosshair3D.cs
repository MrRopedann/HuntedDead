using UnityEngine;

public class Crosshair3D : MonoBehaviour
{
    [Header("Settings")]
    public Transform cameraTransform;       // Камера игрока
    public float defaultDistance = 10f;     // Дистанция от камеры до прицела
    public LayerMask hitMask;               // По чему "стреляет" прицел

    [Header("Appearance")]
    public bool activeOnAim = true;         // Включать прицел только при прицеливании
    public float lookAtOffset = 0.01f;      // Чтобы спрайт чуть «смотрел» на камеру
    public float sizeMultiplier = 0.05f;    // Масштаб прицела (чем выше, тем больше)
    public float minSize = 0.1f;            // Минимальный размер прицела
    public float maxSize = 1f;              // Максимальный размер прицела

    private bool aiming;

    private void Update()
    {
        if (!activeOnAim) return;

        // Включаем/выключаем прицел
        gameObject.SetActive(aiming);
        if (!aiming) return;

        Vector3 targetPos;

        // Проверяем, попал ли луч во что-то
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hitMask))
        {
            targetPos = hit.point;
        }
        else
        {
            targetPos = cameraTransform.position + cameraTransform.forward * defaultDistance;
        }

        // Направление к точке попадания
        Vector3 dir = (targetPos - cameraTransform.position).normalized;

        // Ставим прицел на фиксированное расстояние перед камерой
        transform.position = cameraTransform.position + dir * defaultDistance;

        // Поворот к камере
        transform.LookAt(cameraTransform.position + dir * lookAtOffset);

        // Масштабируем прицел по расстоянию
        float distance = Vector3.Distance(cameraTransform.position, transform.position);
        float scale = Mathf.Clamp(distance * sizeMultiplier, minSize, maxSize);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetAiming(bool value)
    {
        aiming = value;
        gameObject.SetActive(aiming);
    }
}
