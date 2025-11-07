using UnityEngine;
using TMPro;

[RequireComponent(typeof(Chest), typeof(Collider))]
public class ChestUIProximity3D : MonoBehaviour
{
    public TextMeshPro scrapTextPrefab; // 3D TextMeshPro префаб
    public Transform textPosition;      // Точка над сундуком
    public float verticalOffset = 2f;

    private TextMeshPro scrapTextInstance;
    private bool playerNearby = false;
    private Chest chest;

    void Awake()
    {
        chest = GetComponent<Chest>();

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (scrapTextPrefab != null)
        {
            scrapTextInstance = Instantiate(scrapTextPrefab, transform.position + Vector3.up * verticalOffset, Quaternion.identity);
            scrapTextInstance.text = $"Открыть: {chest.scrapCost} скрап";
            scrapTextInstance.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Не задан префаб TextMeshPro 3D!");
        }
    }

    void Update()
    {
        if (scrapTextInstance != null && playerNearby)
        {
            Vector3 pos = (textPosition != null ? textPosition.position : transform.position) + Vector3.up * verticalOffset;
            scrapTextInstance.transform.position = pos;

            Camera cam = Camera.main;
            if (cam != null)
                scrapTextInstance.transform.rotation = Quaternion.LookRotation(scrapTextInstance.transform.position - cam.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && scrapTextInstance != null)
        {
            scrapTextInstance.gameObject.SetActive(true);
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && scrapTextInstance != null)
        {
            scrapTextInstance.gameObject.SetActive(false);
            playerNearby = false;
        }
    }
}
