using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterStandalone : MonoBehaviour
{
    [Header("ќружие")]
    [SerializeField] WeaponSwitcherStandalone switcher;
    [SerializeField] GameObject fixedWeapon;

    [Header("HUD")]
    [SerializeField] TMP_Text ammoLabel;
    [SerializeField] TMP_Text modeLabel;
    [SerializeField] bool autoBindLabels = true;
    [SerializeField] string ammoLabelName = "AmmoLabel";
    [SerializeField] string modeLabelName = "ModeLabel";

    [Header("Crosshair")]
    [SerializeField] RectTransform crosshairRect;
    [SerializeField] Camera playerCamera;

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

        if (ammoLabel) ammoLabel.text = "Ч / Ч";
        if (modeLabel) modeLabel.text = "MODE: Ч";
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
        fire = new InputAction("Fire", InputActionType.Button, "<Mouse>/leftButton");
        reload = new InputAction("Reload", InputActionType.Button, "<Keyboard>/r");
        aim = new InputAction("Aim", InputActionType.Button, "<Mouse>/rightButton");
        toggleMode = new InputAction("ToggleMode", InputActionType.Button, "<Keyboard>/b");

        fire.Enable(); reload.Enable(); aim.Enable(); toggleMode.Enable();
    }

    void OnDisable()
    {
        fire?.Disable(); reload?.Disable(); aim?.Disable(); toggleMode?.Disable();
    }

    void Update()
    {
        // выбор оружи€
        GameObject go = switcher?.Current ?? fixedWeapon;
        var nw = go ? go.GetComponentInChildren<IWeaponTestable>() : null;

        if (!ReferenceEquals(nw, w))
        {
            w?.StopAim();
            w = nw;
            aiming = false;

            if (w == null)
            {
                if (ammoLabel) ammoLabel.text = "Ч / Ч";
                if (modeLabel) modeLabel.text = "MODE: Ч";
            }
            else
            {
                if (ammoLabel) ammoLabel.text = $"{w.CurrentAmmo} / {w.ReserveAmmo}";
                if (modeLabel) modeLabel.text = $"MODE: {w.FireModeName}";
            }
        }

        if (w == null) return;

        // смена режима
        if (toggleMode.triggered)
        {
            w.CycleFireMode();
            if (modeLabel) modeLabel.text = $"MODE: {w.FireModeName}";
        }

        // прицеливание
        bool aimPressed = aim.IsPressed();
        if (aimPressed != aiming)
        {
            aiming = aimPressed;
            if (aiming) w.StartAim();
            else w.StopAim();
        }

        // перезар€дка
        if (reload.triggered)
            w.Reload();

        // стрельба
        if (w.IsAutomatic)
        {
            if (fire.IsPressed()) FireWeapon();
        }
        else
        {
            if (fire.triggered) FireWeapon();
        }

        // обновление UI
        if (ammoLabel) ammoLabel.text = $"{w.CurrentAmmo} / {w.ReserveAmmo}";
    }

    void FireWeapon()
    {
        if (w is Gun g)
        {
            g.Fire(GetAimDirection());
        }
        else
        {
            w.Fire(); // ƒл€ других типов оружи€
        }
    }

    public Vector3 GetAimDirection()
    {
        if (!crosshairRect || !playerCamera) return transform.forward;

        // ѕозици€ прицела на экране
        Vector3 screenPos = crosshairRect.position;
        Ray ray = playerCamera.ScreenPointToRay(screenPos);

        return ray.direction; // —трел€ем по направлению луча
    }

}
