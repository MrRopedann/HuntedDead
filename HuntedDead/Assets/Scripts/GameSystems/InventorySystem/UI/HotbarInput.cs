using UnityEngine;

public class HotbarInput : MonoBehaviour
{
    public HotbarController view;
    public PlayerStateRelay stateRelay;

    public void SelectIndex(int i)
    {
        var now = Time.time;
        if (!view.CanSwitch(now)) return;
        var s = stateRelay.Current;
        if (s == PlayerState.Stunned) return;
        view.selected = i;
        view.OnSwitched(now);
    }
    public void Next()
    {
        int n = view.slotIcons.Length;
        SelectIndex((view.selected + 1 + n) % n);
    }
    public void Prev()
    {
        int n = view.slotIcons.Length;
        SelectIndex((view.selected - 1 + n) % n);
    }
}
