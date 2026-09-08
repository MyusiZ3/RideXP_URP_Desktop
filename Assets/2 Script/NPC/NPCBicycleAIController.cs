// NPCBicycleAIController.cs (Final Fix Version + Personality)
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBPScripts;

/// <summary>
/// Controls AI pathfinding, obstacle avoidance, personality modifiers, and auto-respawn behavior for NPC cyclists.
/// </summary>
public class NPCBicycleAIController : MonoBehaviour
{
    public enum NPCPersonality { Normal, Aggressive, Lazy, Zigzag }

    [Header("Navigation")]
    public WaypointManager waypointManager;
    public float moveSpeed = 5f;
    public float turnSpeed = 5f;
    public float waypointThreshold = 2f;
    public float laneOffsetRange = 1.5f;

    [Header("Obstacle Avoidance")]
    public string obstacleTag = "Obstacle";
    public float obstacleCheckDistance = 5f;
    public float slowDownSpeed = 2f;

    [Header("Jump Settings AI")]
    public float jumpCooldown = 1f;
    public float aiJumpChargeTime = 0.2f;

    [Header("Stuck & Off-Track Detection")]
    [Tooltip("Duration in seconds before a stationary NPC triggers a respawn.")]
    public float stuckThreshold = 5f;
    public bool isUnderSpeedBoost = false;
    public float maxOffTrackTime = 4f;
    [Tooltip("Maximum distance from the active path segment before off-track timer begins.")]
    public float offTrackDistanceThreshold = 10f;

    [Header("Respawn Controls")]
    [Tooltip("Cooldown duration following a respawn to prevent rapid consecutive triggers.")]
    public float respawnSafetyCooldown = 2f;
    public float fallLimitY = -10f;
    public float flyLimitY = 100f;
    public float respawnHeightOffset = 1.5f;
    public float stuckAndBelowYRespawnLimit = 3f;

    [Header("NPC Personality")]
    public NPCPersonality personality = NPCPersonality.Normal;

    private int currentWaypointIndex = 0;
    private List<Vector3> waypoints;
    private Vector3 offset;
    private Animator animator;
    private Rigidbody rb;
    private bool isSlowingDown = false;
    private BicycleController bikeController;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float offTrackTimer = 0f;
    private float lastJumpTime = -999f;
    private float lastRespawnTime = -999f;

