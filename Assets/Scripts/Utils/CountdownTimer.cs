using System;
using System.Collections;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    public static CountdownTimer Instance { get; private set; }

    private Coroutine currentCountdown;

    public event Action<int> OnTimerStart;
    public event Action<int> OnTimerTick;
    public event Action OnTimerComplete;
    public event Action OnTimerCancel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void StartCountdown(int seconds, Action onComplete = null)
    {
        if (Instance == null)
        {
            Debug.LogError("CountdownTimer not found!");
            return;
        }

        Instance.StartCountdown_Internal(seconds, onComplete);
    }

    public static void Cancel()
    {
        if (Instance == null) return;

        Instance.Cancel_Internal();
    }

    void StartCountdown_Internal(int seconds, Action onComplete)
    {
        Cancel_Internal();

        currentCountdown = StartCoroutine(CountdownRoutine(seconds, onComplete));
    }

    void Cancel_Internal()
    {
        if (currentCountdown != null)
        {
            StopCoroutine(currentCountdown);
            currentCountdown = null;

            OnTimerCancel?.Invoke();
        }
    }

    IEnumerator CountdownRoutine(int seconds, Action onComplete)
    {
        OnTimerStart?.Invoke(seconds);

        int remaining = seconds;

        while (remaining > 0)
        {
            OnTimerTick?.Invoke(remaining);
            yield return new WaitForSeconds(1f);
            remaining--;
        }

        OnTimerTick?.Invoke(0);

        OnTimerComplete?.Invoke();
        onComplete?.Invoke();

        currentCountdown = null;
    }
}