using UnityEngine;
using UnityEngine.UI;

public struct HotbarEntry
{
    public VariantKey key;
    public ItemKind kind;
    public Sprite icon;
    public bool valid;
}

public class HotbarController : MonoBehaviour
{
    public HotbarDef def;
    public Image[] slotIcons;
    public Image[] slotCooldowns;

    [HideInInspector] public HotbarEntry[] entries;

    public int selected;
    float _cooldownUntil;

    void Awake()
    {
        if (entries == null || entries.Length != slotIcons.Length)
            entries = new HotbarEntry[slotIcons.Length];
        for (int i = 0; i < slotIcons.Length; i++) if (slotIcons[i]) slotIcons[i].enabled = false;
    }

    public void Assign(int i, in GridItem item)
    {
        if (i < 0 || i >= slotIcons.Length) return;
        entries[i] = new HotbarEntry
        {
            key = item.stack.key,
            kind = item.def.kind,
            icon = item.def.icon,
            valid = true
        };
        if (slotIcons[i]) { slotIcons[i].sprite = item.def.icon; slotIcons[i].enabled = true; }
    }

    public void Clear(int i)
    {
        if (i < 0 || i >= slotIcons.Length) return;
        entries[i] = default;
        if (slotIcons[i]) slotIcons[i].enabled = false;
    }

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
