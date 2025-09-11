using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class ZombieAI : MonoBehaviour
{
    public enum State { Idle, Patrol, Alert, Chase, Attack, Search }
    public State state = State.Idle;

    [Header("Refs")]
    public Transform[] patrolPoints;
    public Transform eyes;
    public Transform target;
    public LayerMask obstacleMask = ~0;

    [Header("Vision")]
    public float viewDistance = 18f;
    [Range(0, 180)] public float viewAngle = 80f;
    [Range(0, 180)] public float peripheralAngle = 140f;
    public float peripheralDistance = 10f;

    [Header("Suspicion")]
    public float suspicionRise = 2.0f;
    public float suspicionFall = 1.0f;
    public float suspicionToChase = 1.0f;
    float suspicion;

    [Header("Hearing (3 rings)")]
    public bool ignoreHearingWhenTargetCrouched = true;
    public float hearNear = 3f;
    public float hearMid = 6f;
    public float hearFar = 10f;
    public float memoryTime = 4f;

    [Header("Move")]
    public float walkSpeed = 1.2f;
    public float runSpeed = 2.6f;
    public float turnLerp = 12f;

    [Header("Attack")]
    public float attackRange = 1.6f;
    public float attackCooldown = 1.0f;

    [Header("Off-Mesh Jump / Drop")]
    public bool useOffMeshLinks = true;
    public float jumpTime = 0.55f;
    public float extraUpVelocity = 1.0f;
    public float reprojectRadius = 2f;

    [Header("Search+Investigate")]
    public float investigateTime = 2.0f;
    public float investigateYawSpeed = 220f;
    public int wedgePoints = 4;
    [Range(15, 120)] public float wedgeHalfAngle = 45f;
    public float wedgeStepDist = 3.0f;
    public int innerRingPoints = 3;
    public float innerRingRadius = 2.0f;
    public float searchMaxTime = 25f;
    [Min(0)] public int revisitLostCount = 2;
    public float revisitSpacing = 2.5f;

    [Header("Prediction / Breadcrumbs")]
    public float predictionTime = 0.6f;
    public float breadcrumbRecordStep = 0.3f;
    public int breadcrumbMax = 8;

    NavMeshAgent agent;
    Rigidbody rb;
    ICrouchProvider targetCrouch;
    IMovementNoiseProvider targetMove;

    int patrolIx;
    float atkTimer;
    float lastSeenT = float.NegativeInfinity;
    Vector3 lastSeenPos;
    Vector3 lastSeenDir;
    bool hasLOS;
    bool traversing;

    readonly List<Vector3> searchPlan = new();
    int searchIdx;
    float searchTimer;
    float waitTimer;

    readonly Queue<Vector3> breadcrumbs = new();
    Vector3 lastTargetPos;
    Vector3 targetVel;
    float breadcrumbTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = true;

        if (!eyes) eyes = transform;
        if (!target) { var p = GameObject.FindWithTag("Player"); if (p) target = p.transform; }
        if (target)
        {
            targetCrouch = target.GetComponentInParent<ICrouchProvider>();
            targetMove = target.GetComponentInParent<IMovementNoiseProvider>();
            lastTargetPos = target.position;
        }

        if (!agent.isOnNavMesh) SnapToNavMesh();
        agent.autoTraverseOffMeshLink = false;

        SetState(patrolPoints != null && patrolPoints.Length > 0 ? State.Patrol : State.Idle);
    }

    void Update()
    {
        if (useOffMeshLinks && !traversing && agent.isOnOffMeshLink)
        {
            StartCoroutine(TraverseLinkBallistic());
            return;
        }

        SenseAndThink();
        atkTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Idle:
                if (CanEngage()) SetState(State.Chase);
                break;

            case State.Patrol:
                agent.speed = walkSpeed;
                if (CanEngage()) { SetState(State.Chase); break; }
                if (!agent.hasPath || agent.remainingDistance < 0.3f) NextPatrol();
                break;

            case State.Alert:
                agent.speed = runSpeed;
                SafeSetDestination(lastSeenPos);
                if (CanEngage()) { SetState(State.Chase); break; }
                if (Arrived(lastSeenPos, 0.5f)) SetState(State.Search);
                break;

            case State.Chase:
                agent.speed = runSpeed;
                if (target)
                {
                    Vector3 pred = PredictTargetPosition();
                    SafeSetDestination(pred);
                    if (DistToTarget() <= attackRange && hasLOS)
                        SetState(State.Attack);
                    else if (!Remembering() && suspicion < 0.25f)
                        SetState(State.Alert);
                }
                break;

            case State.Attack:
                agent.ResetPath();
                FaceTo(target ? target.position : transform.position + transform.forward);
                if (!target || DistToTarget() > attackRange * 1.1f || !hasLOS)
                { SetState(State.Chase); break; }
                if (atkTimer <= 0f) atkTimer = attackCooldown;
                break;

            case State.Search:
                RunSearch();
                break;
        }
    }

    void SenseAndThink()
    {
        hasLOS = false;
        if (!target) return;

        float dist = DistToTarget();

        Vector3 curPos = target.position;
        targetVel = (curPos - lastTargetPos) / Mathf.Max(0.0001f, Time.deltaTime);
        lastTargetPos = curPos;

        if (!(ignoreHearingWhenTargetCrouched && TargetIsCrouching()))
        {
            var n = ReadNoise();
            if (dist <= hearFar && (n.sprinting || n.justJumped)) { Remember(target.position); suspicion = Mathf.Min(1f, suspicion + suspicionRise * Time.deltaTime); }
            else if (dist <= hearMid && (n.running || n.sprinting || n.justJumped)) { Remember(target.position); suspicion = Mathf.Min(1f, suspicion + 0.7f * suspicionRise * Time.deltaTime); }
            else if (dist <= hearNear && n.moving) { Remember(target.position); suspicion = Mathf.Min(1f, suspicion + 0.4f * suspicionRise * Time.deltaTime); }
        }

        bool seen = false;
        if (CheckVision(viewAngle, viewDistance)) { seen = true; suspicion = 1f; }
        else if (CheckVision(peripheralAngle, peripheralDistance)) { suspicion = Mathf.Min(1f, suspicion + suspicionRise * Time.deltaTime); }

        if (!seen) suspicion = Mathf.Max(0f, suspicion - suspicionFall * Time.deltaTime);

        if (seen || state == State.Chase || state == State.Alert)
        {
            breadcrumbTimer -= Time.deltaTime;
            if (breadcrumbTimer <= 0f)
            {
                breadcrumbTimer = breadcrumbRecordStep;
                PushBreadcrumb(SampleOnNavMesh(target.position));
            }
        }

        if (suspicion >= suspicionToChase && state != State.Chase && state != State.Attack)
            SetState(State.Chase);
    }

    bool CheckVision(float fov, float maxDist)
    {
        Vector3 eye = eyes.position;
        Vector3 head = target.position + Vector3.up * 0.8f;
        Vector3 dir = (head - eye);
        if (dir.sqrMagnitude > maxDist * maxDist) return false;
        if (Vector3.Angle(eyes.forward, dir.normalized) > fov * 0.5f) return false;

        if (Physics.Linecast(eye, head, out var hit, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.root != target.root) return false;
        }

        hasLOS = true;
        Remember(target.position);
        return true;
    }

    struct Noise { public bool moving, running, sprinting, justJumped; }
    Noise ReadNoise()
    {
        if (targetMove != null)
            return new Noise { moving = targetMove.IsMoving, running = targetMove.IsRunning, sprinting = targetMove.IsSprinting, justJumped = targetMove.JustJumped };

        float v = targetVel.XZ().magnitude;
        return new Noise { moving = v > 0.05f, running = v > 1.5f, sprinting = v > 3.0f, justJumped = false };
    }

    bool TargetIsCrouching()
    {
        if (!target) return false;
        if (targetCrouch != null) return targetCrouch.IsCrouching;
        var cc = target.GetComponentInParent<CharacterController>();
        return cc && cc.height < 1.4f;
    }

    void Remember(Vector3 pos)
    {
        lastSeenDir = pos - (eyes ? eyes.position : transform.position);
        lastSeenDir.y = 0f;
        if (lastSeenDir.sqrMagnitude > 1e-4f) lastSeenDir.Normalize();

        lastSeenPos = pos;
        lastSeenT = Time.time;
    }

    Vector3 PredictTargetPosition()
    {
        Vector3 pred = target ? target.position + targetVel * predictionTime : lastSeenPos;
        return SampleOnNavMesh(pred);
    }

    void PushBreadcrumb(Vector3 p)
    {
        if (breadcrumbs.Count == 0 || (breadcrumbs.Peek() - p).sqrMagnitude > 0.3f * 0.3f)
        {
            breadcrumbs.Enqueue(p);
            while (breadcrumbs.Count > breadcrumbMax) breadcrumbs.Dequeue();
        }
    }

    void BuildSearchPlan()
    {
        searchPlan.Clear();
        Vector3 lost = SampleOnNavMesh(lastSeenPos);

        if (breadcrumbs.Count > 0)
        {
            var tmp = new List<Vector3>(breadcrumbs);
            for (int i = tmp.Count - 1; i >= 0; --i) TryAddSearchPoint(tmp[i]);
        }

        TryAddSearchPoint(lost);

        if (lastSeenDir.sqrMagnitude < 1e-4f) lastSeenDir = transform.forward;

        for (int i = 1; i <= wedgePoints; i++)
        {
            float d = wedgeStepDist * i;
            Vector3 dir0 = lastSeenDir;
            Vector3 dirL = Quaternion.Euler(0, -wedgeHalfAngle, 0) * lastSeenDir;
            Vector3 dirR = Quaternion.Euler(0, wedgeHalfAngle, 0) * lastSeenDir;
            TryAddSearchPoint(lost + dir0 * d);
            TryAddSearchPoint(lost + dirL * d * 0.9f);
            TryAddSearchPoint(lost + dirR * d * 0.9f);
        }

        float step = 360f / Mathf.Max(1, innerRingPoints);
        for (int i = 0; i < innerRingPoints; i++)
        {
            Vector3 dir = Quaternion.Euler(0, step * i, 0) * Vector3.forward;
            TryAddSearchPoint(lost + dir * innerRingRadius);
        }

        for (int k = 0; k < Mathf.Max(0, revisitLostCount); k++)
        {
            Vector2 rnd = Random.insideUnitCircle.normalized * (0.25f + 0.15f * k);
            TryAddSearchPoint(lost + new Vector3(rnd.x, 0, rnd.y));
            float ringR = innerRingRadius + revisitSpacing * (k + 1);
            for (int i = 0; i < 3; i++)
            {
                float ang = (120f * i + 37f * k);
                Vector3 dir = Quaternion.Euler(0, ang, 0) * Vector3.forward;
                TryAddSearchPoint(lost + dir * ringR);
            }
            TryAddSearchPoint(lost);
        }

        DedupPlan(0.5f);
    }

    void RunSearch()
    {
        agent.speed = walkSpeed;
        searchTimer += Time.deltaTime;

        if (CanEngage()) { SetState(State.Chase); return; }
        if (searchTimer >= searchMaxTime) { CalmDown(); return; }
        if (searchIdx >= searchPlan.Count) { CalmDown(); return; }

        Vector3 goal = searchPlan[searchIdx];
        bool atGoal = Arrived(goal, 0.35f);
        if (!agent.hasPath && !atGoal) SafeSetDestination(goal);

        if (atGoal)
        {
            agent.ResetPath();

            bool atLost = (goal - lastSeenPos).sqrMagnitude <= 0.5f * 0.5f;
            if (atLost && waitTimer < investigateTime)
            {
                waitTimer += Time.deltaTime;
                float yaw = Mathf.Sin(Time.time * investigateYawSpeed * Mathf.Deg2Rad) * wedgeHalfAngle;
                Quaternion look = Quaternion.LookRotation(Quaternion.Euler(0, yaw, 0) * (lastSeenDir.sqrMagnitude > 0 ? lastSeenDir : transform.forward));
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-turnLerp * Time.deltaTime));
                return;
            }

            waitTimer = 0f;
            searchIdx++;
            if (searchIdx < searchPlan.Count) SafeSetDestination(searchPlan[searchIdx]);
        }
    }

    void TryAddSearchPoint(Vector3 pos)
    {
        Vector3 snap = SampleOnNavMesh(pos);
        if ((snap - pos).sqrMagnitude <= 4f) searchPlan.Add(snap);
    }

    void DedupPlan(float minDist)
    {
        for (int i = 0; i < searchPlan.Count; ++i)
            for (int j = searchPlan.Count - 1; j > i; --j)
                if ((searchPlan[i] - searchPlan[j]).sqrMagnitude < minDist * minDist)
                    searchPlan.RemoveAt(j);
    }

    void CalmDown()
    {
        suspicion = 0f;
        breadcrumbs.Clear();
        lastSeenT = float.NegativeInfinity;
        if (patrolPoints != null && patrolPoints.Length > 0) SetState(State.Patrol);
        else SetState(State.Idle);
    }

    System.Collections.IEnumerator TraverseLinkBallistic()
    {
        traversing = true;

        var data = agent.currentOffMeshLinkData;
        Vector3 start = transform.position;
        Vector3 end = data.endPos;

        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;
        rb.isKinematic = false;

        float t = Mathf.Max(0.1f, jumpTime);
        Vector3 delta = end - start;
        Vector3 deltaXZ = new Vector3(delta.x, 0f, delta.z);
        float g = Mathf.Abs(Physics.gravity.y);

        Vector3 vXZ = deltaXZ / t;
        float vy = (delta.y + 0.5f * g * t * t) / t + extraUpVelocity;
        rb.velocity = vXZ + Vector3.up * vy;

        if (deltaXZ.sqrMagnitude > 1e-4f)
            transform.rotation = Quaternion.LookRotation(deltaXZ.normalized, Vector3.up);

        float elapsed = 0f;
        while (elapsed < t * 1.25f) { elapsed += Time.deltaTime; yield return null; }

        rb.isKinematic = true; rb.velocity = Vector3.zero;
        agent.CompleteOffMeshLink();
        agent.Warp(SampleOnNavMesh(transform.position));
        agent.updatePosition = true; agent.updateRotation = true; agent.isStopped = false;

        traversing = false;
    }

    void SetState(State s)
    {
        state = s;
        if (s == State.Idle || s == State.Patrol) { lastSeenT = float.NegativeInfinity; suspicion = 0f; }

        switch (s)
        {
            case State.Idle:
                agent.ResetPath(); agent.speed = 0f; break;
            case State.Patrol:
                agent.isStopped = false; agent.speed = walkSpeed; NextPatrol(); break;
            case State.Alert:
                agent.isStopped = false; break;
            case State.Chase:
                agent.isStopped = false; break;
            case State.Attack:
                agent.isStopped = true; FaceTo(target ? target.position : transform.position + transform.forward); break;
            case State.Search:
                agent.isStopped = false;
                BuildSearchPlan();
                searchIdx = 0; searchTimer = 0f; waitTimer = 0f;
                if (searchPlan.Count > 0) SafeSetDestination(searchPlan[0]);
                break;
        }
    }

    void NextPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) { SetState(State.Idle); return; }
        SafeSetDestination(patrolPoints[patrolIx].position);
        patrolIx = (patrolIx + 1) % patrolPoints.Length;
    }

    void FaceTo(Vector3 p)
    {
        Vector3 dir = p - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 1e-4f) return;
        Quaternion to = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, to, 1f - Mathf.Exp(-turnLerp * Time.deltaTime));
    }

    Vector3 SampleOnNavMesh(Vector3 pos)
    {
        return NavMesh.SamplePosition(pos, out var hit, reprojectRadius, NavMesh.AllAreas) ? hit.position : pos;
    }

    void SnapToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }

    bool SafeSetDestination(Vector3 dst)
    {
        if (!agent.enabled) return false;
        if (!agent.isOnNavMesh) { SnapToNavMesh(); if (!agent.isOnNavMesh) return false; }
        return agent.SetDestination(dst);
    }

    bool CanEngage() => target && (hasLOS || Remembering() || suspicion >= suspicionToChase * 0.8f);
    bool Remembering() => lastSeenT > 0f && (Time.time - lastSeenT) <= memoryTime;
    float DistToTarget() => target ? Vector3.Distance(transform.position, target.position) : Mathf.Infinity;
    bool Arrived(Vector3 p, float r) => (transform.position - p).sqrMagnitude <= r * r;

    void OnDrawGizmosSelected()
    {
        if (!eyes) eyes = transform;
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f); Gizmos.DrawWireSphere(transform.position, hearFar);
        Gizmos.color = new Color(1f, 0.65f, 0f, 0.25f); Gizmos.DrawWireSphere(transform.position, hearMid);
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.25f); Gizmos.DrawWireSphere(transform.position, hearNear);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, 0.1f);
        Vector3 dir = eyes ? eyes.forward : transform.forward;
        Vector3 L = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * dir;
        Vector3 R = Quaternion.Euler(0, viewAngle * 0.5f, 0) * dir;
        Gizmos.DrawLine(eyes.position, eyes.position + L * viewDistance);
        Gizmos.DrawLine(eyes.position, eyes.position + R * viewDistance);
        Gizmos.color = Color.gray;
        Vector3 Lp = Quaternion.Euler(0, -peripheralAngle * 0.5f, 0) * dir;
        Vector3 Rp = Quaternion.Euler(0, peripheralAngle * 0.5f, 0) * dir;
        Gizmos.DrawLine(eyes.position, eyes.position + Lp * peripheralDistance);
        Gizmos.DrawLine(eyes.position, eyes.position + Rp * peripheralDistance);
        Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(lastSeenPos, 0.25f);
    }
}

static class VecXZ
{
    public static Vector3 XZ(this Vector3 v) => new Vector3(v.x, 0f, v.z);
}
