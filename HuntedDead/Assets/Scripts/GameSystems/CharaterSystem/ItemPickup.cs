using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    private ItemObject itemObject;
    public Item item;

    [Header("Rotation")]
    public float rotationSpeed = 45f;

    [Header("Light Settings")]
    public Transform lightOrigin;       // Пустой объект под предметом
    public Light spotLight;             // Пучок вверх
    public Light pointLight;            // Обволакивающее свечение
    public float spotRange = 5f;
    public float spotAngle = 30f;
    public float spotIntensity = 4f;
    public float pointBaseIntensity = 3f;
    public float pointRange = 2f;

    [Header("Point Light Gradient")]
    public float height = 2f;           // Высота, на которой свет полностью гаснет
    public AnimationCurve intensityCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    void Awake()
    {
        itemObject = GetComponent<ItemObject>();
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Start()
    {
        if (item == null && itemObject != null)
            item = itemObject.item;

        if (item == null)
        {
            Debug.LogError($"[ItemPickup] У объекта {name} не задан Item.");
            return;
        }

        // Создаем пустой объект под предметом, если не задан
        if (lightOrigin == null)
        {
            GameObject originGO = new GameObject("LightOrigin");
            originGO.transform.SetParent(transform);
            originGO.transform.localPosition = Vector3.zero;
            lightOrigin = originGO.transform;
        }

        // Spot Light - пучок вверх
        if (spotLight == null)
        {
            GameObject spotGO = new GameObject("SpotLight");
            spotGO.transform.SetParent(lightOrigin);
            spotGO.transform.localPosition = Vector3.zero;
            spotLight = spotGO.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.range = spotRange;
            spotLight.spotAngle = spotAngle;
            spotLight.intensity = spotIntensity;
            spotLight.shadows = LightShadows.None;
            spotLight.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        }

        // Point Light - обволакивающее свечение
        if (pointLight == null)
        {
            GameObject pointGO = new GameObject("PointLight");
            pointGO.transform.SetParent(transform);
            pointGO.transform.localPosition = Vector3.zero;
            pointLight = pointGO.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.range = pointRange;
            pointLight.intensity = pointBaseIntensity;
            pointLight.shadows = LightShadows.None;
        }

        // Цвет редкости
        Color rarityColor = item.GetRarityColor();
        spotLight.color = rarityColor;
        pointLight.color = rarityColor;
    }

    void Update()
    {
        // Вращаем предмет
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Градиент яркости Point Light сверху вниз
        if (pointLight)
        {
            float relativeHeight = Mathf.Clamp01((transform.position.y - lightOrigin.position.y) / height);
            pointLight.intensity = pointBaseIntensity * intensityCurve.Evaluate(relativeHeight);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var inventory = other.GetComponent<Inventory>();
            if (inventory != null && itemObject != null && itemObject.item != null)
            {
                inventory.AddItem(itemObject.item);
                Destroy(gameObject);
            }
        }
    }
}
