// =======================
// CameraPriorityManager.cs
// =======================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraPriorityManager : MonoBehaviour
{
    [Header("References")]
    public CinemachineBrain brain;

    [Header("Cameras")]
    public CinemachineCamera[] cameras;

    [Header("Default Camera")]
    public CinemachineCamera defaultCamera;

    [Header("Priorities")]
    public int activePriority = 20;
    public int inactivePriority = 0;

    [Header("Composer Switching")]
    public bool enableComposerOneFrameBeforeBlend = true;

    // Store initial rotations
    private Dictionary<CinemachineCamera, Quaternion> _initialRotations =
        new Dictionary<CinemachineCamera, Quaternion>();

    private CinemachineCamera _currentCamera;
    private Coroutine _switchRoutine;

    void Start()
    {
        if (brain == null)
        {
            Debug.LogError("CameraPriorityManager: brain not assigned.");
            return;
        }

        // Store initial rotations + start with all composers disabled
        foreach (var cam in cameras)
        {
            if (cam == null) continue;

            _initialRotations[cam] = cam.transform.rotation;

            var composer = cam.GetComponent<CinemachineRotationComposer>();
            if (composer != null)
                composer.enabled = false;

            cam.Priority = inactivePriority;
        }

        if (defaultCamera == null && cameras != null && cameras.Length > 0)
            defaultCamera = cameras[0];

        if (defaultCamera != null)
        {
            defaultCamera.Priority = activePriority;
            _currentCamera = defaultCamera;
            SetComposerEnabled(_currentCamera, true);
        }
        else
        {
            _currentCamera = AsCinemachineCamera(brain.ActiveVirtualCamera);

            if (_currentCamera != null)
                SetComposerEnabled(_currentCamera, true);
        }
    }

    public void MakeLive(CinemachineCamera cam)
    {
        if (cam == null || brain == null)
            return;

        if (_currentCamera == cam)
            return;

        if (_switchRoutine != null)
            StopCoroutine(_switchRoutine);

        _switchRoutine = StartCoroutine(SwitchRoutine(cam));
    }

    public void ReturnToDefault()
    {
        if (defaultCamera == null)
            return;

        MakeLive(defaultCamera);
    }

    IEnumerator SwitchRoutine(CinemachineCamera cam)
    {
        // Reset all other cameras
        foreach (var c in cameras)
        {
            if (c == null || c == cam) continue;

            ResetCameraToInitial(c);
            c.Priority = inactivePriority;
        }

        // Enable composer for incoming cam before blend if wanted
        if (enableComposerOneFrameBeforeBlend)
        {
            SetComposerEnabled(cam, true);
            yield return null;
        }
        else
        {
            SetComposerEnabled(cam, true);
        }

        cam.Priority = activePriority;
        _currentCamera = cam;
        _switchRoutine = null;
    }

    void ResetCameraToInitial(CinemachineCamera cam)
    {
        if (cam == null) return;

        SetComposerEnabled(cam, false);

        if (_initialRotations.TryGetValue(cam, out var rot))
            cam.transform.rotation = rot;

        var clamp = cam.GetComponent<CMClampFromInitial>();
        if (clamp != null)
            clamp.ResetClampState();
    }

    static CinemachineCamera AsCinemachineCamera(ICinemachineCamera cam)
    {
        if (cam == null) return null;
        var comp = cam as Component;
        if (comp == null) return null;
        return comp.GetComponent<CinemachineCamera>();
    }

    static void SetComposerEnabled(CinemachineCamera cam, bool enabled)
    {
        if (cam == null) return;

        var composer = cam.GetComponent<CinemachineRotationComposer>();
        if (composer != null)
            composer.enabled = enabled;
    }

    public void ForceReturnToDefault()
    {
        if (defaultCamera == null)
            return;

        if (_switchRoutine != null)
        {
            StopCoroutine(_switchRoutine);
            _switchRoutine = null;
        }

        foreach (var cam in cameras)
        {
            if (cam == null) continue;

            ResetCameraToInitial(cam);
            cam.Priority = inactivePriority;
        }

        defaultCamera.Priority = activePriority;
        _currentCamera = defaultCamera;
        SetComposerEnabled(defaultCamera, true);
    }

}