using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class CameraPriorityTrigger : MonoBehaviour
{
    public CameraPriorityManager manager;
    public CinemachineCamera cameraToActivate;
    public string playerTag = "Player";

    private bool disableAfterNextExit = false;
    private bool isDisabledPermanently = false;

    private HashSet<Collider> playersInside = new HashSet<Collider>();

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnEnable()
    {
        TryFindManager();

        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnPlayersDespawned += HandlePlayersDespawned;
        }
    }

    private void OnDisable()
    {
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnPlayersDespawned -= HandlePlayersDespawned;
        }
    }

    public void DisableAfterPlayersExit()
    {
        disableAfterNextExit = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryFindManager();

        if (!other.CompareTag(playerTag)) return;
        if (manager == null || cameraToActivate == null) return;
        if (isDisabledPermanently) return;

        CleanupPlayersInside();

        if (!playersInside.Contains(other))
            playersInside.Add(other);

        if (playersInside.Count > 0)
            manager.MakeLive(cameraToActivate);
    }

    private void OnTriggerExit(Collider other)
    {
        TryFindManager();

        if (!other.CompareTag(playerTag)) return;
        if (manager == null) return;

        if (playersInside.Contains(other))
            playersInside.Remove(other);

        EvaluateCameraState();
    }

    private void HandlePlayersDespawned()
    {
        playersInside.Clear();

        TryFindManager();

        if (manager != null)
            manager.ForceReturnToDefault();

        if (disableAfterNextExit)
        {
            isDisabledPermanently = true;
            disableAfterNextExit = false;
        }
    }

    private void TryFindManager()
    {
        if (manager == null)
            manager = FindFirstObjectByType<CameraPriorityManager>();
    }

    private void CleanupPlayersInside()
    {
        playersInside.RemoveWhere(c => c == null || !c.gameObject.activeInHierarchy);
    }

    private void EvaluateCameraState()
    {
        CleanupPlayersInside();

        if (playersInside.Count == 0)
        {
            manager.ReturnToDefault();

            if (disableAfterNextExit)
            {
                isDisabledPermanently = true;
                disableAfterNextExit = false;
            }
        }
    }
}