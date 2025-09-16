using UnityEngine;
using UnityEngine.UI;

public class DragGhost : MonoBehaviour
{
    public Image img;
    bool on;

    public void Show(Sprite s) { img.sprite = s; img.enabled = true; on = true; }
    public void Hide() { img.enabled = false; on = false; }
    void Update() { if (on) transform.position = Input.mousePosition; }
}
