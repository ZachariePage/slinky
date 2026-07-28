using UnityEngine;
using UnityEngine.UI;

public class ScreenshotSlot : MonoBehaviour
{
    [SerializeField] private int screenshotID;
    [SerializeField] private Image image;

    private ScreenshotManager manager;

    void Start()
    {
        manager = ScreenshotManager.Instance;

        if (manager == null)
        {
            Debug.LogError("ScreenshotManager not found!");
            return;
        }

        manager.OnScreenshotUpdated += HandleScreenshotUpdated;

        var sprite = manager.GetScreenshot(screenshotID);
        ApplySprite(sprite); 
    }

    void OnDestroy()
    {
        if (manager != null)
            manager.OnScreenshotUpdated -= HandleScreenshotUpdated;
    }

    void HandleScreenshotUpdated(int id, Sprite sprite)
    {
        if (id != screenshotID)
            return;

        ApplySprite(sprite);
    }

    void ApplySprite(Sprite sprite)
    {
        if (image == null) return;

        if (sprite == null)
        {
            image.sprite = null;
            image.color = new Color(137f/255f, 0f, 0f, 1f);
            return;
        }

        image.enabled = true;
        image.color = new Color(1f, 1f, 1f, 1f);
        image.sprite = sprite;
    }
}