using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScrapContainer : Interactable
{
    [Header("Scrap Settings")]
    public int minScrap = 1;
    public int maxScrap = 5;

    [Header("Lid Animation")]
    public Transform lid;          // —сылка на крышку
    public float openAngle = 90f;  // ”гол открыти€
    public float openSpeed = 2f;   // —корость
    public bool hingeOnSide = false; // true Ч петли сбоку, false Ч сверху

    private bool isOpened = false;
    private float currentAngle = 0f;

    void Start()
    {
        interactionLabel = "ќбыскать";
    }

    void Update()
    {
        if (isOpened && lid != null && currentAngle < openAngle)
        {
            float step = openSpeed * Time.deltaTime;
            float nextAngle = Mathf.Min(currentAngle + step, openAngle);
            // если петли сверху Ч открываетс€ назад, если сбоку Ч вбок
            Vector3 axis = hingeOnSide ? Vector3.forward : Vector3.right;
            lid.localRotation = Quaternion.AngleAxis(-nextAngle, axis);
            currentAngle = nextAngle;
        }
    }

    public override void Interact(Inventory inv)
    {
        if (isOpened || inv == null) return;

        int scrapFound = Random.Range(minScrap, maxScrap + 1);
        inv.AddScrap(scrapFound);

        isOpened = true;
        isUsed = true;

        Debug.Log($"»грок получил {scrapFound} скрапа");
    }
}
