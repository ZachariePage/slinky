using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] private Image fadeImage;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Start fully black (alpha 1 for black)
        fadeImage.color = new Color(0, 0, 0, 1f);
    }


    public static void FadeOut(float duration)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.FadeRoutine(1f, duration));
    }

    public static void FadeIn(float duration)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.FadeRoutine(0f, duration));
    }

    public static void FadeSequence(float fadeOutDuration, float waitTime, float fadeInDuration)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.FadeSequenceRoutine(fadeOutDuration, waitTime, fadeInDuration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);

            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }

    private IEnumerator FadeSequenceRoutine(float fadeOutDuration, float waitTime, float fadeInDuration)
    {
        yield return FadeRoutine(1f, fadeOutDuration);
        yield return new WaitForSeconds(waitTime);
        yield return FadeRoutine(0f, fadeInDuration);
    }
}
