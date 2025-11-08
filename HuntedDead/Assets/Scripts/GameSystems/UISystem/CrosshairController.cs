using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CrosshairController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] crosshairs;
    [Range(0, 100)] public int index = 0;

    [Header("Appearance")]
    public Color color = Color.white;
    public float size = 32f;

    Image img;
    RectTransform rt;

    void Awake()
    {
        img = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
        ApplySprite();
        img.color = color;
        SetSize(size);
    }

    void ApplySprite()
    {
        if (crosshairs != null && crosshairs.Length > 0)
            img.sprite = crosshairs[Mathf.Clamp(index, 0, crosshairs.Length - 1)];
    }

    void SetSize(float s) => rt.sizeDelta = new Vector2(s, s);
}
