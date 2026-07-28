using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsPanel : MonoBehaviour
{
    [SerializeField] private AccessibilityController accessibility;

    [Header("UI")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    void OnEnable()
    {
        if (accessibility == null) return;
        accessibility.AudioSettingsApplied += RefreshFromController;
        RefreshFromController();
    }

    void OnDisable()
    {
        if (accessibility == null) return;
        accessibility.AudioSettingsApplied -= RefreshFromController;
    }

    private void RefreshFromController()
    {
        if (accessibility == null) return;

        masterSlider?.SetValueWithoutNotify(accessibility.GetMasterVolume());
        sfxSlider?.SetValueWithoutNotify(accessibility.GetSfxVolume());
        musicSlider?.SetValueWithoutNotify(accessibility.GetMusicVolume());
    }

    // Hook these up in the Inspector (UI \=\> OnValueChanged)
    public void OnMasterChanged(float v) => accessibility.SetMasterVolume(v);
    public void OnSfxChanged(float v) => accessibility.SetSfxVolume(v);
    public void OnMusicChanged(float v) => accessibility.SetMusicVolume(v);
}