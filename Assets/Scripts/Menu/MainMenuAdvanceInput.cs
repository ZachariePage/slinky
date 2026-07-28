using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuAdvanceInput : MonoBehaviour
{
    [SerializeField] private InputActionReference submitButton;

    [Header("References")]
    [SerializeField]
    private Animator animator;

    [Header("Animator State Names")]
    [SerializeField]
    private string cameraIdleState = "Camera_Idle";

    [SerializeField] private string splashIdleState = "SplashIdle";
    [SerializeField] private string menuIdleState = "MenuIdle";

    [Header("Animator Triggers")]
    [SerializeField]
    private string startFromCameraTrigger = "StartFromCamera";

    [SerializeField] private string startFromSplashTrigger = "StartFromSplash";
    [SerializeField] private string playGameTrigger = "PlayGame";

    [Header("Scene Loading")]
    [SerializeField]
    private int sceneToLoad = 1;

    [Header("Input Lock")]
    [SerializeField]
    private float inputCooldown = 0.2f;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private CanvasGroup videoCanvasGroup;

    private float nextAllowedInputTime;
    private bool isLoadingScene = false;
    private bool isPlayingIntroVideo = false;
    private bool isStartingGame = false;

    [SerializeField] MusicStarter musicStarter;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        submitButton.action.Enable();
        submitButton.action.performed += OnSubmitButtonPressed;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Prepare();
        }
    }

    void OnDisable()
    {

        submitButton.action.performed -= OnSubmitButtonPressed;
        submitButton.action.Disable();

        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnSubmitButtonPressed(InputAction.CallbackContext context)
    {

        AudioClip hitClip = Resources.Load<AudioClip>("hit");

        /*SoundManager.Instance.PlaySFX(hitClip, transform.position);*/

        if (animator == null || isLoadingScene)
        {
            return;
        }

        if (Time.unscaledTime < nextAllowedInputTime)
        {
            return;
        }

        if (isPlayingIntroVideo)
        {
            SkipVideoAndStartGame();
            LockInput();
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(cameraIdleState))
        {
            animator.ResetTrigger(startFromSplashTrigger);
            animator.ResetTrigger(playGameTrigger);
            animator.SetTrigger(startFromCameraTrigger);
            LockInput();

            // Start music
            if(musicStarter)
                musicStarter.SelectMusic();
        }
        else if (stateInfo.IsName(splashIdleState))
        {
            animator.ResetTrigger(startFromCameraTrigger);
            animator.ResetTrigger(playGameTrigger);
            animator.SetTrigger(startFromSplashTrigger);
            LockInput();
        }
    }

    private void LockInput()
    {
        nextAllowedInputTime = Time.unscaledTime + inputCooldown;
    }

    public void OnPlayButtonPressed()
    {
        if (animator == null || isLoadingScene || isStartingGame)
            return;

        if (Time.unscaledTime < nextAllowedInputTime)
            return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(menuIdleState))
        {
            StartIntroVideo();
            LockInput();
        }
    }

    private void StartIntroVideo()
    {
        if (isPlayingIntroVideo || isStartingGame)
            return;
        
        
        if (videoPlayer != null)
        {
            isPlayingIntroVideo = true;
            videoPlayer.time = 0;
            videoPlayer.Play();
        }
        
        if (videoCanvasGroup != null)
        {
            videoCanvasGroup.alpha = 1f;
            videoCanvasGroup.blocksRaycasts = true;
        }
        
        else
        {
            StartGameTransition();
        }
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        if (!isPlayingIntroVideo)
            return;

        StartGameTransition();
    }

    private void SkipVideoAndStartGame()
    {
        if (!isPlayingIntroVideo)
            return;

        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        StartGameTransition();
    }

    private void StartGameTransition()
    {
        if (isStartingGame || isLoadingScene)
            return;

        isPlayingIntroVideo = false;
        isStartingGame = true;

        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        if (videoCanvasGroup != null)
        {
            videoCanvasGroup.alpha = 0f;
            videoCanvasGroup.blocksRaycasts = false;
        }

        animator.ResetTrigger(startFromCameraTrigger);
        animator.ResetTrigger(startFromSplashTrigger);
        animator.SetTrigger(playGameTrigger);

        LockInput();
    }

    public void SetSceneToLoad(int newSceneIndex)
    {
        if (SceneFlow.Instance != null && SceneFlow.Instance.CanUseIndex(newSceneIndex))
        {
            sceneToLoad = newSceneIndex;
            SceneFlow.Instance.PreloadScene(newSceneIndex);
        }
    }

    public void LoadChosenScene()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;
        isPlayingIntroVideo = false;
        isStartingGame = false;
        
        if (SceneFlow.Instance != null && sceneToLoad >= 0)
            SceneFlow.Instance.ActivatePreloadedOrLoad(sceneToLoad);
        else
            SceneManager.LoadScene(1);

        animator.ResetTrigger(startFromCameraTrigger);
        animator.ResetTrigger(startFromSplashTrigger);
        animator.ResetTrigger(playGameTrigger);
    }

    public void OnQuitButtonPressed()
    {
        Debug.Log("Quit requested.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}