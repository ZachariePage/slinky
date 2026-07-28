using System;
using System.Collections.Generic;
using UnityEngine;


public class SlinkyManager : MonoBehaviour
{
    public enum SlinkyWrapMode
    {
        Straight,
        VerticalWrap,
        SideWrap
    }
    
    [Serializable]
    private class AirSlingshotState
    {
        [ReadOnly] public bool Active;
        [ReadOnly] public SlinkyWrapMode WrapMode;
        [ReadOnly] public SlinkyZone Zone;
        [ReadOnly] public Vector3 StartPosition;
        [ReadOnly] public Vector3 TargetPosition;
        [ReadOnly] public float ContinuousPullForce;
    }
    
    SlinkyWrapMode GetWrapMode(List<Vector3> points)
    {
        if (points == null || points.Count < 3)
            return SlinkyWrapMode.Straight;

        float horizontalBend = ComputeHorizontalBend(points);
        float horizontalDeviation = ComputeHorizontalDeviation(points);
        float verticalAmount = ComputeVerticalWrapAmount(points);

        bool hasSideWrap =
            horizontalBend > sideWrapBendThreshold ||
            horizontalDeviation > sideWrapDeviationThreshold;

        if (hasSideWrap)
            return SlinkyWrapMode.SideWrap;

        bool hasVerticalWrap = verticalAmount > verticalWrapHeightThreshold;
        if (hasVerticalWrap)
            return SlinkyWrapMode.VerticalWrap;

        return SlinkyWrapMode.Straight;
    }

    [Header("Players")]
        [SerializeField] private Rigidbody player1;
        [SerializeField] private Rigidbody player2;
        [SerializeField] private PlayerControllerData playerControllerData;

    [Header("Slinky Physics Settings")]
        [Tooltip("Number of physics segments. Higher = better wrapping around geometry.")]
        [SerializeField] private int physicsSegmentCount = 20;
        [SerializeField] private Transform slinkyParent;
        [SerializeField] private GameObject slinkySegmentPrefab;

    [Header("Chain Spring Settings")]
        [Tooltip("Attraction force between each segment and its neighbor. Higher = stiffer chain.")]
        [SerializeField] private float chainSpringStrength = 200f;
        [Tooltip("Damping applied to each segment velocity. Higher = less oscillation.")]
        [SerializeField] private float chainDamping = 12f;
        [Tooltip("Mass of each physics segment.")]
        [SerializeField] private float segmentMass = 0.2f;

    [Header("Distance Zones")]
        [Range(0, 100)] [SerializeField] private int softZoneSize = 60;
        [ReadOnly, SerializeField] private int mediumZoneSize = 15;
        [Range(0, 100)] [SerializeField] private int hardZoneSize = 25;
        
        [SerializeField] private float maxDistance = 25f;

        private float softLimitDistance;
        private float hardLimitDistance;
        
        public bool IsAtMaxDistance() => _chainLength >= maxDistance;
        
        private bool _wasAtMaxDistanceLastFrame = false;

    [Header("Retraction Forces | % of player max speed")]
        [SerializeField] private bool enableMediumZoneRetraction = true;
        [Range(0, 100)] [SerializeField] private int mediumRetractionForcePercent = 30;
        [ReadOnly, SerializeField] private float mediumRetractionForce = 0f;
    
    [Header("Retraction Forces | % of red zone when player can't move anymore")]
        [Range(0, 100)] [SerializeField] private int hardRetractionForcePercent = 40;
        [ReadOnly, SerializeField] private float minHardRetractionForce = 0f;
        [ReadOnly, SerializeField] private float maxHardRetractionForce = 0f;
        [ReadOnly, SerializeField] private float currentHardRetractionForce = 0f;
        [Range(0f, 5f)] [SerializeField] private float draggedPlayerRetractionMultiplier = 3f;
        
    [Header("Wrap Detection")]
        [ReadOnly, SerializeField] private SlinkyWrapMode debugCurrentWrappingMode = SlinkyWrapMode.Straight;
        private static SlinkyWrapMode _currentWrapMode = SlinkyWrapMode.Straight;
        public static SlinkyWrapMode CurrentWrappingMode => _currentWrapMode;
        
        [Tooltip("Total horizontal bend angle needed before the slinky is considered side-wrapped. Higher = more bend needed to consider it wrapped.")]
        [Range(0f, 180f)] [SerializeField] private float sideWrapBendThreshold = 35f;

        [Tooltip("Maximum horizontal distance the slinky can deviate from the direct player-to-player line before it counts as side-wrapped. Higher = more deviation allowed (forgives natural swing of the slinky)")]
        [Range(0f, 10f)] [SerializeField] private float sideWrapDeviationThreshold = 1.25f;

        [Tooltip("Maximum vertical offset from the direct player-to-player line before it counts as vertically wrapped. Higher = less strict about vertical wrapping (less vertical wrap detection)")]
        [Range(0f, 10f)] [SerializeField] private float verticalWrapHeightThreshold = 1.0f;

        [Tooltip("Ignores tiny local bend angles below this value to reduce chain jitter affecting wrap detection. Higher = more stable and less jittery")]
        [Range(0f, 45f)] [SerializeField] private float minLocalBendAngle = 5f;

    [Header("Handle Interaction")]
        [SerializeField] private float airborneHandleRetractionMultiplier = 2.5f;
        [SerializeField] private float forcedHandleSlingshotPullForce = 260f;
        [SerializeField] private float hardSlingshotAirControlMultiplier = 0.1f;
        [SerializeField] private float forcedHandleSlingshotAirControlMultiplier = 0.05f;

