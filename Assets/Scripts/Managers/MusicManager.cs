using UnityEngine;

[System.Serializable]
public class TwoPartMusic
{
    public AudioClip Intro;
    public AudioClip Loop;
}

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public enum MusicType
    {
        None,
        MainMenu,
        Gameplay
    }

    private enum MusicState
    {
        Stopped,
        PlayingIntro,
        PlayingLoop
    }

    public static MusicManager Instance { get; private set; }

    [Header("Tracks")]
    [SerializeField] private TwoPartMusic mainMenuMusic;
    [SerializeField] private TwoPartMusic gameplayMusic;

    private AudioSource audioSource;

    private TwoPartMusic current;
    private MusicType currentType = MusicType.None;
    private MusicState state = MusicState.Stopped;

    private bool canPlayMusic = true;

    // Pause state
    private float pausedTime;
    private AudioClip pausedClip;
    private bool wasLooping;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        Preload(mainMenuMusic);
        Preload(gameplayMusic);
        
        SetMusic(MusicType.MainMenu);
    }

    private void Update()
    {
        if (!canPlayMusic || current == null) return;

        switch (state)
        {
            case MusicState.Stopped:
                PlayIntro();
                break;

            case MusicState.PlayingIntro:
                // Only transition if intro truly finished
                if (!audioSource.isPlaying && state == MusicState.PlayingIntro)
                {
                    PlayLoop();
                }
                break;

            case MusicState.PlayingLoop:
                // Loop handles itself
                break;
        }
    }

    public void SetMusic(MusicType type)
    {
        if (currentType == type) return;

        currentType = type;

        switch (type)
        {
            case MusicType.MainMenu:
                PlayTwoPart(mainMenuMusic);
                break;

            case MusicType.Gameplay:
                PlayTwoPart(gameplayMusic);
                break;

            case MusicType.None:
                Stop();
                break;
        }
    }

    public void Pause()
    {
        if (!audioSource.isPlaying) return;

        pausedTime = audioSource.time;
        pausedClip = audioSource.clip;
        wasLooping = audioSource.loop;

        audioSource.Pause();
        canPlayMusic = false;
    }

    public void Resume()
    {
        if (pausedClip == null) return;

        audioSource.clip = pausedClip;
        audioSource.loop = wasLooping;
        audioSource.time = pausedTime;

        audioSource.Play();
        canPlayMusic = true;

        // Restore correct state
        state = wasLooping ? MusicState.PlayingLoop : MusicState.PlayingIntro;
    }

    public void Stop()
    {
        current = null;
        currentType = MusicType.None;
        canPlayMusic = false;

        audioSource.Stop();
        state = MusicState.Stopped;
    }

    private void PlayTwoPart(TwoPartMusic music)
    {
        if (current == music && canPlayMusic) return;

        current = music;
        canPlayMusic = true;

        audioSource.Stop();
        state = MusicState.Stopped;
    }

    private void PlayIntro()
    {
        if (current.Intro == null)
        {
            PlayLoop();
            return;
        }

        audioSource.loop = false;
        audioSource.clip = current.Intro;
        audioSource.Play();

        state = MusicState.PlayingIntro;
    }

    private void PlayLoop()
    {
        if (current.Loop == null) return;

        audioSource.loop = true;
        audioSource.clip = current.Loop;
        audioSource.Play();

        state = MusicState.PlayingLoop;
    }

    private void Preload(TwoPartMusic music)
    {
        if (music == null) return;

        if (music.Intro != null) music.Intro.LoadAudioData();
        if (music.Loop != null) music.Loop.LoadAudioData();
    }
}