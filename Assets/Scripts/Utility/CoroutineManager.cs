using UnityEngine;
using System;
using System.Collections;

public class CoroutineManager : MonoBehaviour
{
    public static CoroutineManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Run(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public void RunDelayed(float delay, Action action)
    {
        StartCoroutine(DelayCoroutine(delay, action));
    }

    private IEnumerator DelayCoroutine(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}