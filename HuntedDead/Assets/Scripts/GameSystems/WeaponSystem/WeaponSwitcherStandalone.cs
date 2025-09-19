#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwitcherStandalone : MonoBehaviour
{
    [Header("Mount")]
    [SerializeField] Transform mount;
    [SerializeField] Vector3 localPos;
    [SerializeField] Vector3 localEuler;

    [Header("Zoom")]
    [SerializeField] bool enableZoom = true;
    [SerializeField] float zoomMin = -1.5f;
    [SerializeField] float zoomMax = 1.5f;
    [SerializeField] float zoomSensitivity = 0.5f;
    float zoomZ = 0f;

    [Header("Weapons")]
    [SerializeField] bool autoScanResources = false;
    [SerializeField] string resourcesPath = "Weapons";
    [SerializeField] List<GameObject> weaponPrefabs = new();
    [SerializeField] int startIndex = 0;

    [Header("UI")]
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] Button nextBtn;
    [SerializeField] Button prevBtn;
    [SerializeField] TMP_Text activeLabel;

    [Header("Hotkeys")]
    [SerializeField] KeyCode nextKey = KeyCode.E;
    [SerializeField] KeyCode prevKey = KeyCode.Q;
    [SerializeField] bool numberKeys = true;

    [Header("Keyboard Rotate")]
    [SerializeField] bool rotateWithArrows = true;
    [SerializeField] float yawSpeed = 120f;
    [SerializeField] float pitchSpeed = 90f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;
    float yawOffset;
    float pitchOffset;

#if UNITY_EDITOR
    [Header("Editor Safety")]
    [SerializeField] Transform selectionFallback;
#endif

    GameObject currentInstance;
    int currentIndex = -1;
    bool isSwitching;

    void Awake()
    {
        if (!mount) mount = transform;
#if UNITY_EDITOR
        if (!selectionFallback) selectionFallback = mount ? mount : transform;
#endif
        if (autoScanResources && weaponPrefabs.Count == 0)
        {
            var loaded = Resources.LoadAll<GameObject>(resourcesPath);
            weaponPrefabs = new List<GameObject>(loaded);
        }
        weaponPrefabs.RemoveAll(p => p == null);

        if (dropdown)
        {
            dropdown.ClearOptions();
            var opts = new List<TMP_Dropdown.OptionData>();
            foreach (var p in weaponPrefabs) opts.Add(new TMP_Dropdown.OptionData(p.name));
            dropdown.AddOptions(opts);
            dropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        if (nextBtn) nextBtn.onClick.AddListener(Next);
        if (prevBtn) prevBtn.onClick.AddListener(Prev);

        if (weaponPrefabs.Count == 0) SetActiveLabel("—");
    }

    void Start()
    {
        if (weaponPrefabs.Count == 0) return;
        var idx = Mathf.Clamp(startIndex, 0, weaponPrefabs.Count - 1);
        if (dropdown) dropdown.value = idx;
        SwitchTo(idx);
    }

    void Update()
    {
        if (Input.GetKeyDown(nextKey)) Next();
        if (Input.GetKeyDown(prevKey)) Prev();

        if (numberKeys)
        {
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    int idx = i - 1;
                    if (idx < weaponPrefabs.Count)
                    {
                        if (dropdown) dropdown.value = idx; else SwitchTo(idx);
                    }
                }
            }
        }

        if (enableZoom && currentInstance)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > Mathf.Epsilon)
            {
                zoomZ = Mathf.Clamp(zoomZ + scroll * zoomSensitivity, zoomMin, zoomMax);
                ApplyTransform();
            }
        }

        if (rotateWithArrows && currentInstance)
        {
            float dt = Time.deltaTime;
            float yawDelta = 0f;
            if (Input.GetKey(KeyCode.RightArrow)) yawDelta += yawSpeed * dt;
            if (Input.GetKey(KeyCode.LeftArrow)) yawDelta -= yawSpeed * dt;

            float pitchDelta = 0f;
            if (Input.GetKey(KeyCode.UpArrow)) pitchDelta -= pitchSpeed * dt;
            if (Input.GetKey(KeyCode.DownArrow)) pitchDelta += pitchSpeed * dt;

            if (yawDelta != 0f || pitchDelta != 0f)
            {
                yawOffset += yawDelta;
                
                if (yawOffset > 360f) yawOffset -= 360f;
                else if (yawOffset < -360f) yawOffset += 360f;

                pitchOffset = Mathf.Clamp(pitchOffset + pitchDelta, minPitch, maxPitch);
                ApplyTransform();
            }
        }
    }

    void OnDropdownChanged(int idx)
    {
        if (!isSwitching) SwitchTo(idx);
    }

    void Next()
    {
        if (weaponPrefabs.Count == 0) return;
        var idx = (currentIndex + 1) % weaponPrefabs.Count;
        if (dropdown) dropdown.value = idx; else SwitchTo(idx);
    }

    void Prev()
    {
        if (weaponPrefabs.Count == 0) return;
        var idx = (currentIndex - 1 + weaponPrefabs.Count) % weaponPrefabs.Count;
        if (dropdown) dropdown.value = idx; else SwitchTo(idx);
    }

    void SwitchTo(int idx)
    {
        if (idx < 0 || idx >= weaponPrefabs.Count) return;
        if (idx == currentIndex) return;

        isSwitching = true;
        SafeDestroy(currentInstance);

        var prefab = weaponPrefabs[idx];
        if (!prefab) { SetActiveLabel("—"); isSwitching = false; return; }

        currentInstance = Instantiate(prefab, mount);
        currentIndex = idx;

        ApplyTransform();
        SetActiveLabel(prefab.name);
        isSwitching = false;
    }

    void ApplyTransform()
    {
        if (!currentInstance) return;
        var t = currentInstance.transform;
        t.localPosition = localPos + new Vector3(0f, 0f, zoomZ);
        t.localEulerAngles = localEuler + new Vector3(pitchOffset, yawOffset, 0f);
        t.localScale = Vector3.one;
    }

    void SetActiveLabel(string nameText)
    {
        if (activeLabel) activeLabel.text = $"Активно: {nameText}";
    }

    void SafeDestroy(GameObject go)
    {
        if (!go) return;

#if UNITY_EDITOR
        foreach (var sel in Selection.transforms)
        {
            if (sel == null) continue;
            if (sel.gameObject == go || sel.IsChildOf(go.transform))
            {
                Selection.activeObject = selectionFallback ? (Object)selectionFallback.gameObject : null;
                break;
            }
        }
        go.transform.SetParent(null, true);
        go.SetActive(false);

        if (Application.isPlaying)
            Destroy(go);
        else
            EditorApplication.delayCall += () => { if (go) DestroyImmediate(go); };
#else
        Destroy(go);
#endif
    }

    public GameObject Current => currentInstance;
    public int CurrentIndex => currentIndex;
}
