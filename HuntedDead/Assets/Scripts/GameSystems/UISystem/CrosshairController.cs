using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CrosshairController3D : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] crosshairs;
    [Range(0, 100)] public int index = 0;

    [Header("Appearance")]
    public Color color = Color.white;
    public float size = 0.1f; // размер в мире

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        ApplySprite();
        sr.color = color;
        SetSize(size);
    }

    private void ApplySprite()
    {
        if (crosshairs != null && crosshairs.Length > 0)
            sr.sprite = crosshairs[Mathf.Clamp(index, 0, crosshairs.Length - 1)];
    }

    private void SetSize(float s) => transform.localScale = new Vector3(s, s, 1f);
}
