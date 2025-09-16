using UnityEngine;
using UnityEngine.UI;

public class GridCellView : MonoBehaviour
{
    public Image frame;
    public void SetHighlight(bool on) { if (frame) frame.enabled = on; }
}
