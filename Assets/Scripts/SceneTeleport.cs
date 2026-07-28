using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleport : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName = "TestShawn"; // OR use build index
    public int sceneIndex = 2;            // optional alternative

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Load by index if set
        if (sceneIndex >= 0)
            SceneManager.LoadScene(sceneIndex);
        else
            SceneManager.LoadScene(sceneName);
    }
}