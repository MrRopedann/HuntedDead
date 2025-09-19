using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Управление")]
    public bool clickToMoveCamera = false;
    public float mouseSensitivity = 5f;
    public Vector2 cameraLimit = new Vector2(-45, 40);

    [Header("Дистанция и коллизии")]
    public float desiredDistance = 3.5f;
    public float aimDistance = 1.8f;
    public float minDistance = 1.0f;
    public float collisionRadius = 0.25f;
    public LayerMask occluderMask = ~0;
    public float distanceLerp = 12f;
    public float collisionInset = 0.05f;

    [Header("Смещение камеры (локально)")]
    public Vector2 shoulderOffset = new Vector2(0.35f, 0.15f);
    public Vector2 shoulderOffsetAim = new Vector2(0.90f, 0.10f);
    public float headLookHeight = 1.5f;

    [Header("FOV")]
    public float fovNormal = 60f;
    public float fovAim = 45f;
    public float aimLerp = 15f;

    [Header("Скрытие мешающих объектов")]
    public bool hideObstructors = true;
    public float hideWhenDistanceBelow = 1.25f;

    [Header("Ссылки")]
    public Transform playerRoot;
    public Transform cameraTransform;

    [Header("Input System")]
    public InputActionReference look;
    public InputActionReference lookHold;
    public InputActionReference aim;

    [Header("Аим-режим")]
    public bool hardLockBehindOnAim = true;
    public bool rotatePlayerYawInAim = true;

    [SerializeField] bool _uiBlocked;

    float yaw, pitch;
    float offsetY;
    float currentDistance;
    float aimAlpha;

    Transform player;
    Camera cam;



    readonly HashSet<Renderer> hidden = new();
    readonly List<Renderer> tmpToKeep = new();

    void OnEnable() { look?.action.Enable(); lookHold?.action.Enable(); aim?.action.Enable(); }
    void OnDisable() { look?.action.Disable(); lookHold?.action.Disable(); aim?.action.Disable(); UnhideAll(); }

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        if (!playerRoot) playerRoot = player;
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;

        currentDistance = Mathf.Max(minDistance, desiredDistance);
        if (player) offsetY = transform.position.y - player.position.y;

        cam = cameraTransform ? cameraTransform.GetComponent<Camera>() : null;
        if (!clickToMoveCamera) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        if (playerRoot) yaw = playerRoot.eulerAngles.y;
        if (cam) cam.fieldOfView = fovNormal;
    }

    void LateUpdate()
    {
        if (!player || !cameraTransform) return;

        transform.position = player.position + new Vector3(0, offsetY, 0);

        bool isAiming = !_uiBlocked && aim != null && aim.action.IsPressed();
        bool allowRotate =
            !_uiBlocked && (
                isAiming || !clickToMoveCamera ||
                (clickToMoveCamera && lookHold != null && lookHold.action.IsPressed())
            );

        Vector2 delta = allowRotate && look != null ? look.action.ReadValue<Vector2>() : Vector2.zero;
        yaw += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, cameraLimit.x, cameraLimit.y);

        if (isAiming && hardLockBehindOnAim)
        {
            if (rotatePlayerYawInAim && playerRoot)
                playerRoot.rotation = Quaternion.Euler(0f, yaw, 0f);
            else if (playerRoot)
                yaw = playerRoot.eulerAngles.y;

            transform.rotation = Quaternion.Euler(pitch, playerRoot ? playerRoot.eulerAngles.y : yaw, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        float kAim = 1f - Mathf.Exp(-aimLerp * Time.deltaTime);
        aimAlpha = Mathf.Lerp(aimAlpha, isAiming ? 1f : 0f, kAim);

        Vector2 off = Vector2.Lerp(shoulderOffset, shoulderOffsetAim, aimAlpha);
        Vector3 lateralUp = transform.right * off.x + transform.up * off.y;
        float targetDistBase = Mathf.Lerp(desiredDistance, aimDistance, aimAlpha);

        Vector3 anchor = transform.position + transform.up * headLookHeight;
        Vector3 pivot = transform.position + lateralUp;
        Vector3 backDir = -transform.forward;

        float blockedDist = CastForBlock(pivot, backDir, targetDistBase, out bool blocked);
        float goalDist = blocked ? blockedDist : targetDistBase;
        currentDistance = Mathf.Lerp(currentDistance, goalDist, 1f - Mathf.Exp(-distanceLerp * Time.deltaTime));

        Vector3 camPos = pivot + backDir * currentDistance;
        cameraTransform.position = camPos;
        cameraTransform.rotation = transform.rotation;

        if (cam) cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, isAiming ? fovAim : fovNormal, kAim);

        if (hideObstructors)
        {
            bool shouldHide = blocked && currentDistance <= hideWhenDistanceBelow;
            if (shouldHide) HideBetweenCameraAndAnchor(anchor, camPos);
            else UnhideAll();
        }
    }


    public void SetUiBlock(bool blocked)
    {
        _uiBlocked = blocked;
        if (blocked) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        else if (!clickToMoveCamera) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }

    float CastForBlock(Vector3 origin, Vector3 dir, float maxDist, out bool blocked)
    {
        blocked = false; float hitDist = maxDist; float nearest = float.PositiveInfinity;
        var hits = Physics.SphereCastAll(new Ray(origin, dir), collisionRadius, maxDist, occluderMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.collider == null || h.collider.isTrigger) continue;
            if (IsSelf(h.collider.transform)) continue;
            if (h.distance < nearest) nearest = h.distance;
        }
        if (!float.IsInfinity(nearest)) { blocked = true; hitDist = Mathf.Max(minDistance, nearest - collisionInset); }
        return hitDist;
    }

    void HideBetweenCameraAndAnchor(Vector3 anchor, Vector3 camPos)
    {
        tmpToKeep.Clear();
        Vector3 dir = (anchor - camPos); float dist = dir.magnitude;
        if (dist < 1e-4f) { UnhideAll(); return; }
        dir /= dist;

        var hits = Physics.RaycastAll(camPos, dir, dist, occluderMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.collider == null || h.collider.isTrigger) continue;
            if (IsSelf(h.collider.transform)) continue;

            var renderers = h.collider.GetComponentsInParent<Renderer>(true);
            for (int r = 0; r < renderers.Length; r++)
            {
                var rr = renderers[r];
                if (!hidden.Contains(rr)) { rr.enabled = false; hidden.Add(rr); }
                tmpToKeep.Add(rr);
            }
        }

        if (hidden.Count > 0)
        {
            var toUnhide = ListPool<Renderer>.Get();
            foreach (var rr in hidden) if (!tmpToKeep.Contains(rr)) toUnhide.Add(rr);
            for (int i = 0; i < toUnhide.Count; i++) { toUnhide[i].enabled = true; hidden.Remove(toUnhide[i]); }
            ListPool<Renderer>.Release(toUnhide);
        }
    }

    void UnhideAll() { if (hidden.Count == 0) return; foreach (var rr in hidden) if (rr) rr.enabled = true; hidden.Clear(); }
    bool IsSelf(Transform t) { if (!playerRoot) return false; return t.root == playerRoot.root; }

    static class ListPool<T>
    {
        static readonly Stack<List<T>> pool = new();
        public static List<T> Get() => pool.Count > 0 ? pool.Pop() : new List<T>(16);
        public static void Release(List<T> list) { list.Clear(); pool.Push(list); }
    }
}
