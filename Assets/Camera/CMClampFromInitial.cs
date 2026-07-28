// =======================
// CMClampFromInitial.cs
// =======================
using UnityEngine;
using Unity.Cinemachine;

[ExecuteAlways]
public class CMClampFromInitial : CinemachineExtension
{
    [System.Flags]
    public enum ClampAxes
    {
        None = 0,
        Yaw = 1 << 0, // around Up
        Pitch = 1 << 1, // around Right
        Roll = 1 << 2, // around Forward
        All = Yaw | Pitch | Roll
    }

    [Header("Clamp")]
    public ClampAxes axes = ClampAxes.All;

    [Tooltip("Max allowed angle from original center, and also from post-blend center (degrees).")]
    public float maxAngleFromStart = 15f;

    [Header("Post-blend union range")]
    [Tooltip("If true, expands allowed range to include both initial±max and postBlend±max (union).")]
    public bool usePostBlendUnion = true;

    private Quaternion _initialRotation;
    private bool _initialized = false;

    private bool _requestPostBlendCapture = false;

    // Post-blend centers stored as signed deltas from initial (per axis)
    private bool _hasPostYaw, _hasPostPitch, _hasPostRoll;
    private float _postYaw, _postPitch, _postRoll;

    protected override void OnEnable()
    {
        base.OnEnable();
        ResetState();
    }

    public override bool OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
    {
        ResetState();
        return base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
    }

    void ResetState()
    {
        _initialized = false;
        _requestPostBlendCapture = false;

        _hasPostYaw = _hasPostPitch = _hasPostRoll = false;
        _postYaw = _postPitch = _postRoll = 0f;
    }

    // NEW: Call this when you reset the camera transform while it is inactive.
    // Next time it becomes active, it will recapture the reference from the new orientation.
    public void ResetClampState()
    {
        ResetState();
    }

