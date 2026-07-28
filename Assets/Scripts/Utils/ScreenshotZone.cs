using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScreenshotZone : MonoBehaviour
{
    [Header("Screenshot Settings")]
    private int screenshotID = 0;
    [SerializeField] private int countdownTimer = 3;
    [SerializeField] private bool allowScreenshotRetakes = false;
    private int takenScreenshotAmount = 0;

    [Header("Objects to enable on screenshot")]
    [SerializeField] private List<GameObject> objectsToEnableOnScreenshot = new();

    [Header("Objects to disable on screenshot")]
    [SerializeField] private List<GameObject> objectsToDisableOnScreenshot = new();

    [Header("Optional Camera Behaviour")]
    [SerializeField] private bool disableCameraTriggerAfterScreenshot = false;
    [SerializeField] private CameraPriorityTrigger cameraTriggerToDisable;

    [Header("Cues")]
    [SerializeField] private GameCue onStartCountdownCue;
    [SerializeField] private GameCue onStopCountdownCue;
    [SerializeField] private List<GameCue> onScreenshotCues = new();
    [SerializeField] private Vector3 vfxPosition;

    private List<GameObject> playersInZone = new();

    private void Awake()
    {
        // Ensure correct initial state
        foreach (GameObject obj in objectsToEnableOnScreenshot)
            if (obj) obj.SetActive(false);

        foreach (GameObject obj in objectsToDisableOnScreenshot)
            if (obj) obj.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!allowScreenshotRetakes && takenScreenshotAmount > 0)
            return;

        if (other.CompareTag("Player"))
        {
            if (!playersInZone.Contains(other.gameObject))
            {
                playersInZone.Add(other.gameObject);

                if (playersInZone.Count >= 2)
                {
                    if (onStartCountdownCue)
                        onStartCountdownCue.Execute(vfxPosition);

                    CountdownTimer.StartCountdown(countdownTimer, () =>
                    {
                        StartCoroutine(DoScreenshot());
                    });
                }
            }
        }
    }

    IEnumerator DoScreenshot()
    {
        if (screenshotID == 0)
            screenshotID = ScreenshotManager.NextScreenshotID;

        ScreenshotManager.TakeScreenshot(screenshotID);

        yield return new WaitForEndOfFrame();

        takenScreenshotAmount++;

        // ENABLE objects
        foreach (GameObject obj in objectsToEnableOnScreenshot)
            if (obj) obj.SetActive(true);

        // DISABLE objects
        foreach (GameObject obj in objectsToDisableOnScreenshot)
            if (obj) obj.SetActive(false);

        if (onScreenshotCues.Count > 0)
            foreach (GameCue cue in onScreenshotCues)
                cue.Execute(vfxPosition);

        if (disableCameraTriggerAfterScreenshot && cameraTriggerToDisable != null)
            cameraTriggerToDisable.DisableAfterPlayersExit();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playersInZone.Contains(other.gameObject))
                playersInZone.Remove(other.gameObject);

            if (!allowScreenshotRetakes && takenScreenshotAmount > 0)
                return;

            if (playersInZone.Count < 2)
            {
                if (onStopCountdownCue)
                    onStopCountdownCue.Execute(vfxPosition);

                CountdownTimer.Cancel();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (vfxPosition != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(vfxPosition, 0.25f);
        }
    }
}