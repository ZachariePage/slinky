using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtCarrousel : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private List<Sprite> sprites = new();

    [Header("UI References (three Images)")]
    [SerializeField] private Image leftImage;
    [SerializeField] private Image centerImage;
    [SerializeField] private Image rightImage;

    [Header("Timing")]
    [SerializeField] private float autoAdvanceSeconds = 3f;

    [Header("Layout")]
    [SerializeField] private float sideScale = 0.75f;
    [SerializeField, Range(0f, 1f)] private float sideAlpha = 0.3f;
    [SerializeField] private float centerScale = 1.0f;
    [SerializeField, Range(0f, 1f)] private float centerAlpha = 1.0f;

    [Header("Persistence")]
    [SerializeField] private string playerPrefsKey = "ArtCarrousel.CurrentIndex";

    private int currentIndex;
    private float timer;
    private bool isOpen;

    private void Awake()
    {
        currentIndex = Mathf.Clamp(PlayerPrefs.GetInt(playerPrefsKey, 0), 0, Mathf.Max(0, sprites.Count - 1));
        ApplyVisuals();
    }

    private void OnEnable()
    {
        isOpen = true;
        timer = 0f;
        ApplyVisuals();
    }

    private void OnDisable()
    {
        isOpen = false;
        SaveIndex();
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (sprites == null || sprites.Count <= 1)
            return;

        timer += Time.unscaledDeltaTime;
        if (timer >= autoAdvanceSeconds)
        {
            timer = 0f;
            Next();
        }
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
        if (!isOpen)
        {
            SaveIndex();
        }
        else
        {
            timer = 0f;
            ApplyVisuals();
        }
    }

    public void Next()
    {
        if (sprites == null || sprites.Count == 0)
            return;

        currentIndex = Mod(currentIndex + 1, sprites.Count);
        ApplyVisuals();
        SaveIndex();
    }

    public void Previous()
    {
        if (sprites == null || sprites.Count == 0)
            return;

        currentIndex = Mod(currentIndex - 1, sprites.Count);
        ApplyVisuals();
        SaveIndex();
    }

    public int GetCurrentIndex() => currentIndex;

    public void SetCurrentIndex(int index)
    {
        if (sprites == null || sprites.Count == 0)
        {
            currentIndex = 0;
            ApplyVisuals();
            SaveIndex();
            return;
        }

        currentIndex = Mathf.Clamp(index, 0, sprites.Count - 1);
        ApplyVisuals();
        SaveIndex();
    }

    private void ApplyVisuals()
    {
        bool hasAny = sprites != null && sprites.Count > 0;

        if (leftImage != null) leftImage.enabled = hasAny;
        if (centerImage != null) centerImage.enabled = hasAny;
        if (rightImage != null) rightImage.enabled = hasAny;

        if (!hasAny)
            return;

        int count = sprites.Count;
        int leftIndex = count == 1 ? currentIndex : Mod(currentIndex - 1, count);
        int rightIndex = count == 1 ? currentIndex : Mod(currentIndex + 1, count);

        SetImage(leftImage, sprites[leftIndex], sideAlpha, sideScale);
        SetImage(centerImage, sprites[currentIndex], centerAlpha, centerScale);
        SetImage(rightImage, sprites[rightIndex], sideAlpha, sideScale);
    }

    private static void SetImage(Image img, Sprite sprite, float alpha, float scale)
    {
        if (img == null)
            return;

        img.sprite = sprite;

        Color c = img.color;
        c.a = alpha;
        img.color = c;

        img.rectTransform.localScale = new Vector3(scale, scale, 1f);
    }

    private void SaveIndex()
    {
        PlayerPrefs.SetInt(playerPrefsKey, currentIndex);
        PlayerPrefs.Save();
    }

    private static int Mod(int x, int m)
    {
        if (m <= 0) return 0;
        int r = x % m;
        return r < 0 ? r + m : r;
    }
}
