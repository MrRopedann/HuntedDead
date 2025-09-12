using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSlot : MonoBehaviour
{
    [Header("Bind")]
    public Transform rightHandSocket;            // узел на правой руке
    public GameObject[] pistolPrefabs;           // префабы с узлом "Attach_R" внутри

    [Header("Input (HotBar)")]
    public InputActionReference nextSlot;        // Button (например E)
    public InputActionReference prevSlot;        // Button (например Q)
    public InputActionReference[] selectKeys;    // Buttons 1..N (цифры)

    [Header("Switching")]
    public float switchCooldown = 0.20f;

    public int CurrentIndex { get; private set; } = -1;
    public Transform Mounted { get; private set; }

    float nextSwitchTime;

    void OnEnable()
    {
        if (nextSlot) { nextSlot.action.Enable(); nextSlot.action.performed += OnNext; }
        if (prevSlot) { prevSlot.action.Enable(); prevSlot.action.performed += OnPrev; }

        if (selectKeys != null)
        {
            for (int i = 0; i < selectKeys.Length; i++)
            {
                int ix = i; var a = selectKeys[i];
                if (a == null) continue;
                a.action.Enable();
                a.action.performed += _ => TryEquip(ix);
            }
        }
    }

    void OnDisable()
    {
        if (nextSlot) { nextSlot.action.performed -= OnNext; nextSlot.action.Disable(); }
        if (prevSlot) { prevSlot.action.performed -= OnPrev; prevSlot.action.Disable(); }

        if (selectKeys != null)
            for (int i = 0; i < selectKeys.Length; i++)
            {
                var a = selectKeys[i];
                if (a == null) continue;
                a.action.Disable();
            }
    }

    void Start()
    {
        if (!rightHandSocket) { Debug.LogError("WeaponSlot: не задан rightHandSocket"); return; }
        if (pistolPrefabs != null && pistolPrefabs.Length > 0) Equip(0);
    }

    // --- callbacks ---
    void OnNext(InputAction.CallbackContext _) => TryCycle(+1);
    void OnPrev(InputAction.CallbackContext _) => TryCycle(-1);

    void TryCycle(int dir)
    {
        if (Time.time < nextSwitchTime) return;
        Cycle(dir);
        nextSwitchTime = Time.time + switchCooldown;
    }

    void TryEquip(int index)
    {
        if (Time.time < nextSwitchTime) return;
        Equip(index);
        nextSwitchTime = Time.time + switchCooldown;
    }

    // --- logic ---
    public void Cycle(int dir)
    {
        if (pistolPrefabs == null || pistolPrefabs.Length == 0) return;
        int next = (CurrentIndex + dir + pistolPrefabs.Length) % pistolPrefabs.Length;
        Equip(next);
    }

    public void Equip(int index)
    {
        if (pistolPrefabs == null || pistolPrefabs.Length == 0) return;
        index = Mathf.Clamp(index, 0, pistolPrefabs.Length - 1);
        if (index == CurrentIndex && Mounted) return;

        // удалить старый
        if (Mounted) { Destroy(Mounted.gameObject); Mounted = null; }

        // создать новый
        var prefab = pistolPrefabs[index];
        if (!prefab) { Debug.LogError("WeaponSlot: пустой префаб"); return; }

        var root = Instantiate(prefab);
        var attach = FindAttach(root.transform);
        if (!attach)
        {
            Debug.LogError($"WeaponSlot: Attach_R не найден в {prefab.name}");
            Destroy(root);
            return;
        }

        attach.SetParent(rightHandSocket, false);
        Mounted = attach;

        if (root.transform != attach) Destroy(root);
        CurrentIndex = index;
    }

    Transform FindAttach(Transform root)
    {
        var t = root.Find("Attach_R");
        if (t) return t;
        foreach (var tr in root.GetComponentsInChildren<Transform>(true))
            if (tr.name == "Attach_R") return tr;
        return null;
    }
}
