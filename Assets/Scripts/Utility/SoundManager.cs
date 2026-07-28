using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public float SfxVolume => PlayerPrefs.GetFloat("SFXVolume", 1f);
    public float MusicVolume => PlayerPrefs.GetFloat("MusicVolume", 1f);

    [SerializeField] private AudioSource musicSource;

    [SerializeField] private SFXSettings sfxSettings;

    [SerializeField] private GameObject audioPlayerPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

    }

    private void Start()
    {
        if (!musicSource)
        {
            musicSource = GameObject.FindWithTag("MainCamera").GetComponent<AudioSource>();
        }

        if (musicSource && !musicSource.isPlaying)
            musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1, float pitch = 1)
    {
        GameObject audioObject = ObjectPool.Instance.GetPooledObject(audioPlayerPrefab);
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();
        audioObject.transform.position = position;
        audioSource.clip = clip;
        audioSource.volume = volume * SfxVolume;
        audioSource.pitch = pitch;
        audioObject.SetActive(true);
        audioSource.enabled = true; 
        audioSource.Play();

        StartCoroutine(ObjectPool.ReturnToPoolAfterDelay(audioSource.gameObject, clip.length));
        
    }
    
    //Play sfx with random sounds, but to use it we need to resetAudioSource before each use first. Right now not using this anywhere but maybe for later
    public void PlaySFXWithRandomValues(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        GameObject audioObject = ObjectPool.Instance.GetPooledObject(audioPlayerPrefab);
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();

        audioObject.transform.position = position;

        float randomPitch = Random.Range(sfxSettings.pitchMin, sfxSettings.pitchMax);
        float randomVolume = Random.Range(sfxSettings.volumeMin, sfxSettings.volumeMax);

        audioSource.clip = clip;
        audioSource.pitch = pitch * sfxSettings.basePitch * randomPitch;
        audioSource.volume = volume * sfxSettings.baseVolume * randomVolume * SfxVolume;

        audioObject.SetActive(true);
        audioSource.enabled = true;
        audioSource.Play();

        StartCoroutine(
            ObjectPool.ReturnToPoolAfterDelay(
                audioSource.gameObject,
                clip.length / audioSource.pitch
            )
        );
    }

    //not used yet
    public static void ResetAudioSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.time = 0f;
        source.enabled = true;
        
        source.volume = 1f;
        source.pitch = 1f;
        
        source.loop = false;
        source.playOnAwake = false;
        
        source.spatialBlend = 0f;          
        source.dopplerLevel = 0f;         
        source.spread = 0f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 1f;
        source.maxDistance = 500f;
        
        source.outputAudioMixerGroup = null;
        
        source.reverbZoneMix = 1f;
    }


    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        musicSource.volume = volume;
    }
}