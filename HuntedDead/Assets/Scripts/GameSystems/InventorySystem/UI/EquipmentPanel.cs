using UnityEngine;
using UnityEngine.UI;

public class EquipmentPanel : MonoBehaviour
{
    public Image slotHead, slotChest, slotPrimary, slotSecondary, slotBack;

    public bool TryGetSlotUnderScreen(Vector2 screen, out string slot)
    {
        if (RectContains(slotHead, screen)) { slot = "Head"; return true; }
        if (RectContains(slotChest, screen)) { slot = "Chest"; return true; }
        if (RectContains(slotPrimary, screen)) { slot = "Primary"; return true; }
        if (RectContains(slotSecondary, screen)) { slot = "Secondary"; return true; }
        if (RectContains(slotBack, screen)) { slot = "Back"; return true; }
        slot = null; return false;
    }
    bool RectContains(Image img, Vector2 screen)
    {
        if (!img) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(img.rectTransform, screen);
    }
}