        bool IsPlayerGrounded(SlinAndKyControllerBase controller)
        {
            return controller != null && controller.GetIsGrounded();
        }
    
    [Header("Slingshot | Wrapped")]
        [Range(0f, 800f)] [SerializeField] private float hardWrappedSlingshotPullForce = 220f;

    [Header("Slingshot | Straight")]
        [SerializeField] [Range(0.5f, 1.5f)] private float hardStraightPastOtherDistanceMultiplier = 0.8f;
        [Range(0f, 45f)] [SerializeField] private float hardStraightAngleBonus = 12f;
        [Range(0f, 10f)] [SerializeField] private float hardStraightArcHeight = 2.5f;
        [SerializeField] [Range(0f, 45f)] private float slingshotAirTiltAngle = 10f;
        [SerializeField] [Range(5f, 60f)] private float hardStraightLaunchAngle = 28f;
        [SerializeField] [Range(0.1f, 2f)] private float hardStraightForceMultiplier = 0.85f;
        [SerializeField] [Range(0f, 10f)] private float straightAnglePerHeightUnit = 2.5f;
        [SerializeField] [Range(1f, 45f)] private float minStraightLaunchAngle = 8f;
        [SerializeField] [Range(30f, 75f)] private float maxStraightLaunchAngle = 75f;
    
    [Header("Visual")]
        [SerializeField] private GameObject visualRingPrefab;
        [SerializeField] private int visualRingCount = 20;
        [SerializeField] private float visualRingScaleXY = 0.3f;
        [SerializeField] private float visualRingScaleZ = 1f;
        [SerializeField] private Transform visualParent;

    [Header("Visual | Colors")]
        [SerializeField] private Color softZoneColor;
        [SerializeField] private Color mediumZoneColor;
        [SerializeField] private Color hardZoneColor;
    
    private List<Rigidbody> _segmentBodies = new();
    private List<Transform> _visualRings = new();

    private SlinAndKyControllerBase _p1Controller;
    private SlinAndKyControllerBase _p2Controller;
    Rigidbody GetOtherPlayer(SlinAndKyControllerBase.PlayerNumber playerNum)
    {
        return playerNum == SlinAndKyControllerBase.PlayerNumber.Player1 ? player2 : player1;
    }

    public enum SlinkyZone { Soft, Medium, Hard }
    private static SlinkyZone _currentZone = SlinkyZone.Soft;
    public static SlinkyZone CurrentZone => _currentZone;

    // Cached each FixedUpdate for use in force methods
    private float _chainLength;
   
    private bool _chainWentBelowMax = false;
    private Vector3 _p1SlinkyDir;
    private Vector3 _p2SlinkyDir;

    Vector3 GetPlayerSlinkyDir(SlinAndKyControllerBase.PlayerNumber playerNum)
    {
        return playerNum == SlinAndKyControllerBase.PlayerNumber.Player1 ? _p1SlinkyDir : _p2SlinkyDir;
    }
    
    Vector3 GetPlayerSlinkyDir(SlinAndKyControllerBase controller)
    {
        return controller == _p1Controller ? _p1SlinkyDir : _p2SlinkyDir;
    }

    [Header("Debug")]
        [ReadOnly, SerializeField] private SlinkyZone debugCurrentZone = SlinkyZone.Soft;
        [ReadOnly, SerializeField] private float debugChainLength = 0f;
        
        [SerializeField] private AirSlingshotState _p1AirSlingshot = new();
        [SerializeField] private AirSlingshotState _p2AirSlingshot = new();
        AirSlingshotState GetAirSlingshotState(SlinAndKyControllerBase.PlayerNumber playerNum)
        {
            return playerNum == SlinAndKyControllerBase.PlayerNumber.Player1 ? _p1AirSlingshot : _p2AirSlingshot;
        }


    private void OnValidate()
    {
        // Adjust value to be maximum 100%
        if(hardZoneSize + softZoneSize > 100)
            hardZoneSize = 100 - softZoneSize;
        
        if(softZoneSize + hardZoneSize > 100)
            softZoneSize = 100 - hardZoneSize;
        
        RecalculateZoneDistances();
        
    }


    void ResetPlayers()
    {
        player1 = null;
        player2 = null;

        foreach (Transform child in _visualRings)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (Rigidbody child in _segmentBodies)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
        if (!player1 || !player2)
        {
            GameObject[] plrs = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject plr in plrs)
            {
                if (plr.transform.name == "P1")
                    player1 = plr.GetComponent<Rigidbody>();

                if (plr.transform.name == "P2")
                    player2 = plr.GetComponent<Rigidbody>();
            }
        }
        _p1Controller = player1.GetComponent<SlinAndKyControllerBase>();
        _p2Controller = player2.GetComponent<SlinAndKyControllerBase>();

        _p1Controller.OnPlayerJump    += OnPlayerJumpPerformed;
        _p2Controller.OnPlayerJump    += OnPlayerJumpPerformed;
        _p1Controller.OnPlayerLanding += OnPlayerLanding;
        _p2Controller.OnPlayerLanding += OnPlayerLanding;
        
        _p1AirSlingshot.Active = false;
        _p1AirSlingshot.ContinuousPullForce = 0f;
        _p2AirSlingshot.Active = false;
        _p2AirSlingshot.ContinuousPullForce = 0f;
        
