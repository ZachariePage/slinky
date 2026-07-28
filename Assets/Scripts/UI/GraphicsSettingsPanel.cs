using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettingsPanel : MonoBehaviour
{
    [SerializeField] private AccessibilityController accessibility;

    [Header("UI")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Slider contrastSlider;

    [SerializeField] private Toggle bloomToggle;
    [SerializeField] private Toggle chromAbToggle;
    [SerializeField] private Toggle filmGrainToggle;
    [SerializeField] private Toggle paniniToggle;
    [SerializeField] private Toggle vignetteToggle;
    [SerializeField] private Toggle pixelShaderToggle;

    void OnEnable()
    {
        if (accessibility == null) return;
        accessibility.GraphicsSettingsApplied += RefreshFromController;
        RefreshFromController();
    }

    void OnDisable()
    {
        if (accessibility == null) return;
        accessibility.GraphicsSettingsApplied -= RefreshFromController;
    }
    
    private void RefreshFromController()
    {
        if (accessibility == null) return;

        brightnessSlider?.SetValueWithoutNotify(accessibility.GetBrightness());
        contrastSlider?.SetValueWithoutNotify(accessibility.GetContrast());

        bloomToggle?.SetIsOnWithoutNotify(accessibility.GetBloom());
        chromAbToggle?.SetIsOnWithoutNotify(accessibility.GetChromaticAberration());
        filmGrainToggle?.SetIsOnWithoutNotify(accessibility.GetFilmGrain());
        paniniToggle?.SetIsOnWithoutNotify(accessibility.GetPaniniProjection());
        vignetteToggle?.SetIsOnWithoutNotify(accessibility.GetVignette());
        pixelShaderToggle?.SetIsOnWithoutNotify(accessibility.GetPixelShader());
    }

    // Hook these up in the Inspector (UI \=\> OnValueChanged)
    public void OnBrightnessChanged(float v) => accessibility.SetBrightness(v);
    public void OnContrastChanged(float v) => accessibility.SetContrast(v);

    public void OnBloomChanged(bool v) => accessibility.ToggleBloom(v);
    public void OnChromAbChanged(bool v) => accessibility.ToggleChromaticAberration(v);
    public void OnFilmGrainChanged(bool v) => accessibility.ToggleFilmGrain(v);
    public void OnPaniniChanged(bool v) => accessibility.TogglePaniniProjection(v);
    public void OnVignetteChanged(bool v) => accessibility.ToggleVignette(v);
    public void OnPixelShaderChanged(bool v) => accessibility.TogglePixelShader(v);
}
