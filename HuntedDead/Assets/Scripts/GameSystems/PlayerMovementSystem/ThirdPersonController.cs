using UnityEngine;
using UnityEngine.InputSystem;

public interface ICrouchProvider { bool IsCrouching { get; } }

public interface IMovementNoiseProvider
{
    bool IsMoving { get; }
    bool IsRunning { get; }   // бег (не спринт)
    bool IsSprinting { get; }
    bool JustJumped { get; }   // короткий флаг после прыжка
}

public class ThirdPersonController : MonoBehaviour, ICrouchProvider, IMovementNoiseProvider
{
    [Header("Move Speeds")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4f;
    public float sprintSpeed = 5f;
    public float crouchMultiplier = 0.5f;

    [Header("Jump/Gravity")]
    public float jumpForce = 4.0f;
    public float gravity = 9.8f;
    public float groundedBuffer = 0.06f;

    [Header("Fall thresholds")]
    public float fallVelThreshold = 1.5f;
    public float minAirTime = 0.20f;
    public float minFallDistance = 0.40f;

    [Header("Crouch capsule")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1.2f;
    public float heightLerp = 18f;
    public LayerMask headBlockMask = ~0;

    [Header("Noise flags thresholds")]
    [Tooltip("Доля от walkSpeed для IsMoving")]
    public float moveFrac = 0.15f;
    [Tooltip("Доля от runSpeed для IsRunning")]
    public float runFrac = 0.75f;
    [Tooltip("Сколько секунд держать JustJumped")]
    public float jumpedFlagTime = 0.25f;

    // runtime
    bool isCrouching;
    public bool IsCrouching => isCrouching;

    bool isWalking;
    float inX, inZ;
    bool inJump, inSprint;

    // crouch input
    bool crouchHeld;
    bool wantStandUp;

    // noise/state runtime
    float currPlanarSpeed;     // м/с (обновляется в FixedUpdate)
    float jumpedTimer;         // таймер JustJumped

    Animator animator;
    CharacterController cc;

    float velY;
    float groundedTimer;
    bool wasGrounded = true;

    float airTime;
    float leaveGroundY;

    [Header("Input System (assign in Inspector)")]
    public InputActionReference move;
    public InputActionReference jump;
    public InputActionReference sprint;
    public InputActionReference crouch;
    public InputActionReference walk;

    // Animator hashes
    static readonly int P_Speed = Animator.StringToHash("Speed");
    static readonly int P_Grounded = Animator.StringToHash("Grounded");
    static readonly int P_Crouch = Animator.StringToHash("Crouch");
    static readonly int P_Jump = Animator.StringToHash("Jump");
    static readonly int P_Land = Animator.StringToHash("Land");
    static readonly int P_VelY = Animator.StringToHash("VelY");
    static readonly int P_IsFalling = Animator.StringToHash("IsFalling");

    public bool cameraDrivesYawInAim = true;
    public bool isAiming = false;

    // капсула
    float targetHeight;
    Vector3 defaultCenter;

    // ==== IMovementNoiseProvider ====
    public bool IsSprinting => inSprint && !isCrouching;
    public bool IsRunning => !IsSprinting && !isCrouching && currPlanarSpeed >= runSpeed * runFrac;
    public bool IsMoving => currPlanarSpeed >= Mathf.Max(0.05f, walkSpeed * moveFrac);
    public bool JustJumped => jumpedTimer > 0f;
    // =================================

    void OnEnable()
    {
        move?.action.Enable();
        jump?.action.Enable();
        sprint?.action.Enable();
        crouch?.action.Enable();
        if (walk != null) { walk.action.Enable(); walk.action.performed += OnWalkPerformed; walk.action.canceled += OnWalkCanceled; }
    }
    void OnDisable()
    {
        move?.action.Disable();
        jump?.action.Disable();
        sprint?.action.Disable();
        crouch?.action.Disable();
        if (walk != null) { walk.action.performed -= OnWalkPerformed; walk.action.canceled -= OnWalkCanceled; walk.action.Disable(); }
    }

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (cc.height <= 0f) cc.height = standHeight;
        standHeight = Mathf.Max(standHeight, 1.0f);
        crouchHeight = Mathf.Clamp(crouchHeight, 0.6f, standHeight - 0.2f);
        defaultCenter = cc.center;
        targetHeight = standHeight;
        ApplyCapsuleInstant();
    }

    void Update()
    {
        // INPUT
        Vector2 mv = move ? move.action.ReadValue<Vector2>() : Vector2.zero;
        inX = mv.x; inZ = mv.y;
        inJump = jump && jump.action.IsPressed();
        crouchHeld = crouch && crouch.action.IsPressed();
        inSprint = sprint && sprint.action.IsPressed() && !isCrouching;

        // Grounded буфер
        if (cc.isGrounded) groundedTimer = groundedBuffer;
        else groundedTimer -= Time.deltaTime;
        bool groundedBuffered = groundedTimer > 0f;

        bool hasMoveInput = mv.sqrMagnitude > 0.0001f;

        // присед: удержание + отложенное вставание
        if (crouchHeld && cc.isGrounded) { isCrouching = true; wantStandUp = false; }
        if (!crouchHeld && isCrouching)
        {
            if (CanStandUp()) { isCrouching = false; wantStandUp = false; }
            else wantStandUp = true;
        }
        if (wantStandUp && CanStandUp()) { isCrouching = false; wantStandUp = false; }

        // воздух
        if (groundedBuffered) { airTime = 0f; leaveGroundY = transform.position.y; }
        else { if (wasGrounded) leaveGroundY = transform.position.y; airTime += Time.deltaTime; }

        // Jump
        if (inJump && groundedBuffered && !isCrouching)
        {
            animator?.SetTrigger(P_Jump);
            velY = jumpForce;
            groundedTimer = 0f;
            jumpedTimer = jumpedFlagTime; // включаем флаг шума прыжка
        }
        if (jumpedTimer > 0f) jumpedTimer -= Time.deltaTime;

        // целевая скорость для анимации
        float desiredMS = 0f;
        if (groundedBuffered && hasMoveInput)
        {
            if (isCrouching) desiredMS = (isWalking ? walkSpeed : runSpeed) * Mathf.Clamp01(crouchMultiplier);
            else if (inSprint) desiredMS = sprintSpeed;
            else if (isWalking) desiredMS = walkSpeed;
            else desiredMS = runSpeed;
        }
        float speed01 = Mathf.InverseLerp(0f, sprintSpeed, desiredMS);

        // падение
        float fallDistance = Mathf.Max(0f, leaveGroundY - transform.position.y);
        bool shouldFall = !groundedBuffered &&
                          ((velY < -fallVelThreshold && airTime >= minAirTime) || (fallDistance >= minFallDistance));

        // Animator
        if (animator)
        {
            animator.SetBool(P_Grounded, groundedBuffered);
            animator.SetBool(P_Crouch, isCrouching);
            animator.SetBool(P_IsFalling, shouldFall);
            animator.SetFloat(P_Speed, speed01, 0.1f, Time.deltaTime);
            animator.SetFloat(P_VelY, velY);
            if (!wasGrounded && groundedBuffered && velY <= 0f) animator.SetTrigger(P_Land);
        }
        wasGrounded = groundedBuffered;

        // капсула
        float wanted = isCrouching ? crouchHeight : standHeight;
        if (!Mathf.Approximately(targetHeight, wanted)) targetHeight = wanted;
        SmoothCapsule();

        HeadHittingDetect();
    }

    void FixedUpdate()
    {
        // итоговая скорость
        float speed;
        if (inSprint && !isCrouching && !isAiming) speed = sprintSpeed;
        else if (isWalking && !inSprint) speed = walkSpeed;
        else speed = isAiming ? Mathf.Min(runSpeed, sprintSpeed) * 0.85f : runSpeed;

        if (isCrouching) speed *= Mathf.Clamp01(crouchMultiplier);

        // текущая горизонтальная скорость для noise-флагов
        float inputMag = Mathf.Clamp01(new Vector2(inX, inZ).magnitude);
        currPlanarSpeed = speed * inputMag;

        // вертикаль
        velY -= gravity * Time.deltaTime;
        if (cc.isGrounded && velY < 0f) velY = 0f;
        float dy = velY * Time.deltaTime;

        // движение относительно камеры
        Vector3 f = Camera.main.transform.forward; f.y = 0f; f.Normalize();
        Vector3 r = Camera.main.transform.right; r.y = 0f; r.Normalize();

        Vector3 planar = f * (inZ * speed * Time.deltaTime) + r * (inX * speed * Time.deltaTime);

        // ротация
        if (!(isAiming && cameraDrivesYawInAim))
        {
            if (inputMag > 0f)
            {
                float angle = Mathf.Atan2(planar.x, planar.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, angle, 0f), 0.15f);
            }
        }

        // перемещение
        cc.Move(planar + Vector3.up * dy);
    }

