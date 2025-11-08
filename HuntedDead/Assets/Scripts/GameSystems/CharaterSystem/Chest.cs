using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootTable
{
    public ItemRarity rarity;
    [Range(0f, 1f)] public float chance;
    public List<GameObject> itemPrefabs;
}

[RequireComponent(typeof(Collider))]
public class Chest : Interactable
{
    [Header("Loot Settings")]
    public List<LootTable> lootTables;
    public int scrapCost = 1;

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float dropForce = 2f;

    [Header("Animation")]
    public Transform lid;
    public float openAngle = 70f;
    public float openSpeed = 2f;

    [Header("UI")]
    public TextMesh costText;

    private bool isOpened = false;
    private float currentAngle = 0f;

    void Start()
    {
        interactionLabel = $"Открыть ({scrapCost} скрап)";
        if (costText != null)
            costText.text = scrapCost.ToString();
    }

    void Update()
    {
        if (isOpened && lid != null && currentAngle < openAngle)
        {
            float step = openSpeed * Time.deltaTime;
            float nextAngle = Mathf.Min(currentAngle + step, openAngle);
            lid.localRotation = Quaternion.Euler(-nextAngle, 0f, 0f);
            currentAngle = nextAngle;
        }
    }

    public override void Interact(Inventory inv)
    {
        if (isOpened || inv == null) return;

        if (!inv.SpendScrap(scrapCost))
        {
            Debug.Log("Недостаточно скрапа!");
            return;
        }

        isOpened = true;
        isUsed = true;

        if (costText != null)
            costText.gameObject.SetActive(false);

        LootTable table = GetRandomLootTable();
        if (table == null || table.itemPrefabs.Count == 0) return;

        GameObject prefab = table.itemPrefabs[Random.Range(0, table.itemPrefabs.Count)];
        SpawnItem(prefab);
    }

    private LootTable GetRandomLootTable()
    {
        float total = 0f;
        foreach (var t in lootTables) total += t.chance;
        float rand = Random.Range(0f, total);
        float sum = 0f;
        foreach (var t in lootTables)
        {
            sum += t.chance;
            if (rand <= sum) return t;
        }
        return lootTables.Count > 0 ? lootTables[^1] : null;
    }

    private void SpawnItem(GameObject prefab)
    {
        if (spawnPoint == null) spawnPoint = transform;
        GameObject itemGO = Instantiate(prefab, spawnPoint.position + Vector3.up * 0.5f, Quaternion.identity);
        Rigidbody rb = itemGO.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        FlyingItem fly = itemGO.AddComponent<FlyingItem>();
        Vector3 target = spawnPoint.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        fly.Launch(target, 1f, 2f);
    }
}
