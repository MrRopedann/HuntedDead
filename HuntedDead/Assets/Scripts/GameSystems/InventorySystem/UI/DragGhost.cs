using UnityEngine;
using UnityEngine.UI;
public class DragGhost : MonoBehaviour
{
    public Image img; public Vector2 defaultSize = new(64, 64);
    void Awake() { if (img) { img.enabled = false; img.raycastTarget = false; } }
    public void Show(Sprite s) { img.sprite = s; img.enabled = true; ((RectTransform)img.transform).sizeDelta = defaultSize; }
    public void Hide() { img.enabled = false; }
}
