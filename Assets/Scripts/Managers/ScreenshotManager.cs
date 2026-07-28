using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotManager : MonoBehaviour
{
    public static ScreenshotManager Instance { get; private set; }
    public static int NextScreenshotID => Instance != null ? Instance.screenshots.Count + 1 : 1;
    
    private Dictionary<int, Sprite> screenshots = new();

    public event System.Action<int, Sprite> OnScreenshotUpdated;

    [SerializeField] private Canvas UiToHide;
    [SerializeField] private Image flashImage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void TakeScreenshot(int id)
    {
        if (Instance == null)
        {
            Debug.LogError("ScreenshotManager not found!");
            return;
        }

        Instance.TakeScreenshot_Internal(id);
    }

    void TakeScreenshot_Internal(int id)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "Screenshots");
        Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, $"screenshot_{id}.png");

        StartCoroutine(ScreenshotRoutine(filePath, id));
    }
    
    IEnumerator ScreenshotRoutine(string filePath, int id)
    {
        UiToHide.enabled = false;

        yield return new WaitForEndOfFrame();

        ScreenCapture.CaptureScreenshot(filePath);

        UiToHide.enabled = true;

        StartCoroutine(FlashEffect());

        yield return StartCoroutine(LoadScreenshot(filePath, id));
    }
    
    IEnumerator FlashEffect()
    {
        float duration = 0.2f;
        float timer = 0f;

        // Fade in quickly
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / duration);
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        // Fade out
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / duration);
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        flashImage.color = new Color(1f, 1f, 1f, 0f);
    }

    IEnumerator LoadScreenshot(string filePath, int id)
    {
        while (!File.Exists(filePath))
            yield return null;

        yield return new WaitForSeconds(0.1f);

        byte[] fileData = File.ReadAllBytes(filePath);

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        screenshots[id] = sprite;

        OnScreenshotUpdated?.Invoke(id, sprite);
        
        UiToHide.gameObject.SetActive(true);
    }

    public Sprite GetScreenshot(int id)
    {
        screenshots.TryGetValue(id, out var sprite);
        return sprite;
    }

    public void ClearScreenshots()
    {
        List<int> ids = new List<int>(screenshots.Keys);
        for (int i = 0; i < ids.Count; i++)
        {
            // Delete the file
            string folderPath = Path.Combine(Application.persistentDataPath, "Screenshots");
            string filePath = Path.Combine(folderPath, $"screenshot_{ids[i]}.png");
            if (File.Exists(filePath))
            {
                try                {
                    File.Delete(filePath);
                }
                catch (Exception e)                {
                    Debug.LogError($"Failed to delete screenshot file: {e.Message}");
                }
            }
            
            OnScreenshotUpdated?.Invoke(ids[i], null);
        }

        screenshots.Clear();
    }
}
