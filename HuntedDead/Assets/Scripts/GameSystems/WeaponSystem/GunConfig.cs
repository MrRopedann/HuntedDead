using UnityEngine;

[CreateAssetMenu(fileName = "GunConfig", menuName = "Weapons/Gun Config")]
public class GunConfig : ScriptableObject
{
    public string displayName = "Gun";

    [Header("Stats")]
    public float damage = 20f;
    public float rpm = 600f;                 // базовый темп для SEMI/AUTO
    public float range = 200f;
    public float spreadHipDeg = 0.6f;
    public float spreadAimDeg = 0.15f;
    public int magazineSize = 30;
    public int startReserve = 120;
    public int pellets = 1;
    public LayerMask hitMask = ~0;

    [Header("Reload")]
    public float reloadHoldTime = 0.25f;
    public float magOutTime = 0.35f;
    public float magInTime = 0.45f;
    public Vector3 magOutOffset = new Vector3(0f, -0.12f, 0.06f);
    public Vector3 magOutEuler = new Vector3(10f, 0f, 0f);
    public AnimationCurve magCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public GameObject droppedMagPrefab;
    public AudioClip magOutSfx, magInSfx;

    [Header("VFX")]
    public GameObject muzzleVFXPrefab;       // FX_Gunshot_01
    public bool reuseMuzzleVFX = false;
    public GameObject trailVFX;              // Bullet_Trail_FX
    public GameObject impactVFX;

    [Header("Tracer")]
    public float tracerSpeed = 180f;
    public float tracerLifeAfter = 0.15f;

    [Header("SFX")]
    public AudioClip shotSfx, drySfx;

    [System.Flags]
    public enum FireModes { Semi = 1, Burst = 2, Auto = 4 }
    [Header("Fire Modes")]
    public FireModes enabledModes = FireModes.Semi | FireModes.Auto;
    public int burstCount = 3;
    public float burstRpm = 900f;
}