    // Called by manager after blend time (you currently use blend=0, but keeping it for later)
    public void RequestPostBlendCapture() => _requestPostBlendCapture = true;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Aim)
            return;

        Vector3 up = state.ReferenceUp;

        if (!_initialized)
        {
            _initialRotation = state.RawOrientation;
            _initialized = true;
            return;
        }

        // Capture post-blend axis deltas (once requested)
        if (_requestPostBlendCapture)
        {
            _requestPostBlendCapture = false;

            if ((axes & ClampAxes.Yaw) != 0)
            {
                _postYaw = SignedYawDelta(_initialRotation, state.RawOrientation, up);
                _hasPostYaw = true;
            }

            if ((axes & ClampAxes.Pitch) != 0)
            {
                _postPitch = SignedPitchDelta(_initialRotation, state.RawOrientation);
                _hasPostPitch = true;
            }

            if ((axes & ClampAxes.Roll) != 0)
            {
                _postRoll = SignedRollDelta(_initialRotation, state.RawOrientation);
                _hasPostRoll = true;
            }

            return;
        }

        float max = Mathf.Max(0f, maxAngleFromStart);

        // Current deltas
        float curYaw = 0f, curPitch = 0f, curRoll = 0f;

        if ((axes & ClampAxes.Yaw) != 0)
            curYaw = SignedYawDelta(_initialRotation, state.RawOrientation, up);

        if ((axes & ClampAxes.Pitch) != 0)
            curPitch = SignedPitchDelta(_initialRotation, state.RawOrientation);

        if ((axes & ClampAxes.Roll) != 0)
            curRoll = SignedRollDelta(_initialRotation, state.RawOrientation);

        // Allowed ranges per axis
        float yawMin = -max, yawMax = +max;
        float pitchMin = -max, pitchMax = +max;
        float rollMin = -max, rollMax = +max;

        if (usePostBlendUnion)
        {
            if (_hasPostYaw)
            {
                yawMin = Mathf.Min(yawMin, _postYaw - max);
                yawMax = Mathf.Max(yawMax, _postYaw + max);
            }

            if (_hasPostPitch)
            {
                pitchMin = Mathf.Min(pitchMin, _postPitch - max);
                pitchMax = Mathf.Max(pitchMax, _postPitch + max);
            }

            if (_hasPostRoll)
            {
                rollMin = Mathf.Min(rollMin, _postRoll - max);
                rollMax = Mathf.Max(rollMax, _postRoll + max);
            }
        }

        // Clamp
        float clYaw = curYaw, clPitch = curPitch, clRoll = curRoll;
        bool changed = false;

        if ((axes & ClampAxes.Yaw) != 0)
        {
            clYaw = Mathf.Clamp(curYaw, yawMin, yawMax);
            changed |= !Mathf.Approximately(clYaw, curYaw);
        }

        if ((axes & ClampAxes.Pitch) != 0)
        {
            clPitch = Mathf.Clamp(curPitch, pitchMin, pitchMax);
            changed |= !Mathf.Approximately(clPitch, curPitch);
        }

        if ((axes & ClampAxes.Roll) != 0)
        {
            clRoll = Mathf.Clamp(curRoll, rollMin, rollMax);
            changed |= !Mathf.Approximately(clRoll, curRoll);
        }

        if (!changed)
            return;

        // Rebuild orientation by clamping requested axes and preserving the remaining "rest"
        state.RawOrientation = RebuildOrientation(_initialRotation, state.RawOrientation, up, axes, clYaw, clPitch, clRoll);
    }

    // ----------------- Deltas -----------------

    static float SignedYawDelta(Quaternion reference, Quaternion current, Vector3 up)
    {
        Vector3 refFwd = Vector3.ProjectOnPlane(reference * Vector3.forward, up).normalized;
        Vector3 curFwd = Vector3.ProjectOnPlane(current * Vector3.forward, up).normalized;
        if (refFwd.sqrMagnitude < 1e-6f || curFwd.sqrMagnitude < 1e-6f) return 0f;
        return Vector3.SignedAngle(refFwd, curFwd, up);
    }

    static float SignedPitchDelta(Quaternion reference, Quaternion current)
    {
        Vector3 right = (reference * Vector3.right).normalized;

        Vector3 refFwd = Vector3.ProjectOnPlane(reference * Vector3.forward, right).normalized;
        Vector3 curFwd = Vector3.ProjectOnPlane(current * Vector3.forward, right).normalized;
        if (refFwd.sqrMagnitude < 1e-6f || curFwd.sqrMagnitude < 1e-6f) return 0f;

        return Vector3.SignedAngle(refFwd, curFwd, right);
    }

    static float SignedRollDelta(Quaternion reference, Quaternion current)
    {
        Vector3 fwd = (reference * Vector3.forward).normalized;

        Vector3 refUp = Vector3.ProjectOnPlane(reference * Vector3.up, fwd).normalized;
        Vector3 curUp = Vector3.ProjectOnPlane(current * Vector3.up, fwd).normalized;
        if (refUp.sqrMagnitude < 1e-6f || curUp.sqrMagnitude < 1e-6f) return 0f;

        return Vector3.SignedAngle(refUp, curUp, fwd);
    }

    // ----------------- Rebuild -----------------

    static Quaternion RebuildOrientation(
        Quaternion initial,
        Quaternion current,
        Vector3 up,
        ClampAxes axes,
        float clampedYaw,
        float clampedPitch,
        float clampedRoll)
    {
        // Order: Yaw (up) -> Pitch (initial right) -> Roll (initial forward)
        Quaternion baseRot = initial;

        if ((axes & ClampAxes.Yaw) != 0)
            baseRot = Quaternion.AngleAxis(clampedYaw, up) * baseRot;

        if ((axes & ClampAxes.Pitch) != 0)
        {
            Vector3 right = (initial * Vector3.right).normalized;
            baseRot = Quaternion.AngleAxis(clampedPitch, right) * baseRot;
        }

        if ((axes & ClampAxes.Roll) != 0)
        {
            Vector3 fwd = (initial * Vector3.forward).normalized;
            baseRot = Quaternion.AngleAxis(clampedRoll, fwd) * baseRot;
        }

        // Build "current base" the same way, so we can preserve any remaining rotation
        Quaternion currentBase = initial;

        if ((axes & ClampAxes.Yaw) != 0)
        {
            float curYaw = SignedYawDelta(initial, current, up);
            currentBase = Quaternion.AngleAxis(curYaw, up) * currentBase;
        }

        if ((axes & ClampAxes.Pitch) != 0)
        {
            Vector3 right = (initial * Vector3.right).normalized;
            float curPitch = SignedPitchDelta(initial, current);
            currentBase = Quaternion.AngleAxis(curPitch, right) * currentBase;
        }

        if ((axes & ClampAxes.Roll) != 0)
        {
            Vector3 fwd = (initial * Vector3.forward).normalized;
            float curRoll = SignedRollDelta(initial, current);
            currentBase = Quaternion.AngleAxis(curRoll, fwd) * currentBase;
        }

        // Preserve remaining rotation (usually tiny; keeps behavior stable)
        Quaternion rest = Quaternion.Inverse(currentBase) * current;
        return baseRot * rest;
    }
}