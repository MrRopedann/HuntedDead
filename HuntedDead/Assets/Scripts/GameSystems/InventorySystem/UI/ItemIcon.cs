using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemIcon : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI qty;
    public Image qualityFrame;

    public void Bind(Sprite s, int count, Color qColor)
    {
        icon.sprite = s;
        qty.text = count > 1 ? count.ToString() : "";
        if (qualityFrame) qualityFrame.color = qColor;
    }
}
