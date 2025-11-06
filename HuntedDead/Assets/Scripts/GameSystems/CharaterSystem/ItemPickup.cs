using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    private ItemObject itemObject;
    public Item item;
    public Light glowLight;
    public float rotationSpeed = 45f;

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

        // Создаём свет, если его нет
        if (glowLight == null)
        {
            glowLight = new GameObject("GlowLight").AddComponent<Light>();
            glowLight.type = LightType.Point;          // точечный свет
            glowLight.transform.SetParent(transform);  // привязка к предмету
            glowLight.transform.localPosition = Vector3.zero; // центр предмета
            glowLight.range = 1f;       // радиус свечения
            glowLight.intensity = 10f;   // насыщенность
            glowLight.shadows = LightShadows.None; // отключаем тени
        }

        glowLight.color = item.GetRarityColor(); // цвет из редкости

    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
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
