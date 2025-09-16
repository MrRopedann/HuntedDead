using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour, IWeaponTestable
{
    [SerializeField] GunConfig config;

    [System.Serializable]
    public class Sockets
    {
        public Transform muzzle;
        public Transform mag;
        public Transform magHome;
        public Transform magOut;
        public Transform dropSpawn;
        public AudioSource audio;
    }
    [SerializeField] Sockets sockets = new Sockets();

    [Header("Debug")]
    [SerializeField] bool drawDebug = false;

    // runtime
    int ammoInMag, reserve;
    float nextFireTime;
    bool isReloading, isAiming;
    Coroutine burstRoutine;
    ParticleSystem[] muzzlePs;
    GunConfig.FireModes fireMode;

    // === IWeaponTestable ===
    public bool IsAutomatic => (fireMode & GunConfig.FireModes.Auto) != 0 && fireMode == GunConfig.FireModes.Auto;
    public int CurrentAmmo => ammoInMag;
    public int ReserveAmmo => reserve;
    public string DisplayName => config ? config.displayName : name;
    public string FireModeName => fireMode.ToString().ToUpperInvariant();
    public void CycleFireMode() { fireMode = NextEnabledMode(fireMode); }

    // ===== Lifecycle =====
    void Awake()
    {
        if (!config) { Debug.LogError("Gun: config is null", this); enabled = false; return; }
        AutoBindSockets(); EnsureAudio();
        ammoInMag = Mathf.Max(0, config.magazineSize);
        reserve = Mathf.Max(0, config.startReserve);
        fireMode = FirstEnabledMode();
    }

    void Start()
    {
        if (config.reuseMuzzleVFX && config.muzzleVFXPrefab && sockets.muzzle)
        {
            var fx = Instantiate(config.muzzleVFXPrefab, sockets.muzzle.position, sockets.muzzle.rotation, sockets.muzzle);
            muzzlePs = fx.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in muzzlePs)
            {
                var m = ps.main; m.loop = false; m.playOnAwake = false;
                var em = ps.emission; em.rateOverTime = 0f;
                if (em.burstCount == 0) em.SetBurst(0, new ParticleSystem.Burst(0f, 1));
            }
        }
    }

    // ===== Public API =====
    public void StartAim() => isAiming = true;
    public void StopAim() => isAiming = false;

    public void Reload()
    {
        if (isReloading) return;
        if (ammoInMag >= config.magazineSize) return;
        if (reserve <= 0) return;
        if (burstRoutine != null) { StopCoroutine(burstRoutine); burstRoutine = null; }
        StartCoroutine(CoReloadMag());
    }

    public void Fire()
    {
        if (isReloading) return;

        // Burst запускается один раз
        if (fireMode == GunConfig.FireModes.Burst)
        {
            if (burstRoutine == null && CanStartShot())
                burstRoutine = StartCoroutine(CoBurst());
            return;
        }

        // Semi/Auto
        if (CanStartShot())
        {
            DoSingleShot();
            nextFireTime = Time.time + 60f / Mathf.Max(1f, config.rpm);
        }
    }

    // ===== Internals =====
    bool CanStartShot()
    {
        if (Time.time < nextFireTime) return false;
        if (ammoInMag <= 0)
        {
            if (config.drySfx && sockets.audio) sockets.audio.PlayOneShot(config.drySfx);
            return false;
        }
        return true;
    }

    IEnumerator CoBurst()
    {
        int shots = Mathf.Min(config.burstCount, ammoInMag);
        float dt = 60f / Mathf.Max(1f, config.burstRpm);

        for (int i = 0; i < shots; i++)
        {
            if (isReloading || ammoInMag <= 0) break;
            DoSingleShot();
            if (i < shots - 1) yield return new WaitForSeconds(dt);
        }

        nextFireTime = Time.time + 60f / Mathf.Max(1f, config.rpm);
        burstRoutine = null;
    }

    void DoSingleShot()
    {
        ammoInMag--;

        // Muzzle VFX
        if (config.reuseMuzzleVFX && muzzlePs != null && muzzlePs.Length > 0)
        {
            foreach (var ps in muzzlePs)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
            }
        }
        else if (config.muzzleVFXPrefab && sockets.muzzle)
        {
            var fx = Instantiate(config.muzzleVFXPrefab, sockets.muzzle.position, sockets.muzzle.rotation, sockets.muzzle);
            var all = fx.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in all) ps.Play(true);
            Destroy(fx, ComputeTTL(all));
        }

        if (config.shotSfx && sockets.audio) sockets.audio.PlayOneShot(config.shotSfx);

        float spread = isAiming ? config.spreadAimDeg : config.spreadHipDeg;
        int shots = Mathf.Max(1, config.pellets);

        for (int i = 0; i < shots; i++)
        {
            Vector3 origin = sockets.muzzle ? sockets.muzzle.position : transform.position;
            Vector3 dir = ApplySpread((sockets.muzzle ? sockets.muzzle.forward : transform.forward), spread);
            Vector3 hitPoint = origin + dir * config.range;

            if (Physics.Raycast(origin, dir, out var hit, config.range, config.hitMask, QueryTriggerInteraction.Ignore))
            {
                hitPoint = hit.point;
                var dmg = hit.collider.GetComponentInParent<IDamageable>();
                if (dmg != null) dmg.TakeDamage(config.damage, hit.point, hit.normal);

                if (config.impactVFX)
                {
                    var fx = Instantiate(config.impactVFX, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(fx, 2f);
                }
            }

            if (config.trailVFX) SpawnTracer(origin, hitPoint);
            if (drawDebug) Debug.DrawLine(origin, hitPoint, Color.red, 0.2f);
        }
    }

    IEnumerator CoReloadMag()
    {
        isReloading = true;

        // вынуть
        yield return MoveMag(sockets.magHome, sockets.magOut, config.magOutTime, config.magOutSfx);

        // клон магазина
        if (config.droppedMagPrefab && sockets.mag)
        {
            var t = sockets.dropSpawn ? sockets.dropSpawn : sockets.mag;
            var dm = Instantiate(config.droppedMagPrefab, t.position, t.rotation);
            var rb = dm.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddForce(transform.right * 0.6f + transform.up * 0.8f, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 1.2f, ForceMode.Impulse);
            }
            Destroy(dm, 6f);
        }

        if (config.reloadHoldTime > 0f) yield return new WaitForSeconds(config.reloadHoldTime);

        // вставить
        yield return MoveMag(sockets.magOut, sockets.magHome, config.magInTime, config.magInSfx);

        // пополнить
        int need = config.magazineSize - ammoInMag;
        int take = Mathf.Min(need, reserve);
        ammoInMag += take; reserve -= take;

        isReloading = false;
    }

    IEnumerator MoveMag(Transform from, Transform to, float time, AudioClip sfx)
    {
        if (!sockets.mag || !from || !to || time <= 0f)
        {
            if (sfx && sockets.audio) sockets.audio.PlayOneShot(sfx);
            yield return new WaitForSeconds(Mathf.Max(0.01f, time));
            yield break;
        }

        if (sfx && sockets.audio) sockets.audio.PlayOneShot(sfx);

        Vector3 p0 = from.localPosition, p1 = to.localPosition;
        Quaternion r0 = from.localRotation, r1 = to.localRotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float k = config.magCurve.Evaluate(Mathf.Clamp01(t));
            sockets.mag.localPosition = Vector3.LerpUnclamped(p0, p1, k);
            sockets.mag.localRotation = Quaternion.SlerpUnclamped(r0, r1, k);
            yield return null;
        }
        sockets.mag.localPosition = p1; sockets.mag.localRotation = r1;
    }

    // ===== Helpers =====
    void SpawnTracer(Vector3 start, Vector3 end)
    {
        var go = Instantiate(config.trailVFX, start, Quaternion.LookRotation(end - start));
        var bt = go.GetComponent<BulletTrail>(); if (!bt) bt = go.AddComponent<BulletTrail>();
        bt.Init(start, end, config.tracerSpeed, config.tracerLifeAfter);
    }

    Vector3 ApplySpread(Vector3 fwd, float spreadDeg)
    {
        if (spreadDeg <= 0f) return fwd;
        float ang = Random.Range(0f, spreadDeg);
        float az = Random.Range(0f, 360f);
        Quaternion q = Quaternion.AngleAxis(ang, Quaternion.AngleAxis(az, fwd) * Vector3.up);
        return q * fwd;
    }

    float ComputeTTL(ParticleSystem[] systems)
    {
        float max = 0.2f;
        foreach (var ps in systems)
        {
            var m = ps.main;
            float dur = m.duration;
            float life = m.startLifetime.mode == ParticleSystemCurveMode.TwoConstants
                ? m.startLifetime.constantMax
                : m.startLifetime.constant;
            max = Mathf.Max(max, dur + life);
        }
        return Mathf.Clamp(max, 0.05f, 5f);
    }

    GunConfig.FireModes FirstEnabledMode()
    {
        if ((config.enabledModes & GunConfig.FireModes.Semi) != 0) return GunConfig.FireModes.Semi;
        if ((config.enabledModes & GunConfig.FireModes.Burst) != 0) return GunConfig.FireModes.Burst;
        return GunConfig.FireModes.Auto;
    }

    GunConfig.FireModes NextEnabledMode(GunConfig.FireModes cur)
    {
        for (int i = 0; i < 3; i++)
        {
            cur = (GunConfig.FireModes)(((int)cur << 1) & 0b111);
            if (cur == 0) cur = GunConfig.FireModes.Semi;
            if ((config.enabledModes & cur) != 0) return cur;
        }
        return cur;
    }

    void EnsureAudio()
    {
        if (!sockets.audio)
        {
            sockets.audio = gameObject.GetComponent<AudioSource>();
            if (!sockets.audio) sockets.audio = gameObject.AddComponent<AudioSource>();
            sockets.audio.playOnAwake = false; sockets.audio.loop = false;
            sockets.audio.spatialBlend = 0f; sockets.audio.volume = 0.8f;
        }
    }

    [ContextMenu("Auto-Bind Sockets")]
    public void AutoBindSockets()
    {
        // ищем по именам
        if (!sockets.muzzle) sockets.muzzle = FindDeep("Muzzle");
        if (!sockets.mag) sockets.mag = FindDeep("Mag");
        if (!sockets.magHome) sockets.magHome = FindDeep("MagHome");
        if (!sockets.magOut) sockets.magOut = FindDeep("MagOut");
        if (!sockets.dropSpawn) sockets.dropSpawn = FindDeep("DropSpawn");

        // создаём недостающие
        if (sockets.mag && !sockets.magHome)
        {
            sockets.magHome = new GameObject("MagHome").transform;
            sockets.magHome.SetParent(sockets.mag.parent, false);
            sockets.magHome.localPosition = sockets.mag.localPosition;
            sockets.magHome.localRotation = sockets.mag.localRotation;
        }
        if (sockets.mag && !sockets.magOut)
        {
            sockets.magOut = new GameObject("MagOut").transform;
            sockets.magOut.SetParent(sockets.mag.parent, false);
            sockets.magOut.localPosition = sockets.magHome.localPosition + config.magOutOffset;
            sockets.magOut.localRotation = sockets.magHome.localRotation * Quaternion.Euler(config.magOutEuler);
        }
    }

    Transform FindDeep(string name)
    {
        var tfs = GetComponentsInChildren<Transform>(true);
        foreach (var t in tfs) if (t.name == name) return t;
        return null;
    }
}
