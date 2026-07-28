using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AccessibilityController : MonoBehaviour
{
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private UniversalRendererData rendererData;

    // Volume settings
    private ColorAdjustments _colorAdjustments; // Brightness and contrast
    private Bloom _bloom;
    private ChromaticAberration _chromaticAberration;
    private FilmGrain _filmGrain;
    private PaniniProjection _paniniProjection;
    private Vignette _vignette;
    
    // Renderer feature
    private FullScreenPassRendererFeature _fullScreenFeature;

    public event Action GraphicsSettingsApplied;
    
    
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";
    [SerializeField] private string musicVolumeParam = "MusicVolume";

    public event Action AudioSettingsApplied;
    
    private static class PrefKeys
    {
        // Graphics
        public const string Brightness = "settings.graphics.brightness";
        public const string Contrast = "settings.graphics.contrast";
        public const string Bloom = "settings.graphics.bloom";
        public const string ChromaticAberration = "settings.graphics.chromAb";
        public const string FilmGrain = "settings.graphics.filmGrain";
        public const string PaniniProjection = "settings.graphics.panini";
        public const string Vignette = "settings.graphics.vignette";
        public const string PixelShader = "settings.graphics.pixelShader";
        
        // Audio
        public const string AudioMaster = "settings.audio.master";
        public const string AudioSfx = "settings.audio.sfx";
        public const string AudioMusic = "settings.audio.music";
    }
    
    private const float DefaultBrightness = 50f;
    private const float DefaultContrast = 33f;
    
    private const float AudioMinDb = -70f;
    private const float AudioMaxDb = 0f;
    private const float MusicMaxDb = -18f;

    void Awake()
    {
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out _colorAdjustments);
            postProcessVolume.profile.TryGet(out _bloom);
            postProcessVolume.profile.TryGet(out _chromaticAberration);
            postProcessVolume.profile.TryGet(out _filmGrain);
            postProcessVolume.profile.TryGet(out _paniniProjection);
            postProcessVolume.profile.TryGet(out _vignette);
        }

        if (rendererData != null)
        {
            foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
            {
                if (feature is FullScreenPassRendererFeature fsFeature)
                {
                    _fullScreenFeature = fsFeature;
                    break;
                }
            }
        }
        
        LoadAndApplyGraphicsSettings();
        LoadAndApplyAudioSettings();
    }

#region Settings Application

    

    
    private void LoadAndApplyGraphicsSettings()
    {
        SetBrightness(GetBrightness());
        SetContrast(GetContrast());

        ToggleBloom(GetBloom());
        ToggleChromaticAberration(GetChromaticAberration());
        ToggleFilmGrain(GetFilmGrain());
        TogglePaniniProjection(GetPaniniProjection());
        ToggleVignette(GetVignette());
        TogglePixelShader(GetPixelShader());
        
        GraphicsSettingsApplied?.Invoke();
    }
    
    private void LoadAndApplyAudioSettings()
    {
        SetMasterVolume(GetMasterVolume());
        SetSfxVolume(GetSfxVolume());
        SetMusicVolume(GetMusicVolume());

        AudioSettingsApplied?.Invoke();
    }
    
#endregion
    
#region Graphics Getters Functions
    
    public float GetBrightness() => PlayerPrefs.GetFloat(PrefKeys.Brightness, DefaultBrightness);
    public float GetContrast() => PlayerPrefs.GetFloat(PrefKeys.Contrast, DefaultContrast);

    public bool GetBloom() => PlayerPrefs.GetInt(PrefKeys.Bloom, 1) == 1;
    public bool GetChromaticAberration() => PlayerPrefs.GetInt(PrefKeys.ChromaticAberration, 1) == 1;
    public bool GetFilmGrain() => PlayerPrefs.GetInt(PrefKeys.FilmGrain, 1) == 1;
    public bool GetPaniniProjection() => PlayerPrefs.GetInt(PrefKeys.PaniniProjection, 1) == 1;
    public bool GetVignette() => PlayerPrefs.GetInt(PrefKeys.Vignette, 1) == 1;
    public bool GetPixelShader() => PlayerPrefs.GetInt(PrefKeys.PixelShader, 1) == 1;
    
#endregion
    
