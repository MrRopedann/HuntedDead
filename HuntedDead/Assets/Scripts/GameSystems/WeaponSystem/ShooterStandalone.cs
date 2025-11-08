using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterStandalone : MonoBehaviour
{
    [Header("ќружие")]
    [SerializeField] private WeaponSwitcherStandalone switcher;
    [SerializeField] private GameObject fixedWeapon;

    [Header("HUD")]
    [SerializeField] private TMP_Text ammoLabel;
    [SerializeField] private TMP_Text modeLabel;
    [SerializeField] private bool autoBindLabels = true;
    [SerializeField] private string ammoLabelName = "AmmoLabel";
    [SerializeField] private string modeLabelName = "ModeLabel";

    [Header("Crosshair 3D")]
    [SerializeField] private Crosshair3D crosshair3D; // ссылка на 3D прицел

    [Header("Player Camera")]
    [SerializeField] private Camera playerCamera;

    private InputAction fire, reload, aim, toggleMode;
    private IWeaponTestable currentWeapon;
    private bool aiming;

    private void Awake()
    {
        if (autoBindLabels)
        {
            ammoLabel ??= FindLabel(ammoLabelName);
            modeLabel ??= FindLabel(modeLabelName);
        }

        if (ammoLabel) ammoLabel.text = "Ч / Ч";
        if (modeLabel) modeLabel.text = "MODE: Ч";

        if (crosshair3D) crosshair3D.gameObject.SetActive(false);
    }

    private TMP_Text FindLabel(string targetName)
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            foreach (var l in canvas.GetComponentsInChildren<TMP_Text>(true))
                if (l.name == targetName || l.name.Contains(targetName)) return l;
        }

        var go = GameObject.Find(targetName);
        return go ? go.GetComponent<TMP_Text>() : null;
    }

    private void OnEnable()
    {
        fire = new InputAction("Fire", InputActionType.Button, "<Mouse>/leftButton");
        reload = new InputAction("Reload", InputActionType.Button, "<Keyboard>/r");
        aim = new InputAction("Aim", InputActionType.Button, "<Mouse>/rightButton");
        toggleMode = new InputAction("ToggleMode", InputActionType.Button, "<Keyboard>/b");

        fire.Enable(); reload.Enable(); aim.Enable(); toggleMode.Enable();
    }

    private void OnDisable()
    {
        fire?.Disable(); reload?.Disable(); aim?.Disable(); toggleMode?.Disable();
    }

    private void Update()
    {
        // ¬ыбор оружи€
        GameObject go = switcher?.Current ?? fixedWeapon;
        var nw = go ? go.GetComponentInChildren<IWeaponTestable>() : null;

        if (!ReferenceEquals(nw, currentWeapon))
        {
            currentWeapon?.StopAim();
            currentWeapon = nw;
            aiming = false;
            UpdateCrosshair();
            UpdateHUD();
        }

        if (currentWeapon == null) return;

        // —мена режима стрельбы
        if (toggleMode.triggered)
        {
            currentWeapon.CycleFireMode();
            UpdateHUD();
        }

        // ѕрицеливание
        bool aimPressed = aim.IsPressed();
        if (aimPressed != aiming)
        {
            aiming = aimPressed;
            if (aiming) currentWeapon.StartAim();
            else currentWeapon.StopAim();

            if (crosshair3D) crosshair3D.SetAiming(aiming);
        }

        // ѕерезар€дка
        if (reload.triggered) currentWeapon.Reload();

        // —трельба только при прицеливании
        if (aiming)
        {
            if (currentWeapon.IsAutomatic && fire.IsPressed()) FireWeapon();
            else if (!currentWeapon.IsAutomatic && fire.triggered) FireWeapon();
        }

        // ќбновление HUD
        UpdateHUD();
    }

    private void FireWeapon()
    {
        if (currentWeapon is Gun gun)
        {
            Vector3 aimPoint = GetAimPoint();
            Vector3 origin = gun.sockets.muzzle ? gun.sockets.muzzle.position : gun.transform.position;
            Vector3 dir = (aimPoint - origin).normalized;
            gun.Fire(dir);
        }
        else
        {
            currentWeapon.Fire();
        }
    }

    // ѕолучаем точку в мире, куда указывает прицел
    private Vector3 GetAimPoint()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            return hit.point;
        return ray.origin + ray.direction * 100f; // точка на 100 метров, если ничего не попали
    }

    private void UpdateCrosshair()
    {
        if (crosshair3D) crosshair3D.SetAiming(aiming);
    }

    private void UpdateHUD()
    {
        if (ammoLabel) ammoLabel.text = currentWeapon != null ? $"{currentWeapon.CurrentAmmo} / {currentWeapon.ReserveAmmo}" : "Ч / Ч";
        if (modeLabel) modeLabel.text = currentWeapon != null ? $"MODE: {currentWeapon.FireModeName}" : "MODE: Ч";
    }
}
