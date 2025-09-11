using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Image))]
public class CrosshairController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] crosshairs;         // набор спрайтов
    [Range(0, 100)] public int index;   // текущий

    [Header("Appearance")]
    public Color color = Color.white;
    public float sizeNormal = 32f;      // px
    public float sizeAim = 24f;         // px
    public float fadeIn = 16f;          // скорость по€влени€
    public float fadeOut = 12f;         // скорость исчезновени€
    public bool showOnlyWhenAiming = true;

    [Header("Input")]
    public InputActionReference aim;     // та же ѕ ћ, что и в камере/контроллере

    [Header("Optional spread (px add)")]
    public float spreadPixels = 0f;      // можно мен€ть с оружи€

    Image img;
    RectTransform rt;
    float alpha; // 0..1

    void OnEnable() { aim?.action.Enable(); }
    void OnDisable() { aim?.action.Disable(); }

    void Awake()
    {
        img = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
        ApplySprite();
        img.color = color;
        SetSize(sizeNormal);
        alpha = showOnlyWhenAiming ? 0f : 1f;
        SetAlpha(alpha);
    }

    void Update()
    {
        bool isAiming = aim && aim.action.IsPressed();
        float targetAlpha = showOnlyWhenAiming ? (isAiming ? 1f : 0f) : 1f;
        float k = (targetAlpha > alpha ? fadeIn : fadeOut) * Time.deltaTime;
        alpha = Mathf.MoveTowards(alpha, targetAlpha, k);
        SetAlpha(alpha);

        // размер
        float baseSize = isAiming ? sizeAim : sizeNormal;
        SetSize(baseSize + spreadPixels);
    }

    public void SetIndex(int i)
    {
        index = Mathf.Clamp(i, 0, crosshairs.Length - 1);
        ApplySprite();
    }

    public void Next()
    {
        if (crosshairs == null || crosshairs.Length == 0) return;
        index = (index + 1) % crosshairs.Length;
        ApplySprite();
    }

    public void SetSpread(float px) => spreadPixels = Mathf.Max(0, px);
    public void SetColor(Color c) { color = c; img.color = new Color(c.r, c.g, c.b, alpha); }

    void ApplySprite()
    {
        if (crosshairs != null && crosshairs.Length > 0 && index < crosshairs.Length && index >= 0)
            img.sprite = crosshairs[index];
    }

    void SetAlpha(float a) { var c = img.color; c.a = a; img.color = c; }

    void SetSize(float px) { rt.sizeDelta = new Vector2(px, px); }
}
