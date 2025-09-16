using UnityEngine;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    public HotbarDef def;
    public Image[] slotIcons;
    public Image[] slotCooldowns;
    public int selected;
    float _cooldownUntil;

    public bool CanSwitch(float now) => now >= _cooldownUntil;
    public void OnSwitched(float now) { _cooldownUntil = now + def.ringCooldownSeconds; UpdateCooldown(now); }
    public void UpdateCooldown(float now)
    {
        float left = Mathf.Max(0f, _cooldownUntil - now);
        float fill = def.ringCooldownSeconds <= 0f ? 0f : left / def.ringCooldownSeconds;
        for (int i = 0; i < slotCooldowns.Length; i++) if (slotCooldowns[i]) slotCooldowns[i].fillAmount = fill;
    }
    void Update() { UpdateCooldown(Time.time); }
}