    private void Start()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("NPC"), true);

        if (waypointManager == null)
        {
            Debug.LogError($"[NPCBicycleAIController] WaypointManager not assigned on {gameObject.name}");
            enabled = false;
            return;
        }

        waypoints = waypointManager.waypoints;
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError($"[NPCBicycleAIController] WaypointManager contains no waypoints for {gameObject.name}");
            enabled = false;
            return;
        }

        offset = new Vector3(Random.Range(-laneOffsetRange, laneOffsetRange), 0f, Random.Range(-laneOffsetRange, laneOffsetRange) * 0.2f);
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        bikeController = GetComponent<BicycleController>();

        if (bikeController != null)
        {
            bikeController.isAIControlled = true;
        }
        else
        {
            Debug.LogWarning($"[NPCBicycleAIController] BicycleController component missing on {name}");
        }

        lastPosition = transform.position;

        switch (personality)
        {
            case NPCPersonality.Aggressive:
                moveSpeed += 2f;
                turnSpeed += 2f;
                break;
            case NPCPersonality.Lazy:
                moveSpeed *= 0.6f;
                slowDownSpeed *= 0.6f;
                break;
        }
    }

    private void Update()
    {
        if (Time.time < lastRespawnTime + respawnSafetyCooldown)
            return;

        if (waypoints == null || waypoints.Count == 0) return;

        if (transform.position.y < fallLimitY || transform.position.y > flyLimitY)
        {
            HandleRespawn("Out of Bounds");
            return;
        }

        Vector3 currentTargetWaypoint = waypoints[currentWaypointIndex];
        Vector3 previousWaypoint = waypoints[Mathf.Max(0, currentWaypointIndex - 1)];

        float distFromPath = HandleOffTrackDetection(previousWaypoint, currentTargetWaypoint, transform.position);

        if (distFromPath > offTrackDistanceThreshold)
        {
            offTrackTimer += Time.deltaTime;
            if (offTrackTimer >= maxOffTrackTime)
            {
                HandleRespawn("Off track duration exceeded");
                return;
            }
        }
        else
        {
            offTrackTimer = 0f;
        }

        Vector3 target = waypoints[currentWaypointIndex] + offset;
        Vector3 direction = target - transform.position;
        direction.y = 0;

        Vector3 flatDir = rb.linearVelocity;
        flatDir.y = 0;
        float angleDrift = Vector3.Angle(flatDir.normalized, transform.forward);

        if (angleDrift > 60f && rb.linearVelocity.magnitude > 3f)
        {
            HandleRespawn("Excessive drift off-track");
            return;
        }

        isSlowingDown = false;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hitInfo, obstacleCheckDistance))
        {
            if (hitInfo.collider.CompareTag(obstacleTag))
            {
                isSlowingDown = true;
                PerformObstacleAvoidance();
            }
        }

        if (direction.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        float currentSpeed = isSlowingDown ? slowDownSpeed : moveSpeed;

        if (bikeController != null)
        {
            if (currentSpeed > 0.1f)
                bikeController.pedalInput = 1f;
            else if (currentSpeed > 0.01f)
                bikeController.pedalInput = 0.3f;
            else
                bikeController.pedalInput = 0f;

            if (direction.magnitude > 0.1f)
            {
                Vector3 localTargetDirection = transform.InverseTransformDirection(direction.normalized);
                bikeController.steerInput = Mathf.Clamp(localTargetDirection.x * 2f, -1f, 1f);
            }
            else
            {
                bikeController.steerInput = 0f;
            }

            if (animator != null)
            {
                animator.SetFloat("Speed", currentSpeed);
                animator.SetBool("IsPedaling", bikeController.pedalInput > 0.1f);

                float currentSteer = bikeController.steerInput;
                animator.SetFloat("SteerDirection", currentSteer);
                animator.SetBool("IsTurning", Mathf.Abs(currentSteer) > 0.2f);

                bool isSprinting = (currentSpeed > moveSpeed * 0.9f && currentSpeed > slowDownSpeed + 1f && !isSlowingDown);
                if (personality == NPCPersonality.Aggressive && currentSpeed > (moveSpeed - 1f))
                {
                    isSprinting = true;
                }
                animator.SetBool("IsSprinting", isSprinting);
            }
        }

        if (personality == NPCPersonality.Zigzag)
        {
            float zigzagOffset = Mathf.Sin(Time.time * 3f) * 0.5f;
            transform.position += (transform.forward + transform.right * zigzagOffset) * currentSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
        }

        if (animator != null)
            animator.SetFloat("Speed", currentSpeed);

        float distanceToTarget = Vector3.Distance(new Vector3(transform.position.x, target.y, transform.position.z), target);
        if (distanceToTarget < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            offset = new Vector3(Random.Range(-laneOffsetRange, laneOffsetRange), 0f, Random.Range(-laneOffsetRange, laneOffsetRange) * 0.2f);
            lastPosition = transform.position;
            stuckTimer = 0f;
        }

        if (Vector3.Distance(transform.position, lastPosition) < 0.1f && currentSpeed > 0.1f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckThreshold)
            {
                stuckTimer = 0f;
                if (transform.position.y < stuckAndBelowYRespawnLimit)
                    HandleRespawn("Stuck (Low Elevation)");
                else
                    HandleRespawn("Stuck (Stationary)");
                return;
            }
        }
        else
        {
            stuckTimer = 0f;
            if (Vector3.Distance(transform.position, lastPosition) >= 1f)
                lastPosition = transform.position;
        }
    }

    /// <summary>
    /// Activates NPC AI control when a race begins.
    /// </summary>
    public void ActivateAIForRace()
    {
        enabled = true;
        if (rb != null)
            rb.isKinematic = false;

        lastPosition = transform.position;
        stuckTimer = 0f;
        isSlowingDown = false;

        if (bikeController != null && moveSpeed > 0.1f)
            bikeController.pedalInput = 1f;

        Debug.Log($"[NPCBicycleAIController] AI activated for race on {gameObject.name}");
    }

    private void AttemptSingleJumpSequence()
    {
        if (Time.time < lastJumpTime + jumpCooldown)
            return;

        if (TryGetComponent(out BicycleController bc))
        {
            StartCoroutine(ExecuteJumpSequence(bc));
            lastJumpTime = Time.time;
        }
    }

    private void AttemptDoubleJumpSequence()
    {
        if (Time.time < lastJumpTime + jumpCooldown)
            return;

        if (TryGetComponent(out BicycleController bc))
        {
            StartCoroutine(ExecuteDoubleJumpInternal(bc));
            lastJumpTime = Time.time;
        }
    }

    private IEnumerator ExecuteJumpSequence(BicycleController bc)
    {
        if (bc == null) yield break;

        bc.bunnyHopInputState = 1;
        yield return new WaitForSeconds(aiJumpChargeTime);
        bc.bunnyHopInputState = -1;
        yield return null;
        bc.bunnyHopInputState = 0;
    }

    private IEnumerator ExecuteDoubleJumpInternal(BicycleController bc)
    {
        yield return StartCoroutine(ExecuteJumpSequence(bc));
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(ExecuteJumpSequence(bc));
    }

    private void PerformObstacleAvoidance()
    {
        float choice = Random.value;
        if (choice < 0.33f)
        {
            transform.Rotate(0, Random.Range(-45f, 45f), 0);
        }
        else if (choice < 0.66f)
        {
            AttemptSingleJumpSequence();
        }
        else
        {
            AttemptDoubleJumpSequence();
        }
    }

    private void HandleRespawn(string reason)
    {
        if (Time.time < lastRespawnTime + respawnSafetyCooldown)
            return;

        offTrackTimer = 0f;
        Debug.Log($"[NPCBicycleAIController] Respawning {gameObject.name}. Reason: {reason}");

        if (waypoints == null || waypoints.Count == 0)
        {
            transform.position = Vector3.up * respawnHeightOffset;
            return;
        }

        Vector3 basePoint = waypoints[Mathf.Clamp(currentWaypointIndex, 0, waypoints.Count - 1)];
        Vector3 respawnPosition = basePoint + Vector3.up * respawnHeightOffset;

        if (Physics.Raycast(basePoint + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f))
        {
            Vector3 safePos = hit.point + Vector3.up * respawnHeightOffset;
            Collider[] overlaps = Physics.OverlapSphere(safePos, 0.5f);
            if (overlaps.Length == 0)
            {
                respawnPosition = safePos;
            }
            else
            {
                respawnPosition = basePoint + Vector3.up * 2f;
            }
        }

        Vector3 directionToNext = (waypoints[(currentWaypointIndex + 1) % waypoints.Count] - basePoint).normalized;
        directionToNext.y = 0;

        transform.position = respawnPosition;
        if (directionToNext != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(directionToNext);

        if (bikeController != null)
        {
            bikeController.ResetPhysicsState();
        }
        else if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Physics.SyncTransforms();

        stuckTimer = 0f;
        isSlowingDown = false;
        lastPosition = transform.position;
        lastRespawnTime = Time.time;
    }

    private float HandleOffTrackDetection(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lineDirection = lineEnd - lineStart;
        float lineLengthSquared = lineDirection.sqrMagnitude;

        if (lineLengthSquared == 0)
            return Vector3.Distance(point, lineStart);

        float t = Mathf.Clamp01(Vector3.Dot(point - lineStart, lineDirection) / lineLengthSquared);
        Vector3 projection = lineStart + t * lineDirection;
        return Vector3.Distance(point, projection);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPCRider") && other.gameObject == gameObject)
            return;

        if (other.CompareTag("ObstacleTrigger"))
        {
            AttemptSingleJumpSequence();
        }
        else if (other.CompareTag("DoubleJumpZone"))
        {
            AttemptDoubleJumpSequence();
        }
        else if (other.CompareTag("TurnTrigger"))
        {
            if (Time.time > lastJumpTime + jumpCooldown)
            {
                transform.Rotate(0, Random.Range(-45f, 45f), 0);
                lastJumpTime = Time.time;
            }
        }
        else if (other.CompareTag("SpeedBoost"))
        {
            StartCoroutine(SpeedBoostRoutine());
        }
        else if (other.CompareTag("BranchPoint"))
        {
            var branch = other.GetComponent<BranchSelector>();
            if (branch != null)
                Debug.Log($"[NPCBicycleAIController] {gameObject.name} encountered BranchPoint: {other.name}");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("NPCRider") && other.gameObject == gameObject)
            return;

        if (other.CompareTag("ObstacleTrigger"))
        {
            AttemptSingleJumpSequence();
        }
        else if (other.CompareTag("DoubleJumpZone"))
        {
            AttemptDoubleJumpSequence();
        }
        else if (other.CompareTag("TurnTrigger"))
        {
            if (Time.time > lastJumpTime + jumpCooldown)
            {
                transform.Rotate(0, Random.Range(-45f, 45f), 0);
                lastJumpTime = Time.time;
            }
        }
    }

    private IEnumerator SpeedBoostRoutine()
    {
        float originalSpeed = moveSpeed;
        float boostMultiplier = 1.5f;
        float maxBoostSpeed = 12f;

        moveSpeed *= boostMultiplier;
        isUnderSpeedBoost = true;

        if (TryGetComponent(out BicycleController bc))
        {
            bc.ApplySpeedBoost(boostMultiplier, 3f);
        }
        else if (animator != null)
        {
            animator.SetFloat("SpeedMultiplier", boostMultiplier);
        }

        float duration = 3f;
        while (duration > 0f)
        {
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxBoostSpeed);
            duration -= Time.deltaTime;
            yield return null;
        }

        moveSpeed = originalSpeed;
        isUnderSpeedBoost = false;
        if (animator != null) animator.SetFloat("SpeedMultiplier", 1f);
    }

    /// <summary>
    /// Updates the NPC's active path using a newly assigned spline.
    /// </summary>
    public void SetNewPathFromSpline(RamSpline newSpline)
    {
        if (newSpline == null || newSpline.points == null || newSpline.points.Count == 0)
        {
            Debug.LogWarning($"[NPCBicycleAIController] Invalid spline provided to {gameObject.name}");
            return;
        }

        List<Vector3> newPathPoints = new List<Vector3>(newSpline.points);
        int bestStartingIndex = 0;

        if (newPathPoints.Count > 0)
        {
            float minDistanceToPoint = float.MaxValue;
            Vector3 npcPosition = transform.position;
            Vector3 npcsForward = transform.forward;

            for (int i = 0; i < newPathPoints.Count; i++)
            {
                float distance = Vector3.Distance(npcPosition, newPathPoints[i]);
                if (distance < minDistanceToPoint)
                {
                    minDistanceToPoint = distance;
                    bestStartingIndex = i;
                }
            }

            if (newPathPoints.Count > 1)
            {
                Vector3 directionToClosestPoint = (newPathPoints[bestStartingIndex] - npcPosition).normalized;
                float dotProduct = Vector3.Dot(npcsForward, directionToClosestPoint);

                if (dotProduct < -0.3f && bestStartingIndex + 1 < newPathPoints.Count)
                {
                    Vector3 directionOfSegmentAfterClosest = (newPathPoints[bestStartingIndex + 1] - newPathPoints[bestStartingIndex]).normalized;
                    if (Vector3.Dot(npcsForward, directionOfSegmentAfterClosest) > 0f)
                    {
                        bestStartingIndex++;
                    }
                }
            }
        }

        waypoints = newPathPoints;
        currentWaypointIndex = bestStartingIndex;

        offset = new Vector3(Random.Range(-laneOffsetRange, laneOffsetRange), 0f, Random.Range(-laneOffsetRange, laneOffsetRange) * 0.2f);
        lastPosition = transform != null ? transform.position : (waypoints.Count > 0 ? waypoints[0] : Vector3.zero);

        stuckTimer = 0f;
        isSlowingDown = false;
    }
}