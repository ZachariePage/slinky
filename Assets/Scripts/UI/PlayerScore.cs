 using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerScore : MonoBehaviour
{
    float combinedScorePlayer;

    [SerializeField] private TextMeshProUGUI combinedUI;
    [SerializeField] private TextMeshProUGUI[] playerUIs;
    [SerializeField] private float player1palier;
    [SerializeField] private float player2palier;

    [Header("UI")]
    [SerializeField] private TMP_Text player1ScoreText;
    [SerializeField] private TMP_Text player2ScoreText;
    [SerializeField] private TMP_Text CombinedScoreText;
    [SerializeField] private TMP_Text totalCoinsText;

    private bool UIDisabled = false;
    [SerializeField] private GameObject[] UItoDisable;
    [SerializeField] private GameObject EndLevelUI;
    [SerializeField] private TMP_Text player1ScoreTextEnd;
    [SerializeField] private TMP_Text player2ScoreTextEnd;
    [SerializeField] private TMP_Text CombinedScoreTextEnd;
    [SerializeField] private TMP_Text totalCoinsTextEnd;

    [Header("End Score Animator")]
    [SerializeField] private GameObject endScoreCanvas;
    [SerializeField] private Animator endScoreAnimator;

    [Header("Input")]
    [SerializeField] private InputActionReference submitButton;
    [SerializeField] private float inputCooldown = 0.2f;

    [Header("Animator State Names")]
    [SerializeField] private string endScoreState = "EndScore";
    [SerializeField] private string winnerSlyIdleState = "WinnerSlyIdle";
    [SerializeField] private string winnerKyIdleState = "WinnerKyIdle";
    [SerializeField] private string pictureEndIdleState = "PictureEndIdlee";
    [SerializeField] private string pictureCloseState = "PictureClose";

    [Header("Animator Parameters")]
    [SerializeField] private string winnerIdParameter = "WinnerID";
    [SerializeField] private string showScoreTrigger = "ShowScore";
    [SerializeField] private string closeScoreTrigger = "CloseScore";

    private bool endSequenceActive = false;
    private bool canAdvanceToScore = false;
    private bool canAdvanceToClose = false;
    private bool closeStarted = false;
    private int winnerId = -1;
    private float nextAllowedInputTime = 0f;
    private int nextBuildIndexToActivate = -1;

    void Start()
    {
        WorldManager.Instance.OnScoreChanged += UpdateScore;
        WorldManager.Instance.OnCoinCollected += UpdateCoinsUI;
        WorldManager.Instance.OnCrownUpdate += UpdatePalier;
        WorldManager.Instance.OnGameEnd += EndGame;
    }

    void OnEnable()
    {
        if (submitButton != null)
        {
            submitButton.action.Enable();
            submitButton.action.performed += OnSubmitButtonPressed;
        }
    }

    void OnDisable()
    {
        if (submitButton != null)
        {
            submitButton.action.performed -= OnSubmitButtonPressed;
            
        }
    }

    void Update()
    {
        if (!endSequenceActive || endScoreAnimator == null)
            return;

        AnimatorStateInfo stateInfo = endScoreAnimator.GetCurrentAnimatorStateInfo(0);
        bool inTransition = endScoreAnimator.IsInTransition(0);

        if (!inTransition)
        {
            if (!canAdvanceToScore &&
                (stateInfo.IsName(winnerSlyIdleState) || stateInfo.IsName(winnerKyIdleState)))
            {
                canAdvanceToScore = true;
            }

            if (!canAdvanceToClose &&
                stateInfo.IsName(pictureEndIdleState))
            {
                canAdvanceToClose = true;
            }

            if (closeStarted &&
                stateInfo.IsName(pictureCloseState) &&
                stateInfo.normalizedTime >= 1f)
            {
                closeStarted = false;
                endSequenceActive = false;
                canAdvanceToScore = false;
                canAdvanceToClose = false;

                player1palier = 0;
                player2palier = 0;

                Time.timeScale = 1f;

                ScreenshotManager.Instance.ClearScreenshots();
                if (SceneFlow.Instance != null && nextBuildIndexToActivate >= 0)
                    SceneFlow.Instance.ActivatePreloadedOrLoad(nextBuildIndexToActivate);
                else
                    WorldManager.Instance.FinishEndScreenAndLoadNextScene();
            }
        }
    }

    private void OnSubmitButtonPressed(InputAction.CallbackContext context)
    {
        if (!endSequenceActive || endScoreAnimator == null)
            return;

        if (Time.unscaledTime < nextAllowedInputTime)
            return;

        AnimatorStateInfo stateInfo = endScoreAnimator.GetCurrentAnimatorStateInfo(0);

        if (canAdvanceToScore &&
            (stateInfo.IsName(winnerSlyIdleState) || stateInfo.IsName(winnerKyIdleState)))
        {
            canAdvanceToScore = false;
            endScoreAnimator.ResetTrigger(closeScoreTrigger);
            endScoreAnimator.SetTrigger(showScoreTrigger);
            LockInput();
            return;
        }

        if (canAdvanceToClose &&
            stateInfo.IsName(pictureEndIdleState))
        {
            canAdvanceToClose = false;
            closeStarted = true;
            endScoreAnimator.ResetTrigger(showScoreTrigger);
            endScoreAnimator.SetTrigger(closeScoreTrigger);
            LockInput();
        }
    }

    private void LockInput()
    {
        nextAllowedInputTime = Time.unscaledTime + inputCooldown;
    }

    void UpdateScore(int PlayerID, int score, int combinedScore)
    {
        if (UIDisabled) return;

        int playerScore0 = WorldManager.Instance.GetPlayerScore(0);
        int playerScore1 = WorldManager.Instance.GetPlayerScore(1);
        combinedScorePlayer = combinedScore;

        player1ScoreText.text = playerScore0.ToString();
        player2ScoreText.text = playerScore1.ToString();
    }

    void UpdateCoinsUI(int coins)
    {
        if (UIDisabled) return;
        totalCoinsText.text = coins.ToString();
    }

    void UpdatePalier(int playerID)
    {
        if (UIDisabled) return;

        if (playerID == 0)
            player1palier++;

        if (playerID == 1)
            player2palier++;
    }

    public void SetUI(bool value)
    {
        UIDisabled = !value;

        foreach (GameObject go in UItoDisable)
        {
            go.SetActive(value);
        }
    }

    public void SetEndLevelUI(bool value)
    {
        EndLevelUI.SetActive(value);
    }

    public void EndGame()
    {
        EndLevelUI.SetActive(true);

        //int kyScore = (int)player1palier * 10 + WorldManager.Instance.GetPlayerScore(0);
        //int slyScore = (int)player2palier * 10 + WorldManager.Instance.GetPlayerScore(1);

        int kyScore = WorldManager.Instance.GetPlayerScore(0);
        int slyScore = WorldManager.Instance.GetPlayerScore(1);
        int Combined = slyScore + kyScore;

        if (player1ScoreTextEnd != null)
            player1ScoreTextEnd.text = kyScore.ToString();

        if (player2ScoreTextEnd != null)
            player2ScoreTextEnd.text = slyScore.ToString();

        if (CombinedScoreTextEnd != null)
            CombinedScoreTextEnd.text = Combined.ToString();

        if (totalCoinsTextEnd != null)
            totalCoinsTextEnd.text = WorldManager.Instance.coinAtTheEnd.ToString();

        SetUI(false);

        winnerId = WorldManager.Instance.GetCurrentCrownOwner();

        if (winnerId != 0 && winnerId != 1)
        {
            winnerId = kyScore >= slyScore ? 0 : 1;
        }

        endSequenceActive = true;
        canAdvanceToScore = false;
        canAdvanceToClose = false;
        closeStarted = false;

        WorldManager.Instance.SetPlayersScoreScreenLock(true);

        if (endScoreCanvas != null)
            endScoreCanvas.SetActive(true);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (endScoreAnimator != null)
        {
            endScoreAnimator.ResetTrigger(showScoreTrigger);
            endScoreAnimator.ResetTrigger(closeScoreTrigger);
            endScoreAnimator.SetInteger(winnerIdParameter, winnerId);
            endScoreAnimator.Play(endScoreState, 0, 0f);
            endScoreAnimator.Update(0f);
        }
        
        if (SceneFlow.Instance != null)
        {
            nextBuildIndexToActivate = SceneFlow.Instance.GetNextPlayableBuildIndex();
            SceneFlow.Instance.PreloadScene(nextBuildIndexToActivate);
        }

        LockInput();
    }
}