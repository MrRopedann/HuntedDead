using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemIcon : MonoBehaviour
{
    public Image iconImage;               // укажи именно слой иконки
    public TextMeshProUGUI countText;

    public void Bind(Sprite s, int qty, Color qcol)
    {
        if (!iconImage) return;

        iconImage.sprite = s;
        iconImage.color = Color.white;
        iconImage.enabled = s != null;
        iconImage.preserveAspect = false;

        // занять весь слот, чтобы поворот не вылезал за рамку
        var rt = iconImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localEulerAngles = Vector3.zero;   // сброс поворота по умолчанию

        if (countText) countText.text = qty > 1 ? qty.ToString() : "";
    }

    public void SetRotated(bool rotated)
    {
        if (!iconImage) return;
        var rt = iconImage.rectTransform;
        rt.localEulerAngles = rotated ? new Vector3(0, 0, 90f) : Vector3.zero;
    }
}