    // капсула: плавная смена высоты и центра
    void SmoothCapsule()
    {
        if (!cc) return;
        float k = 1f - Mathf.Exp(-heightLerp * Time.deltaTime);
        float newH = Mathf.Lerp(cc.height, targetHeight, k);

        float standCenterY = defaultCenter.y;
        float crouchCenterY = defaultCenter.y - (standHeight - crouchHeight) * 0.5f;
        float newCenterY = Mathf.Lerp(cc.center.y, isCrouching ? crouchCenterY : standCenterY, k);

        cc.height = newH;
        cc.center = new Vector3(defaultCenter.x, newCenterY, defaultCenter.z);
    }

    void ApplyCapsuleInstant()
    {
        cc.height = standHeight;
        cc.center = defaultCenter;
    }

    bool CanStandUp()
    {
        float needed = (standHeight - cc.height);
        if (needed <= 0.01f) return true;

        Vector3 origin = transform.position + Vector3.up * (cc.radius + 0.02f);
        float castDist = needed + 0.05f;
        float radius = Mathf.Max(0.05f, cc.radius * 0.95f);

        return !Physics.SphereCast(origin, radius, Vector3.up, out _, castDist, headBlockMask, QueryTriggerInteraction.Ignore);
    }

    void HeadHittingDetect()
    {
        float headHitDistance = 1.1f;
        Vector3 ccCenter = transform.TransformPoint(cc.center);
        float hitCalc = cc.height / 2f * headHitDistance;
        if (Physics.Raycast(ccCenter, Vector3.up, hitCalc)) { if (velY > 0f) velY = 0f; }
    }

    // Walk (Hold)
    void OnWalkPerformed(InputAction.CallbackContext _) { if (cc && cc.isGrounded) isWalking = true; }
    void OnWalkCanceled(InputAction.CallbackContext _) { isWalking = false; }
}
