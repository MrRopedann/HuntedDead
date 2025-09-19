using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterStandalone : MonoBehaviour
{
    [SerializeField] WeaponSwitcherStandalone switcher;

    [Header("HUD")]
    [SerializeField] TMP_Text ammoLabel;
    [SerializeField] TMP_Text modeLabel;
    [SerializeField] bool autoBindLabels = true;
    [SerializeField] string ammoLabelName = "AmmoLabel";
    [SerializeField] string modeLabelName = "ModeLabel";

    InputAction fire, reload, aim, toggleMode;
    IWeaponTestable w;
    bool aiming;

    void Awake()
    {
        if (autoBindLabels)
        {
            if (!ammoLabel) ammoLabel = FindLabel(ammoLabelName);
            if (!modeLabel) modeLabel = FindLabel(modeLabelName);
        }

        if (ammoLabel) ammoLabel.text = "— / —";
        if (modeLabel) modeLabel.text = "MODE: —";
    }

    TMP_Text FindLabel(string targetName)
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas)
        {
            foreach (var l in canvas.GetComponentsInChildren<TMP_Text>(true))
                if (l.name == targetName) return l;
            foreach (var l in canvas.GetComponentsInChildren<TMP_Text>(true))
                if (l.name.Contains(targetName)) return l;
        }
        var go = GameObject.Find(targetName);
        return go ? go.GetComponent<TMP_Text>() : null;
    }

    void OnEnable()
    {
        fire = new InputAction(name: "Fire", type: InputActionType.Button, binding: "<Mouse>/leftButton");
        reload = new InputAction(name: "Reload", type: InputActionType.Button, binding: "<Keyboard>/r");
        aim = new InputAction(name: "Aim", type: InputActionType.Button, binding: "<Mouse>/rightButton");
        toggleMode = new InputAction(name: "ToggleMode", type: InputActionType.Button, binding: "<Keyboard>/b");
        fire.Enable(); reload.Enable(); aim.Enable(); toggleMode.Enable();
    }

    void OnDisable()
    {
        fire?.Disable(); reload?.Disable(); aim?.Disable(); toggleMode?.Disable();
    }

    void Update()
    {

        var go = switcher ? switcher.Current : null;
        var nw = go ? go.GetComponentInChildren<IWeaponTestable>() : null;

        if (!ReferenceEquals(nw, w))
        {
            if (w != null) w.StopAim();
            w = nw;
            aiming = false;

            if (w == null)
            {
                if (ammoLabel) ammoLabel.text = "— / —";
                if (modeLabel) modeLabel.text = "MODE: —";
            }
            else
            {
                if (ammoLabel) ammoLabel.text = $"{w.CurrentAmmo} / {w.ReserveAmmo}";
                if (modeLabel) modeLabel.text = $"MODE: {w.FireModeName}";
            }
        }
        if (w == null) return;

        if (toggleMode.triggered)
        {
            w.CycleFireMode();
            if (modeLabel) modeLabel.text = $"MODE: {w.FireModeName}";
        }

        bool aimPressed = aim.IsPressed();
        if (aimPressed != aiming) { aiming = aimPressed; if (aiming) w.StartAim(); else w.StopAim(); }

        if (reload.triggered) w.Reload();

        if (w.IsAutomatic) { if (fire.IsPressed()) w.Fire(); }
        else { if (fire.triggered) w.Fire(); }

        if (ammoLabel) ammoLabel.text = $"{w.CurrentAmmo} / {w.ReserveAmmo}";
    }
}
