using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractUI : MonoBehaviour
{
    public CanvasGroup group;
    public TextMeshProUGUI label;
    public Image progress;

    public void Show(string text)
    {
        if (label) label.text = text;
        if (group) group.alpha = 1f;
    }
    public void Hide()
    {
        if (group) group.alpha = 0f;
        SetProgress(0f);
    }
    public void SetProgress(float v)
    {
        if (progress) { progress.type = Image.Type.Filled; progress.fillMethod = Image.FillMethod.Radial360; progress.fillOrigin = (int)Image.Origin360.Top; progress.fillAmount = Mathf.Clamp01(v); }
    }
}
