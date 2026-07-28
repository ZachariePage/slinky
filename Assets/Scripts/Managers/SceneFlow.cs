using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlow : MonoBehaviour
{
    public static SceneFlow Instance { get; private set; }

    [Header("Build Indices")]
    [SerializeField] private int menuIndex = 0;
    [SerializeField] private int bootstrapIndex = 3;

    private AsyncOperation preloadOp;
    private int preloadedBuildIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public bool CanUseIndex(int buildIndex)
    {
        return buildIndex != bootstrapIndex && buildIndex >= 0 && buildIndex < SceneManager.sceneCountInBuildSettings;
    }

    public int GetNextPlayableBuildIndex()
    {
        int active = SceneManager.GetActiveScene().buildIndex;
        int next = active + 1;

        // Walk forward; wrap to menu; always skip bootstrap.
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings + 2; i++)
        {
            if (next >= SceneManager.sceneCountInBuildSettings)
                return menuIndex;

            if (next == bootstrapIndex)
            {
                next++;
                continue;
            }

            return next;
        }

        return menuIndex;
    }

    public void PreloadScene(int buildIndex)
    {
        if (preloadOp != null && preloadedBuildIndex == buildIndex)
            return;

        CancelPreload();
        StartCoroutine(PreloadRoutine(buildIndex));
    }

    public void CancelPreload()
    {
        preloadOp = null;
        preloadedBuildIndex = -1;
    }

    public bool IsLoaded(int buildIndex)
    {
        var s = SceneManager.GetSceneByBuildIndex(buildIndex);
        return s.IsValid() && s.isLoaded;
    }

    public void ActivatePreloadedOrLoad(int buildIndex)
    {
        StartCoroutine(ActivateOrLoadRoutine(buildIndex));
    }

    private IEnumerator PreloadRoutine(int buildIndex)
    {
        if (IsLoaded(buildIndex))
        {
            preloadedBuildIndex = buildIndex;
            yield break;
        }

        preloadOp = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        preloadedBuildIndex = buildIndex;

        preloadOp.allowSceneActivation = false;

        while (preloadOp != null && preloadOp.progress < 0.9f)
            yield return null;
    }

    private IEnumerator ActivateOrLoadRoutine(int buildIndex)
    {
        // If we previously preloaded this scene, activate it.
        if (preloadOp != null && preloadedBuildIndex == buildIndex)
        {
            preloadOp.allowSceneActivation = true;
            while (!preloadOp.isDone)
                yield return null;
        }
        else if (!IsLoaded(buildIndex))
        {
            yield return SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        }

        // Make it active (lighting, Find\(\) defaults, etc.)
        var loaded = SceneManager.GetSceneByBuildIndex(buildIndex);
        if (loaded.IsValid() && loaded.isLoaded)
            SceneManager.SetActiveScene(loaded);

        // Unload everything else (including bootstrap if it was loaded).
        yield return UnloadAllExcept(buildIndex);

        // SceneManager.LoadScene(3, LoadSceneMode.Additive);

        // Clear preload handle if it was the one we activated.
        if (preloadedBuildIndex == buildIndex)
        {
            preloadOp = null;
            preloadedBuildIndex = -1;
        }
    }

    private IEnumerator UnloadAllExcept(int keepBuildIndex)
    {
        // Iterate backwards because sceneCount changes as we unload.
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var s = SceneManager.GetSceneAt(i);
            if (!s.IsValid() || !s.isLoaded)
                continue;

            if (s.buildIndex == keepBuildIndex)
                continue;

            yield return SceneManager.UnloadSceneAsync(s);
        }
    }
}