#region Graphics Settings Functions

    // Allow -2 to +2 (show 0 to 100 as slider)
    public void SetBrightness(float value)
    {
        value = Mathf.Clamp(value, 0f, 100f);
        PlayerPrefs.SetFloat(PrefKeys.Brightness, value);

        // 0 = -2 | 50 = 0 | 100 = +2
        float adjustedValue = (value - 50f) / 25f;
        _colorAdjustments?.postExposure.Override(adjustedValue);
    }

    // Allow -50 to +100 (show 0 to 100 as slider)
    public void SetContrast(float value)
    {
        value = Mathf.Clamp(value, 0f, 100f);
        PlayerPrefs.SetFloat(PrefKeys.Contrast, value);

        // 0 = -50 | 33 = 0 | 66 = +50 | 100 = +100
        float adjustedValue = (value - 33f) * 1.5f;
        _colorAdjustments?.contrast.Override(adjustedValue);
    }

    public void ToggleBloom(bool value)
    {
        PlayerPrefs.SetInt(PrefKeys.Bloom, value ? 1 : 0);
        if (_bloom != null) _bloom.active = value;
    }

    public void ToggleChromaticAberration(bool value)
    {
        PlayerPrefs.SetInt(PrefKeys.ChromaticAberration, value ? 1 : 0);
        if (_chromaticAberration != null) _chromaticAberration.active = value;
    }
    
    public void ToggleFilmGrain(bool value)
    {
        PlayerPrefs.SetInt(PrefKeys.FilmGrain, value ? 1 : 0);
        if (_filmGrain != null) _filmGrain.active = value;
    }
    
    public void TogglePaniniProjection(bool value)
    {
        PlayerPrefs.SetInt(PrefKeys.PaniniProjection, value ? 1 : 0);
        if (_paniniProjection != null) _paniniProjection.active = value;
    }
    
    public void ToggleVignette(bool value)
    {
        PlayerPrefs.SetInt(PrefKeys.Vignette, value ? 1 : 0);
        if (_vignette != null) _vignette.active = value;
    }

    public void TogglePixelShader(bool value)
    {
        PlayerPrefs.SetInt(PrefKeys.PixelShader, value ? 1 : 0);
        if (_fullScreenFeature != null) _fullScreenFeature.SetActive(value);
    }
    
#endregion
    
#region Audio Getters Functions
    
    public float GetMasterVolume() => PlayerPrefs.GetFloat(PrefKeys.AudioMaster, 1f);
    public float GetSfxVolume() => PlayerPrefs.GetFloat(PrefKeys.AudioSfx, 1f);
    public float GetMusicVolume() => PlayerPrefs.GetFloat(PrefKeys.AudioMusic, 1f);

#endregion
    
#region Audio Setters Functions
    
    public void SetMasterVolume(float value01)
    {
        value01 = Mathf.Clamp01(value01);
        PlayerPrefs.SetFloat(PrefKeys.AudioMaster, value01);
        SetMixerDb(masterVolumeParam, Slider01ToDb(value01));
    }

    public void SetSfxVolume(float value01)
    {
        value01 = Mathf.Clamp01(value01);
        PlayerPrefs.SetFloat(PrefKeys.AudioSfx, value01);
        SetMixerDb(sfxVolumeParam, Slider01ToDb(value01, true));
    }

    public void SetMusicVolume(float value01)
    {
        value01 = Mathf.Clamp01(value01);
        PlayerPrefs.SetFloat(PrefKeys.AudioMusic, value01);
        SetMixerDb(musicVolumeParam, Slider01ToDb(value01));
    }

    private static float Slider01ToDb(float value01, bool isMusic = false)
    {
        if(isMusic)
            return Mathf.Lerp(AudioMinDb, MusicMaxDb, Mathf.Clamp01(value01));
            
        
        // 0 => -70, 1 => 0
        return Mathf.Lerp(AudioMinDb, AudioMaxDb, Mathf.Clamp01(value01));
    }

    private void SetMixerDb(string exposedParam, float db)
    {
        if (audioMixer == null) return;
        if (string.IsNullOrWhiteSpace(exposedParam)) return;
        audioMixer.SetFloat(exposedParam, db);
    }
    
#endregion
    
#region Save & Resets

    public void SaveNow()
    {
        PlayerPrefs.Save();
    }
    
    public void ResetGraphicsToDefaults()
    {
        PlayerPrefs.DeleteKey(PrefKeys.Brightness);
        PlayerPrefs.DeleteKey(PrefKeys.Contrast);
        PlayerPrefs.DeleteKey(PrefKeys.Bloom);
        PlayerPrefs.DeleteKey(PrefKeys.ChromaticAberration);
        PlayerPrefs.DeleteKey(PrefKeys.FilmGrain);
        PlayerPrefs.DeleteKey(PrefKeys.PaniniProjection);
        PlayerPrefs.DeleteKey(PrefKeys.Vignette);
        PlayerPrefs.DeleteKey(PrefKeys.PixelShader);

        LoadAndApplyGraphicsSettings();
        PlayerPrefs.Save();
    }
    
    public void ResetAudioToDefaults()
    {
        PlayerPrefs.DeleteKey(PrefKeys.AudioMaster);
        PlayerPrefs.DeleteKey(PrefKeys.AudioSfx);
        PlayerPrefs.DeleteKey(PrefKeys.AudioMusic);

        LoadAndApplyAudioSettings();
        PlayerPrefs.Save();
    }
    
#endregion
    
}