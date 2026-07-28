using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    
    [Header("Transition")]
    [SerializeField] private GameObject transitionGO;
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private string transitionInState = "TransitionIn";
    [SerializeField] private InputActionReference cancelAction;
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Tab Settings Panels")]
    [SerializeField] private GameObject graphicsTab;
    [SerializeField] private GameObject audioTab;
    [SerializeField] private GameObject controlsTab;

    [Header("In-Game UI")]
    [SerializeField] private GameObject inGameUI;

    [Header("Reaction Settings")]
    [SerializeField] private float reactionDuration = 0.3f;

    [Header("Player 1 UI")]
    [SerializeField] private GameObject player1IdleGO;
    [SerializeField] private GameObject player1OpenGO;

    [Header("Player 2 UI")]
    [SerializeField] private GameObject player2IdleGO;
    [SerializeField] private GameObject player2OpenGO;

    private int _lastTabIndex = 0;
    private bool _isPaused;

    private Coroutine player1Coroutine;
    private Coroutine player2Coroutine;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        cancelAction.action.Enable();
        cancelAction.action.performed += OnBackButtonPressed;

        SubscribeToWorldManager();
    }

    

    private void OnDisable()
    {
        UnsubscribeFromWorldManager();
        
        cancelAction.action.performed -= OnBackButtonPressed;
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnBackButtonPressed(InputAction.CallbackContext obj)
    {
        if (_isPaused && settingsPanel != null && settingsPanel.activeSelf)
        {
            BackToPause();
        }
        else if(_isPaused && pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            TogglePause();
        }
        
    }
    private void Start()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        SetTab(0);

        SetIdleState(player1IdleGO, player1OpenGO);
        SetIdleState(player2IdleGO, player2OpenGO);

        PlayTransitionIn();
    }
    

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        _isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (inGameUI != null)
            inGameUI.SetActive(true);

        if(inGameUI.transform.parent != null && SceneManager.GetActiveScene().name != "Menu_Ui")
            inGameUI.transform.parent.gameObject.SetActive(true);

        SetIdleState(player1IdleGO, player1OpenGO);
        SetIdleState(player2IdleGO, player2OpenGO);

        UnsubscribeFromWorldManager();
        SubscribeToWorldManager();

        PlayTransitionIn();
    }

    private void PlayTransitionIn()
    {
        if (transitionGO == null || transitionAnimator == null)
            return;

        transitionGO.SetActive(true);

        transitionAnimator.Play(transitionInState, 0, 0f);
        transitionAnimator.Update(0f);

        StartCoroutine(DisableTransitionAfterAnim());
    }

    private IEnumerator DisableTransitionAfterAnim()
    {
        AnimatorStateInfo stateInfo;

        do
        {
            stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        while (stateInfo.normalizedTime < 1f || transitionAnimator.IsInTransition(0));

        transitionGO.SetActive(false);
    }

    private void SubscribeToWorldManager()
    {
        if (WorldManager.Instance == null) return;

        WorldManager.Instance.OnScoreChanged -= HandleScoreChanged;
        WorldManager.Instance.OnCoinCollected -= HandleCoinCollected;

        WorldManager.Instance.OnScoreChanged += HandleScoreChanged;
        WorldManager.Instance.OnCoinCollected += HandleCoinCollected;
    }

    private void UnsubscribeFromWorldManager()
    {
        if (WorldManager.Instance == null) return;

        WorldManager.Instance.OnScoreChanged -= HandleScoreChanged;
        WorldManager.Instance.OnCoinCollected -= HandleCoinCollected;
    }

    private void HandleScoreChanged(int playerID, int scoreAdded, int combinedScore)
    {
        if (playerID == 0)
        {
            RestartReaction(ref player1Coroutine, player1IdleGO, player1OpenGO);
        }
        else if (playerID == 1)
        {
            RestartReaction(ref player2Coroutine, player2IdleGO, player2OpenGO);
        }
    }

    private void HandleCoinCollected(int totalCoins)
    {
        RestartReaction(ref player1Coroutine, player1IdleGO, player1OpenGO);
        RestartReaction(ref player2Coroutine, player2IdleGO, player2OpenGO);
    }

    private void RestartReaction(ref Coroutine coroutineRef, GameObject idleGO, GameObject openGO)
    {
        if (idleGO == null || openGO == null)
            return;

        if (coroutineRef != null)
            StopCoroutine(coroutineRef);

        coroutineRef = StartCoroutine(PlayReaction(idleGO, openGO));
    }

    private IEnumerator PlayReaction(GameObject idleGO, GameObject openGO)
    {
        idleGO.SetActive(false);
        openGO.SetActive(true);

        yield return new WaitForSecondsRealtime(reactionDuration);

        openGO.SetActive(false);
        idleGO.SetActive(true);
    }

    private void SetIdleState(GameObject idleGO, GameObject openGO)
    {
        if (idleGO != null)
            idleGO.SetActive(true);

        if (openGO != null)
            openGO.SetActive(false);
    }

    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void TogglePause()
    {
        if (WorldManager.Instance != null && !WorldManager.Instance.CanPauseGame())
            return;

        _isPaused = !_isPaused;

        Time.timeScale = _isPaused ? 0f : 1f;
        
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.SetPlayersInputEnabled(!_isPaused);
        }

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(_isPaused);

        if (inGameUI != null)
            inGameUI.SetActive(!_isPaused);

        if (!_isPaused && settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        SetTab(_lastTabIndex);
    }

    public void BackToPause()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void SetTab(int index)
    {
        _lastTabIndex = index;

        if (graphicsTab != null) graphicsTab.SetActive(index == 0);
        if (audioTab != null) audioTab.SetActive(index == 1);
        if (controlsTab != null) controlsTab.SetActive(index == 2);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

   
    public void RegisterUI(GameObject pausePanel, GameObject settingsP, Image fade,
        GameObject gfxTab, GameObject audioT, GameObject ctrlTab,
        GameObject gameUI,
        GameObject p1Idle, GameObject p1Open,
        GameObject p2Idle, GameObject p2Open)
    {
        pauseMenuPanel = pausePanel;
        settingsPanel  = settingsP;
        graphicsTab    = gfxTab;
        audioTab       = audioT;
        controlsTab    = ctrlTab;
        inGameUI       = gameUI;
        player1IdleGO  = p1Idle;
        player1OpenGO  = p1Open;
        player2IdleGO  = p2Idle;
        player2OpenGO  = p2Open;

        // Remettre dans un état propre
        _isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null)  settingsPanel.SetActive(false);
        if (inGameUI != null)       inGameUI.SetActive(true);

        SetIdleState(player1IdleGO, player1OpenGO);
        SetIdleState(player2IdleGO, player2OpenGO);
        SetTab(0);
    }
}