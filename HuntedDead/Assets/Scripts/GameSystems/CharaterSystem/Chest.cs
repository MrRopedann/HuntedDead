using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootTable
{
    public ItemRarity rarity;
    [Range(0f, 1f)] public float chance;
    public List<GameObject> itemPrefabs; // Префабы предметов
}

[RequireComponent(typeof(Collider))]
public class Chest : MonoBehaviour
{
    [Header("Loot Settings")]
    public List<LootTable> lootTables;
    public int scrapCost = 1;

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float dropForce = 2f;

    [Header("Chest Animation")]
    public Transform lid;
    public float openAngle = 70f;
    public float openSpeed = 2f;

    [Header("UI")]
    public TextMesh costText; // 3D текст над сундуком

    private Inventory playerInventory;
    private bool isOpened = false;
    private float currentAngle = 0f;

    void Start()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Inventory>();
        if (playerInventory == null)
            Debug.LogWarning("Не найден Inventory на Player!");

        // Показываем стоимость над сундуком в начале
        if (costText != null)
            costText.text = scrapCost.ToString();
    }

    void Update()
    {
        // Анимация открытия крышки
        if (isOpened && lid != null && currentAngle < openAngle)
        {
            float step = openSpeed * Time.deltaTime;
            float nextAngle = Mathf.Min(currentAngle + step, openAngle);
            lid.localRotation = Quaternion.Euler(-nextAngle, 0f, 0f);
            currentAngle = nextAngle;
        }
    }

    public void OpenChest()
    {
        if (isOpened) return;

        if (playerInventory == null) return;

        if (!playerInventory.SpendScrap(scrapCost))
        {
            Debug.Log("Недостаточно скрапа для открытия сундука!");
            return;
        }

        isOpened = true;

        // Скрываем стоимость над сундуком
        if (costText != null)
            costText.gameObject.SetActive(false);

        // Выбор редкости и спавн предмета
        LootTable selectedTable = GetRandomLootTable();
        if (selectedTable == null || selectedTable.itemPrefabs.Count == 0) return;

        GameObject prefabToSpawn = selectedTable.itemPrefabs[Random.Range(0, selectedTable.itemPrefabs.Count)];
        SpawnItem(prefabToSpawn);

        Debug.Log($"Сундук выдал предмет редкости {selectedTable.rarity}: {prefabToSpawn.name}");
    }

    private LootTable GetRandomLootTable()
    {
        float totalChance = 0f;
        foreach (var table in lootTables) totalChance += table.chance;

        float rand = Random.Range(0f, totalChance);
        float sum = 0f;

        foreach (var table in lootTables)
        {
            sum += table.chance;
            if (rand <= sum) return table;
        }

        return lootTables.Count > 0 ? lootTables[lootTables.Count - 1] : null;
    }

    private void SpawnItem(GameObject prefab)
    {
        if (spawnPoint == null) spawnPoint = transform;

        GameObject itemGO = Instantiate(prefab, spawnPoint.position + Vector3.up * 0.5f, Quaternion.identity);

        Rigidbody rb = itemGO.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true; // отключаем физику на время полета

        // Запускаем полёт
        FlyingItem fly = itemGO.AddComponent<FlyingItem>();
        Vector3 target = spawnPoint.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        fly.Launch(target, 1f, 2f); // 1 секунда, высота дуги 2
    }
}