        SpawnSegments();
        SpawnVisualRings();
    }
    void Start()
    {
        RecalculateZoneDistances();
        if (!player1 || !player2)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.transform.name == "P1")
                    player1 = player.GetComponent<Rigidbody>();

                if (player.transform.name == "P2")
                    player2 = player.GetComponent<Rigidbody>();
            }
        }
        _p1Controller = player1.GetComponent<SlinAndKyControllerBase>();
        _p2Controller = player2.GetComponent<SlinAndKyControllerBase>();
        
        // Jump Events
        _p1Controller.OnPlayerJump += OnPlayerJumpPerformed;
        _p2Controller.OnPlayerJump += OnPlayerJumpPerformed;
        
        // Landing Events
        _p1Controller.OnPlayerLanding += OnPlayerLanding;
        _p2Controller.OnPlayerLanding += OnPlayerLanding;

        SpawnSegments();
        SpawnVisualRings();

        WorldManager.Instance.OnPlayerSpawn += ResetPlayers;
    }

    private void RecalculateZoneDistances()
    {
        mediumZoneSize = 100 - softZoneSize - hardZoneSize;
    
        softLimitDistance = maxDistance * (softZoneSize / 100f);
        hardLimitDistance = maxDistance * ((softZoneSize + mediumZoneSize) / 100f);

        if (playerControllerData)
        {
            float maxSpeed = playerControllerData.moveSpeed;
            mediumRetractionForce = maxSpeed * (mediumRetractionForcePercent / 100f);
            minHardRetractionForce = mediumRetractionForce;

            float hardZoneSpan = Mathf.Max(0.0001f, maxDistance - hardLimitDistance);
            float percent01 = Mathf.Clamp01(hardRetractionForcePercent / 100f);
            float targetDist = hardLimitDistance + hardZoneSpan * percent01;
            float riseDist = Mathf.Max(0.0001f, targetDist - hardLimitDistance);
            float slope = (maxSpeed - minHardRetractionForce) / riseDist;
            maxHardRetractionForce = minHardRetractionForce + slope * hardZoneSpan;
            currentHardRetractionForce = 0f;
        }
    }

    void FixedUpdate()
    {
        if (!player1 || !player2)
            return;

        List<Vector3> chainPoints = BuildChainPoints();

        _chainLength = ComputeChainLength(chainPoints);
        _p1SlinkyDir = ComputeP1SlinkyDir(chainPoints);
        _p2SlinkyDir = ComputeP2SlinkyDir(chainPoints);

        _currentWrapMode = GetWrapMode(chainPoints);

        if (_chainLength > hardLimitDistance)
            _currentZone = SlinkyZone.Hard;
        else if (_chainLength > softLimitDistance)
            _currentZone = SlinkyZone.Medium;
        else
            _currentZone = SlinkyZone.Soft;

        UpdateCurrentHardRetractionForce();
        UpdateMaxDistanceSlingshot();
        UpdateRetractionForces();
        UpdateForcedHandleSlingshot();
        UpdateAirSlingshots();
        UpdateChain();
        
        if (_chainLength >= maxDistance)
        {
            
            _p1Controller.ClampVelocityAtDistance(-_p1SlinkyDir);
            _p2Controller.ClampVelocityAtDistance(-_p2SlinkyDir);
        }
        
    }

    void Update()
    {
        if (!player1 || !player2)
        {
            return;
        }
        UpdateVisualRings();

        _p1Controller.SetSlinkyPlayerDirection(_p1SlinkyDir);
        _p2Controller.SetSlinkyPlayerDirection(_p2SlinkyDir);

        debugCurrentWrappingMode = _currentWrapMode;
        debugCurrentZone = _currentZone;
        debugChainLength = _chainLength;
    }

    // ─── Chain Measurement ────────────────────────────────────────────────────

    /// <summary>
    /// Builds the ordered list of positions: [p1, seg0, seg1, ..., segN, p2]
    /// </summary>
    List<Vector3> BuildChainPoints()
    {
        if (!player1 || !player2)
        {
            return null;
        }
        List<Vector3> points = new(_segmentBodies.Count + 2) { player1.position };
        foreach (Rigidbody seg in _segmentBodies)
            points.Add(seg.position);
        points.Add(player2.position);
        return points;
    }

    /// <summary>
    /// Sums the distance between every consecutive pair of chain points.
    /// This is the real "slinky length" regardless of wrapping around objects.
    /// </summary>
    float ComputeChainLength(List<Vector3> points)
    {
        float length = 0f;
        for (int i = 0; i < points.Count - 1; i++)
            length += Vector3.Distance(points[i], points[i + 1]);
        return length;
    }

    /// <summary>
    /// Direction from player1 along the chain: from p1 towards the first segment (or p2 if no segments).
    /// This is where the slinky "pulls" player1 from, accounting for wrapping.
    /// </summary>
    Vector3 ComputeP1SlinkyDir(List<Vector3> points)
    {
        if (points.Count < 2) return Vector3.zero;
        Vector3 dir = (points[1] - points[0]).normalized; // p1 -> first segment
        return dir == Vector3.zero ? Vector3.zero : dir;
    }

    /// <summary>
    /// Direction from player2 along the chain: from p2 towards the last segment (or p1 if no segments).
    /// </summary>
    Vector3 ComputeP2SlinkyDir(List<Vector3> points)
    {
        if (points.Count < 2) return Vector3.zero;
        int last = points.Count - 1;
        Vector3 dir = (points[last - 1] - points[last]).normalized; // p2 -> last segment
        return dir == Vector3.zero ? Vector3.zero : dir;
    }

    // ─── Slinky Bend Computing ─────────────────────────────────────────────────────────────
    float ComputeHorizontalBend(List<Vector3> points)
    {
        float totalAngle = 0f;

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector3 prev = points[i] - points[i - 1];
            Vector3 next = points[i + 1] - points[i];

            prev.y = 0f;
            next.y = 0f;

            if (prev.sqrMagnitude < 0.0001f || next.sqrMagnitude < 0.0001f)
                continue;

            prev.Normalize();
            next.Normalize();

            float angle = Vector3.Angle(prev, next);
            if (angle >= minLocalBendAngle)
                totalAngle += angle;
        }

        return totalAngle;
    }
    
    float ComputeHorizontalDeviation(List<Vector3> points)
    {
        if (points == null || points.Count < 2)
            return 0f;

        Vector3 start = points[0];
        Vector3 end = points[^1];

        start.y = 0f;
        end.y = 0f;

        Vector3 line = end - start;
        float lineLength = line.magnitude;

        if (lineLength < 0.0001f)
            return 0f;

        Vector3 lineDir = line / lineLength;

        float maxDeviation = 0f;

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector3 p = points[i];
            p.y = 0f;

            Vector3 fromStart = p - start;
            float projection = Vector3.Dot(fromStart, lineDir);
            Vector3 closestPoint = start + lineDir * projection;

            float deviation = Vector3.Distance(p, closestPoint);
            if (deviation > maxDeviation)
                maxDeviation = deviation;
        }

        return maxDeviation;
    }
    
    float ComputeVerticalWrapAmount(List<Vector3> points)
    {
        if (points == null || points.Count < 2)
            return 0f;

        float maxHeightOffset = 0f;
        float startY = points[0].y;
        float endY = points[^1].y;

        for (int i = 1; i < points.Count - 1; i++)
        {
            float t = i / (float)(points.Count - 1);
            float expectedY = Mathf.Lerp(startY, endY, t);
            float offset = Mathf.Abs(points[i].y - expectedY);

            if (offset > maxHeightOffset)
                maxHeightOffset = offset;
        }

        return maxHeightOffset;
    }

    // ─── Spawning ─────────────────────────────────────────────────────────────

    void SpawnSegments()
    {
        _segmentBodies.Clear();

        for (int i = 0; i < physicsSegmentCount; i++)
        {
            float t = (i + 1f) / (physicsSegmentCount + 1f);
            Vector3 startPos = Vector3.Lerp(player1.position, player2.position, t);

            GameObject go = Instantiate(slinkySegmentPrefab, startPos, Quaternion.identity, slinkyParent);
            go.name = $"SlinkySegment_{i}";

            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass        = segmentMass;
                rb.useGravity  = true;
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                _segmentBodies.Add(rb);
            }
        }
    }

    void SpawnVisualRings()
    {
        if (visualRingPrefab == null) return;
        _visualRings.Clear();

        for (int i = 0; i < visualRingCount; i++)
        {
            GameObject ring = Instantiate(visualRingPrefab, Vector3.zero, Quaternion.identity, visualParent);
            if (ring.TryGetComponent(out Rigidbody rb))  rb.isKinematic = true;
            if (ring.TryGetComponent(out Collider col))  col.enabled    = false;
            _visualRings.Add(ring.transform);
        }
    }


    // ─── Chain Physics ────────────────────────────────────────────────────────

    void UpdateChain()
    {
        for (int i = 0; i < _segmentBodies.Count; i++)
        {
            Rigidbody rb = _segmentBodies[i];

            Vector3 prevPos = (i == 0)
                ? player1.position
                : _segmentBodies[i - 1].position;

            Vector3 nextPos = (i == _segmentBodies.Count - 1)
                ? player2.position
                : _segmentBodies[i + 1].position;

            Vector3 forceToPrev  = (prevPos - rb.position) * chainSpringStrength;
            Vector3 forceToNext  = (nextPos - rb.position) * chainSpringStrength;
            Vector3 dampingForce = -rb.linearVelocity * chainDamping;

            rb.AddForce(forceToPrev + forceToNext + dampingForce, ForceMode.Force);
        }
    }


    // ─── Players events ───────────────────────────────────────────────

    void OnPlayerJumpPerformed(SlinAndKyControllerBase.PlayerNumber playerNum)
    {
        SlinAndKyControllerBase currentPlayer = playerNum switch
        {
            SlinAndKyControllerBase.PlayerNumber.Player1 => _p1Controller,
            SlinAndKyControllerBase.PlayerNumber.Player2 => _p2Controller,
            _ => null
        };

        if (currentPlayer == null || _currentZone != SlinkyZone.Hard)
            return;

        if (currentPlayer.IsOnHandle())
            return;

        AirSlingshotState state = GetAirSlingshotState(playerNum);
        state.Active = true;
        state.WrapMode = _currentWrapMode;
        state.Zone = _currentZone;

        Rigidbody currentRb = playerNum == SlinAndKyControllerBase.PlayerNumber.Player1 ? player1 : player2;
        state.StartPosition = currentRb != null ? currentRb.position : Vector3.zero;
        state.TargetPosition = Vector3.zero;
        state.ContinuousPullForce = 0f;

        float airControlMultiplier = GetSlingshotAirControlMultiplier(_currentZone);
        
        currentPlayer.StartSlingshot(airControlMultiplier);
        
       
        
        switch (_currentWrapMode)
        {
            case SlinkyWrapMode.SideWrap:
                StartWrappedSlingshot(playerNum, currentPlayer, state);
                break;

            case SlinkyWrapMode.Straight:
            case SlinkyWrapMode.VerticalWrap:
                _chainWentBelowMax = false;
                StartStraightSlingshot(playerNum, currentPlayer, state);
                
                break;
        }
    }
    
    void OnPlayerLanding(SlinAndKyControllerBase.PlayerNumber playerNum)
    {
        AirSlingshotState state = GetAirSlingshotState(playerNum);
        state.Active = false;
        state.ContinuousPullForce = 0f;
        state.TargetPosition = Vector3.zero;

        SlinAndKyControllerBase controller = playerNum == SlinAndKyControllerBase.PlayerNumber.Player1
            ? _p1Controller
            : _p2Controller;

        controller?.EndSlingshot();
    }
    
    // ─── Zone Retraction Forces on Players ───────────────────────────────────────────────
    void UpdateCurrentHardRetractionForce()
    {
        if (_currentZone != SlinkyZone.Hard)
        {
            currentHardRetractionForce = 0f;
            return;
        }

        float hard01 = Mathf.InverseLerp(hardLimitDistance, maxDistance, _chainLength);
        currentHardRetractionForce = Mathf.Lerp(minHardRetractionForce, maxHardRetractionForce, hard01);
    }
    
    void UpdateRetractionForces()
    {
        bool p1Ignored = ShouldIgnoreRetraction(_p1Controller);
        bool p2Ignored = ShouldIgnoreRetraction(_p2Controller);

        bool p1Airborne = !IsPlayerGrounded(_p1Controller);
        bool p2Airborne = !IsPlayerGrounded(_p2Controller);

        bool p1OnHandle = IsPlayerOnHandle(_p1Controller);
        bool p2OnHandle = IsPlayerOnHandle(_p2Controller);

        switch (_currentZone)
        {
            case SlinkyZone.Medium:
            {
                if (!enableMediumZoneRetraction)
                    break;

                float p1Force = mediumRetractionForce;
                float p2Force = mediumRetractionForce;

                if (p2OnHandle && p1Airborne && !p1Ignored)
                    p1Force *= airborneHandleRetractionMultiplier;

                if (p1OnHandle && p2Airborne && !p2Ignored)
                    p2Force *= airborneHandleRetractionMultiplier;

                if (!p1Ignored && (p1Airborne || !IsPlayerTryingToMove(_p1Controller)))
                    ApplyMediumZoneRetraction(_p1Controller, p1Force);

                if (!p2Ignored && (p2Airborne || !IsPlayerTryingToMove(_p2Controller)))
                    ApplyMediumZoneRetraction(_p2Controller, p2Force);

                break;
            }

            case SlinkyZone.Hard:
            {
                float p1Force = currentHardRetractionForce;
                float p2Force = currentHardRetractionForce;
                
                bool p1DisableRetraction = p2OnHandle && p1Airborne;
                bool p2DisableRetraction = p1OnHandle && p2Airborne;

                if (p2OnHandle && p1Airborne && !p1Ignored)
                    p1Force *= airborneHandleRetractionMultiplier;

                if (p1OnHandle && p2Airborne && !p2Ignored)
                    p2Force *= airborneHandleRetractionMultiplier;

                if (!p1OnHandle && !p2OnHandle)
                {
                    if (IsPlayerTryingToMove(_p1Controller) && !IsPlayerTryingToMove(_p2Controller) && IsPlayerGrounded(_p2Controller) && !p2Ignored)
                        p2Force *= draggedPlayerRetractionMultiplier;
                    else if (IsPlayerTryingToMove(_p2Controller) && !IsPlayerTryingToMove(_p1Controller) && IsPlayerGrounded(_p1Controller) && !p1Ignored)
                        p1Force *= draggedPlayerRetractionMultiplier;
                }

                if (!p1Ignored && !p1DisableRetraction)
                    ApplyHardZoneRetraction(_p1Controller, p1Force);

                if (!p2Ignored && !p2DisableRetraction)
                    ApplyHardZoneRetraction(_p2Controller, p2Force);

                break;
            }
        }
    }
    
    bool IsPlayerTryingToMove(SlinAndKyControllerBase controller)
    {
        if (controller == null)
            return false;

        if (controller.IsOnHandle() || controller.IsSlingshotting())
            return false;

        return controller.GetPlayerInput().sqrMagnitude > 0.01f;
    }
    
    bool IsPlayerSlingshotting(SlinAndKyControllerBase controller)
    {
        return controller != null && controller.IsSlingshotting();
    }

    bool IsPlayerOnHandle(SlinAndKyControllerBase controller)
    {
        return controller != null && controller.IsOnHandle();
    }

    bool ShouldIgnoreRetraction(SlinAndKyControllerBase controller)
    {
        return controller == null || controller.IsOnHandle() || controller.IsSlingshotting();
    }

    float GetSlingshotAirControlMultiplier(SlinkyZone zone)
    {
        return zone switch
        {
            SlinkyZone.Hard => hardSlingshotAirControlMultiplier,
            _ => 1f
        };
    }
    
    void ApplyMediumZoneRetraction(SlinAndKyControllerBase controller, float force)
    {
        if (controller == null || force <= 0f) return;
        controller.ApplyForceVelocity(force, GetPlayerSlinkyDir(controller));
    }
    
    void ApplyHardZoneRetraction(SlinAndKyControllerBase controller, float force)
    {
        if (controller == null || force <= 0f) return;
        controller.ApplyForceVelocity(force, GetPlayerSlinkyDir(controller));
    }
    
    // ─── Zone Slingshot Forces on Players ───────────────────────────────────────────────
    void UpdateAirSlingshots()
    {
        UpdateAirSlingshotForPlayer(SlinAndKyControllerBase.PlayerNumber.Player1, _p1Controller);
        UpdateAirSlingshotForPlayer(SlinAndKyControllerBase.PlayerNumber.Player2, _p2Controller);
    }
    
    void UpdateAirSlingshotForPlayer(SlinAndKyControllerBase.PlayerNumber playerNum, SlinAndKyControllerBase controller)
    {
        if (controller == null) return;

        AirSlingshotState state = GetAirSlingshotState(playerNum);
        if (!state.Active) return;

        switch (state.WrapMode)
        {
            case SlinkyWrapMode.SideWrap:
                UpdateWrappedSlingshot(playerNum, controller, state);
                break;

            case SlinkyWrapMode.Straight:
            case SlinkyWrapMode.VerticalWrap:
                UpdateStraightSlingshot(playerNum, controller, state);
                break;
        }
    }
    
    void UpdateMaxDistanceSlingshot()
    {
        bool isAtMax = IsAtMaxDistance();

        if (isAtMax && !_wasAtMaxDistanceLastFrame)
        {
            TryMaxDistanceSlingshot(
                SlinAndKyControllerBase.PlayerNumber.Player1,
                _p1Controller,
                _p2Controller);

            TryMaxDistanceSlingshot(
                SlinAndKyControllerBase.PlayerNumber.Player2,
                _p2Controller,
                _p1Controller);
        }

        _wasAtMaxDistanceLastFrame = isAtMax;
    }
    
    void TryMaxDistanceSlingshot(
        SlinAndKyControllerBase.PlayerNumber playerNum,
        SlinAndKyControllerBase controller,
        SlinAndKyControllerBase otherController)
    {
        if (controller == null || otherController == null)
            return;

        Rigidbody currentRb = playerNum == SlinAndKyControllerBase.PlayerNumber.Player1 ? player1 : player2;
        Rigidbody otherRb = GetOtherPlayer(playerNum);

        if (currentRb == null || otherRb == null)
            return;

        bool otherOnHandle = otherController.IsOnHandle();

        if (!otherOnHandle)
        {
            if (!controller.GetIsGrounded())
                return;
        }

        Vector3 dir;

        if (otherOnHandle)
        {
            dir = (otherRb.position - currentRb.position).normalized;
        }
        else
        {
            dir = (otherRb.position - currentRb.position).normalized;
        }

        float force = otherOnHandle
            ? forcedHandleSlingshotPullForce
            : forcedHandleSlingshotPullForce * 0.5f;

        controller.ApplySlingShotVelocity(force, dir);
    }
    
    void UpdateForcedHandleSlingshot()
    {
        TryForceHandleSlingshot(
            SlinAndKyControllerBase.PlayerNumber.Player1,
            _p1Controller,
            _p2Controller);

        TryForceHandleSlingshot(
            SlinAndKyControllerBase.PlayerNumber.Player2,
            _p2Controller,
            _p1Controller);

        EndForcedHandleSlingshotIfReturnedToSoft(
            SlinAndKyControllerBase.PlayerNumber.Player1,
            _p1Controller);

        EndForcedHandleSlingshotIfReturnedToSoft(
            SlinAndKyControllerBase.PlayerNumber.Player2,
            _p2Controller);
    }
    
    void TryForceHandleSlingshot(
        SlinAndKyControllerBase.PlayerNumber playerNum,
        SlinAndKyControllerBase controller,
        SlinAndKyControllerBase otherController)
    {
        if (controller == null || otherController == null)
            return;

        if (controller.IsSlingshotting())
            return;

        if (controller.GetIsGrounded())
            return;

        if (!otherController.IsOnHandle())
            return;

        if (_chainLength < maxDistance)
            return;

        AirSlingshotState state = GetAirSlingshotState(playerNum);
        state.Active = true;
        state.WrapMode = SlinkyWrapMode.SideWrap;
        state.Zone = SlinkyZone.Hard;
        state.StartPosition = playerNum == SlinAndKyControllerBase.PlayerNumber.Player1 ? player1.position : player2.position;
        state.TargetPosition = Vector3.zero;
        state.ContinuousPullForce = forcedHandleSlingshotPullForce;

        controller.StartSlingshot(forcedHandleSlingshotAirControlMultiplier);
    }
    
    void EndForcedHandleSlingshotIfReturnedToSoft(
        SlinAndKyControllerBase.PlayerNumber playerNum,
        SlinAndKyControllerBase controller)
    {
        if (controller == null || !controller.IsSlingshotting())
            return;

        if (_currentZone != SlinkyZone.Soft)
            return;

        AirSlingshotState state = GetAirSlingshotState(playerNum);
        if (!state.Active)
            return;

        state.Active = false;
        state.ContinuousPullForce = 0f;
        state.TargetPosition = Vector3.zero;
    }
    
    void UpdateWrappedSlingshot(SlinAndKyControllerBase.PlayerNumber playerNum, SlinAndKyControllerBase controller, AirSlingshotState state)
    {
        if (controller == null || state.ContinuousPullForce <= 0f)
            return;

        Vector3 dir = GetPlayerSlinkyDir(playerNum);
        Vector3 horizontalDir = new Vector3(dir.x, 0f, dir.z);

        if (horizontalDir.sqrMagnitude < 0.0001f)
        {
            Rigidbody currentRb = playerNum == SlinAndKyControllerBase.PlayerNumber.Player1 ? player1 : player2;
            Rigidbody otherRb = GetOtherPlayer(playerNum);

            if (currentRb == null || otherRb == null)
                return;

            Vector3 fallback = otherRb.position - currentRb.position;
            horizontalDir = new Vector3(fallback.x, 0f, fallback.z);

            if (horizontalDir.sqrMagnitude < 0.0001f)
                return;
        }

        horizontalDir.Normalize();
        controller.ApplyForceVelocity(state.ContinuousPullForce, horizontalDir);
    }
    
    void UpdateStraightSlingshot(SlinAndKyControllerBase.PlayerNumber playerNum, SlinAndKyControllerBase controller, AirSlingshotState state)
    {
        // If I need to update stuff on the slinky after the launch
    }
    
    void StartWrappedSlingshot(SlinAndKyControllerBase.PlayerNumber playerNum, SlinAndKyControllerBase controller, AirSlingshotState state)
    {
        state.ContinuousPullForce = state.Zone switch
        {
            SlinkyZone.Hard => hardWrappedSlingshotPullForce,
            _ => 0f
        };
    }
    
    void StartStraightSlingshot(SlinAndKyControllerBase.PlayerNumber playerNum, SlinAndKyControllerBase controller, AirSlingshotState state)
    {
        Rigidbody currentRb = playerNum == SlinAndKyControllerBase.PlayerNumber.Player1 ? player1 : player2;
        if (currentRb == null || controller == null)
            return;

        Vector3 target = GetHardStraightSlingshotTarget(playerNum);
        state.TargetPosition = target;

        Vector3 toTarget = target - currentRb.position;
        float horizontalDistance = new Vector2(toTarget.x, toTarget.z).magnitude;

        if (horizontalDistance <= 0.001f)
            return;

        float launchAngle = GetHeightAdjustedStraightLaunchAngle(
            currentRb.position,
            target,
            hardStraightLaunchAngle
        );

        float launchForce = RecalculateSlingshotForce(
            horizontalDistance,
            Mathf.Abs(controller.GetPlayerGravity()),
            launchAngle
        );

        launchForce *= hardStraightForceMultiplier;

        Vector3 launchDir = BuildStraightLaunchDirection(toTarget, launchAngle);
        controller.ApplySlingShotVelocity(launchForce, launchDir);
    }
    
    float GetHeightAdjustedStraightLaunchAngle(Vector3 startPos, Vector3 targetPos, float baseAngle)
    {
        float heightDelta = targetPos.y - startPos.y;
        float adjustedAngle = baseAngle + (heightDelta * straightAnglePerHeightUnit);

        return Mathf.Clamp(adjustedAngle, minStraightLaunchAngle, maxStraightLaunchAngle);
    }
    
    Vector3 BuildStraightLaunchDirection(Vector3 toTarget, float launchAngle)
    {
        Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
        if (flat.sqrMagnitude < 0.0001f)
            return Vector3.up;

        Vector3 flatDir = flat.normalized;
        float upwardBias = Mathf.Tan(launchAngle * Mathf.Deg2Rad);

        return (flatDir + Vector3.up * upwardBias).normalized;
    }

    Vector3 GetHardStraightSlingshotTarget(SlinAndKyControllerBase.PlayerNumber playerNum)
    {
        Rigidbody currentRb = playerNum == SlinAndKyControllerBase.PlayerNumber.Player1 ? player1 : player2;
        Rigidbody otherRb = GetOtherPlayer(playerNum);

        if (currentRb == null || otherRb == null)
            return Vector3.zero;

        Vector3 toOther = otherRb.position - currentRb.position;
        float dist = toOther.magnitude;

        if (dist <= 0.001f)
            return otherRb.position;

        Vector3 dir = toOther.normalized;

        Vector3 target = otherRb.position + dir * (dist * hardStraightPastOtherDistanceMultiplier);
        target.y += hardStraightArcHeight;

        return target;
    }
    Vector3 GetClampDirection(SlinAndKyControllerBase.PlayerNumber playerNum)
    {
        if (_currentWrapMode == SlinkyWrapMode.SideWrap)
        {
            return -GetPlayerSlinkyDir(playerNum);
        }
    
        Rigidbody currentRb = playerNum == SlinAndKyControllerBase.PlayerNumber.Player1 ? player1 : player2;
        Rigidbody otherRb = GetOtherPlayer(playerNum);
        return (currentRb.position - otherRb.position).normalized;
    }

    private float RecalculateSlingshotForce(float distanceWanted, float jumpGravity, float shotAngle)
    {
        float sinValue = Mathf.Sin(2f * shotAngle * Mathf.Deg2Rad);
        if (distanceWanted <= 0f || jumpGravity <= 0f || Mathf.Abs(sinValue) < 0.0001f)
            return 0f;

        return Mathf.Sqrt((distanceWanted * jumpGravity) / sinValue);
    }

    // ─── Visual Rings ─────────────────────────────────────────────────────────

    void UpdateVisualRings()
    {
        if (_visualRings.Count == 0) return;

        List<Vector3> points = BuildChainPoints();

        // Compute zone blend factors
        float softBlend   = 0f;
        float hardBlend   = 0f;

        if (_chainLength <= softLimitDistance)
        {
            softBlend = 1f;
        }
        else if (_chainLength <= hardLimitDistance)
        {
            softBlend = 1f - Mathf.InverseLerp(softLimitDistance, hardLimitDistance, _chainLength);
        }

        if (_chainLength > hardLimitDistance)
        {
            hardBlend = Mathf.InverseLerp(hardLimitDistance, maxDistance, _chainLength);
        }

        Vector3[] rawPositions = new Vector3[_visualRings.Count];
        for (int i = 0; i < _visualRings.Count; i++)
        {
            float uniformT = i / (float)(_visualRings.Count - 1);

            // Soft zone: remap t to compress rings toward the center
            float t = uniformT;
            if (softBlend > 0f)
            {
                float compressedT = ApplyCenterCompression(uniformT);
                t = Mathf.Lerp(uniformT, compressedT, softBlend);
            }

            Vector3 pos = SampleChain(points, t);

            // Hard zone: flatten the Y sag toward a straight line between p1 and p2
            if (hardBlend > 0f)
            {
                Vector3 p1Pos = player1.position;
                Vector3 p2Pos = player2.position;
                Vector3 straightPos = Vector3.Lerp(p1Pos, p2Pos, uniformT);

                Vector3 dir = p2Pos - p1Pos;
                float dist = dir.magnitude;

                // Raycast on geometry only (ignore Players and SlinkySegments)
                int layerMask = ~(LayerMask.GetMask("Player", "SlinkySegment"));
                bool pathClear = !Physics.Raycast(p1Pos, dir.normalized, dist, layerMask);

                if (pathClear)
                {
                    pos.y = Mathf.Lerp(pos.y, straightPos.y, hardBlend);
                }
            }

            rawPositions[i] = pos;
        }

        // 3-point smoothing pass
        Vector3[] smoothedPositions = new Vector3[_visualRings.Count];
        for (int i = 0; i < _visualRings.Count; i++)
        {
            Vector3 prev = rawPositions[Mathf.Max(i - 1, 0)];
            Vector3 curr = rawPositions[i];
            Vector3 next = rawPositions[Mathf.Min(i + 1, _visualRings.Count - 1)];
            smoothedPositions[i] = (prev + curr + next) / 3f;
        }

        for (int i = 0; i < _visualRings.Count; i++)
        {
            _visualRings[i].position = smoothedPositions[i];

            Vector3 prevPos = smoothedPositions[Mathf.Max(i - 1, 0)];
            Vector3 nextPos = smoothedPositions[Mathf.Min(i + 1, _visualRings.Count - 1)];
            Vector3 forward = (nextPos - prevPos).normalized;
            if (forward != Vector3.zero)
                _visualRings[i].rotation = Quaternion.LookRotation(forward);

            float spacing = i < _visualRings.Count - 1
                ? Vector3.Distance(smoothedPositions[i],     smoothedPositions[i + 1])
                : Vector3.Distance(smoothedPositions[i - 1], smoothedPositions[i]);

            _visualRings[i].localScale = new Vector3(visualRingScaleXY, visualRingScaleXY, spacing * visualRingScaleZ);

            Color ringColor;
            if (_chainLength <= softLimitDistance)
            {
                ringColor = softZoneColor;
            }
            else if (_chainLength <= hardLimitDistance)
            {
                float t = Mathf.InverseLerp(softLimitDistance, hardLimitDistance, _chainLength);
                ringColor = Color.Lerp(softZoneColor, mediumZoneColor, t);
            }
            else if (_chainLength > hardLimitDistance)
            {
                float t = Mathf.InverseLerp(hardLimitDistance, maxDistance, _chainLength);
                ringColor = Color.Lerp(mediumZoneColor, hardZoneColor, t);
            }
            else
            {
                ringColor = hardZoneColor;
            }

            _visualRings[i].GetComponent<MeshRenderer>().material.color = ringColor;
        }
    }

    /// <summary>
    /// Remaps a uniform [0,1] t value so that rings near the center (t=0.5) 
    /// are pulled closer together, simulating soft-zone slinky compression.
    /// </summary>
    float ApplyCenterCompression(float t)
    {
        // Map t into [-1, 1], apply power > 1 to compress center, map back to [0, 1]
        float centered = (t - 0.5f) * 2f;          // [-1, 1]
        float eased    = Mathf.Sign(centered) * Mathf.Pow(Mathf.Abs(centered), 1.5f); // compress center, stretch ends
        return (eased / 2f) + 0.5f;                 // back to [0, 1]
    }

    Vector3 SampleChain(List<Vector3> points, float t)
    {
        if (points.Count == 1) return points[0];
        float scaled = t * (points.Count - 1);
        int   index  = Mathf.Clamp((int)scaled, 0, points.Count - 2);
        float localT = scaled - index;
        return Vector3.Lerp(points[index], points[index + 1], localT);
    }


    // ─── Gizmos ───────────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (player1 == null || player2 == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(player1.position, player1.position + player1.transform.forward);
        Gizmos.DrawLine(player2.position, player2.position + player2.transform.forward);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetAirSlingshotState(SlinAndKyControllerBase.PlayerNumber.Player1).TargetPosition, 0.5f);
        Gizmos.DrawSphere(GetAirSlingshotState(SlinAndKyControllerBase.PlayerNumber.Player2).TargetPosition, 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        if (player1 == null || player2 == null) return;

        Vector3 midpoint = (player1.position + player2.position) * 0.5f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(player1.position, player2.position);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(midpoint, softLimitDistance * 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(midpoint, hardLimitDistance * 0.5f);
        
        float hardZoneSpan = Mathf.Max(0.0001f, maxDistance - hardLimitDistance);
        float percent01 = Mathf.Clamp01(hardRetractionForcePercent / 100f);
        float targetDist = hardLimitDistance + hardZoneSpan * percent01;
        Gizmos.color = new Color(255, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(midpoint, targetDist * 0.5f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(midpoint, maxDistance * 0.5f);
    }
}
