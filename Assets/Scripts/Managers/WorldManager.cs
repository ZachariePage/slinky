using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;


public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    [Header("Intro / Scene Players")]
    [SerializeField] private bool useScenePlayersAtStart = true;
    [SerializeField] private string sceneSlinkyRootTag = "SceneSlinkyRoot";
    [SerializeField] private string toyBoxTag = "ToyBox";
    [SerializeField] private Animator toyBoxAnimator;
    [SerializeField] private float endCinematicUnlockDelay = 0;


    [Header("Scene References")]
    [SerializeField] private CameraPriorityManager cameraPriorityManager;

    [Header("Crown UI")]
    [SerializeField] private List<GameObject> crownsP1;
    [SerializeField] private List<GameObject> crownsP2;

    [Header("level sequence")] 
    [SerializeField] private string[] levelNames;
    private int currentLevel = 0;
    private bool introStarted = false;
    private bool introEnding = false;
    private bool introFinished = false;

    [Header("UI")]
    [SerializeField] private TMP_Text player1ScoreText;
    [SerializeField] private TMP_Text player2ScoreText;
    [SerializeField] private TMP_Text totalCoinsText;

    [Header("Room Coins")]
    [SerializeField] private int maxCoinsInRoom = 0;

    [SerializeField] private int scorePalier = 10;

    private GameObject[] _enemies;
    [SerializeField] private GameObject respawnLocation;
    [SerializeField] private GameObject playerControllerPrefab;
    [SerializeField] private GameObject slinkyParentGO;
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    [SerializeField] private List<GameObject> players = new List<GameObject>();
    public IReadOnlyList<GameObject> Players => players;
    [SerializeField] private GameObject inGameCanva;

    /// <summary>
    /// notify when score changed
    /// </summary>
    /// <param name="PlayerID"></param>
    /// <param name="Score"></param>
    ///  <param name="CombinedScore"></param>
    public event Action<int, int, int> OnScoreChanged;
    public event Action<int> OnCoinCollected;
    public event Action<int> OnCrownUpdate;
    public event Action OnGameEnd;
    public event Action OnWorldStart;
    public event Action OnPlayerDie;
    public event Action OnPlayerSpawn;


    public event Action OnPlayersDespawned;

    private Dictionary<int, int> playersScoreCount = new Dictionary<int, int>();
    
    private InputDevice[] savedPlayer1Devices;
    private InputDevice[] savedPlayer2Devices;
    private bool savedPlayer2Enabled = true;
    
    [SerializeField] private int combinedScore = 0;
    [SerializeField] private int totalCoinsCollected = 0;

    //cheats
    private int respawnIndex = 0;
    private GameObject[] respawnPoints;

    private int currentCrownOwner = -1;

    private int lastKillFrame = -1;
    [HideInInspector] public int coinAtTheEnd;
    [SerializeField] private GameCue[] onCrownChangeCues;

    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        respawnLocation = GameObject.FindWithTag("PlayerStart");
        
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        DontDestroyOnLoad(gameObject);

       
    }

    void Start()
    {
        
        respawnPoints = GameObject.FindGameObjectsWithTag("RespawnPoint");
        respawnIndex = 0;

        UpdateScoreUI();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow)) 
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                respawnIndex++;
                int newLoc = (respawnIndex % respawnPoints.Length + respawnPoints.Length) % respawnPoints.Length;
                SetRespawnLocation(respawnPoints[newLoc]);
                KillPlayer();
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow)) 
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                respawnIndex--;
                int newLoc = (respawnIndex % respawnPoints.Length + respawnPoints.Length) % respawnPoints.Length;
                SetRespawnLocation(respawnPoints[newLoc]);
                KillPlayer();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
    }

    private void RefreshSceneReferences()
    {
        cameraPriorityManager = FindFirstObjectByType<CameraPriorityManager>();

        if (cameraPriorityManager == null)
        {
            Debug.LogWarning("[WorldManager] No CameraPriorityManager found in loaded scene.");
        }
    }

    public bool IsIntroControlLocked()
    {
        return introStarted && !introFinished;
    }

    public void SetPlayersScoreScreenLock(bool locked)
    {
        SetPlayersGameplayEnabled(!locked);
    }

    private void StartIntroSequence()
    {
        if (introStarted) return;

        introStarted = true;
        introEnding = false;
        introFinished = false;

        SetPlayersGameplayEnabled(false);
        SetPlayersCinematic(true);

        if (toyBoxAnimator != null)
            toyBoxAnimator.SetBool("InCinematic", true);

        InputSystem.onAnyButtonPress.CallOnce(control =>
        {
            if (introEnding || introFinished) return;
            StartCoroutine(PlayIntroEnd());
        });
    }

    private IEnumerator PlayIntroEnd()
    {
        if (introEnding || introFinished)
            yield break;

        introEnding = true;

        // Make sure gameplay is blocked immediately
        SetPlayersGameplayEnabled(false);

        TriggerPlayersCinematicEnd();

        if (toyBoxAnimator != null)
            toyBoxAnimator.SetTrigger("EndCinematic");

        // Wait one frame so the trigger is actually consumed
        yield return null;

        // NOW the delay starts after EndCinematic began
        yield return new WaitForSeconds(endCinematicUnlockDelay);

        SetPlayersCinematic(false);

        if (toyBoxAnimator != null)
            toyBoxAnimator.SetBool("InCinematic", false);

        introEnding = false;
        introFinished = true;

        SetPlayersGameplayEnabled(true);
    }



    public bool CanPauseGame()
    {
        return introFinished && !introEnding;
    }
    
    
    private void SetPlayersGameplayEnabled(bool enabled)
    {
        foreach (GameObject p in players)
        {
            if (p == null) continue;

            SlinAndKyControllerBase controller = p.GetComponent<SlinAndKyControllerBase>();

            if (controller == null)
            {
                Debug.LogWarning($"[WorldManager] No SlinAndKyControllerBase found on {p.name}");
                continue;
            }

            controller.SetGameplayEnabled(enabled);
            
        }
    }

    private void SetPlayersCinematic(bool active)
    {
        foreach (GameObject p in players)
        {
            if (p == null) continue;

            SlinAndKyControllerBase controller = p.GetComponent<SlinAndKyControllerBase>();

            if (controller == null)
            {
                Debug.LogWarning($"[WorldManager] No SlinAndKyControllerBase found on {p.name}");
                continue;
            }

            controller.SetIntroCinematic(active);
            
        }
    }

    private void TriggerPlayersCinematicEnd()
    {
        foreach (GameObject p in players)
        {
            if (p == null) continue;

            SlinAndKyControllerBase controller = p.GetComponent<SlinAndKyControllerBase>();

            if (controller == null)
            {
                Debug.LogWarning($"[WorldManager] No SlinAndKyControllerBase found on {p.name}");
                continue;
            }

            controller.TriggerIntroCinematicEnd();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSceneReferences();

        SetSpawnPointDebug();
        GameObject canva = GameObject.Find("In-Game Canva");
        if (inGameCanva != null)
        {
            inGameCanva.SetActive(scene.name != "Menu_Ui");
            currentLevel = 0;
        }
            
        // Reset scene-only refs every time a new scene loads
        foreach (var p in players)
        {
            Destroy(p.gameObject);
        }
        respawnLocation = GameObject.FindWithTag("PlayerStart");
        slinkyParentGO = null;
        player1 = null;
        player2 = null;
        players.Clear();

        introStarted = false;
        introFinished = false; 
        useScenePlayersAtStart = true;
        
        ResetAllScores();

        GameObject canvaCheck = GameObject.Find("In-Game Canva");
       
        PlayerScore ps = FindFirstObjectByType<PlayerScore>();
       
        if (ps != null)
        {
            ps.SetUI(true);
            ps.SetEndLevelUI(false);
        }
        
        // Try intro path first
        if (useScenePlayersAtStart && !introFinished)
        {
            bool foundSceneSetup = FindSceneIntroSetup();

            if (foundSceneSetup)
            {
                bool scenePlayersOk = SetupScenePlayersFromRoot();

                if (scenePlayersOk)
                {
                    StartIntroSequence();
                    return; // IMPORTANT: stop here, no spawn
                }
            }
        }
        
        // Fallback: normal behaviour
        if (respawnLocation != null)
        {
            SpawnPlayers();
            OnWorldStart?.Invoke();
        }
    }

    public void ResetAllScores()
    {
        totalCoinsCollected = 0;
        combinedScore = 0;

        var keys = playersScoreCount.Keys.ToList();
        foreach (int key in keys)
        {
            playersScoreCount[key] = 0;
        }

        foreach (var p in players)
        {
            p.GetComponent<PlayerBrain>().SetCrown(false);
        }
        
        foreach (var c in crownsP1)
        {
            if (c != null)
            {
                c.SetActive(false);
            }
        }

        foreach (var c in crownsP2)
        {
            if (c != null)
            {
                c.SetActive(false);
            }
        }
        
        OnScoreChanged?.Invoke(0, 0, combinedScore);
        OnScoreChanged?.Invoke(0, 0, combinedScore);
    }
    
    void SpawnPlayers()
    {
        
        if (respawnLocation == null || playerControllerPrefab == null)
        {
            Debug.LogError("WorldManager: Missing respawnLocation or playerControllerPrefab.");
            return;
        }

        introFinished = true;
        useScenePlayersAtStart = false;

        slinkyParentGO = Instantiate(playerControllerPrefab, respawnLocation.transform.position, respawnLocation.transform.rotation);

        players.Clear();

        PlayerBrain[] found = slinkyParentGO.GetComponentsInChildren<PlayerBrain>(true);

        foreach (PlayerBrain p in found)
        {
            players.Add(p.gameObject);

            PlayerState ps = p.GetComponent<PlayerState>();
            if (ps == null)
                ps = p.gameObject.AddComponent<PlayerState>();

            ps.Init(p.GetPlayerId());
        }

        players = players
            .OrderBy(p => p.GetComponent<PlayerBrain>().GetPlayerId())
            .ToList();

        if (players.Count >= 2)
        {
            player1 = players[0];
            player2 = players[1];
        }

        if (currentCrownOwner != -1)
        {
            if (GetPlayerScore(currentCrownOwner) >= 10)
            {
                UpdateCrown(currentCrownOwner);
            }
        }
        RegisterPlayersToCamera();
        RestorePlayerDevices();
        OnPlayerSpawn?.Invoke();
        
    }
    
    
    //Function that makes sure the controls are correctly assign to the right players
    private void RestorePlayerDevices()
    {
        if (savedPlayer1Devices == null || savedPlayer2Devices == null)  return;
        
        PlayerInput pi1 = player1.GetComponent<PlayerInput>();
        PlayerInput pi2 = player2.GetComponent<PlayerInput>();

        if (pi1 != null && pi2 != null)
        {
            pi1.SwitchCurrentControlScheme(savedPlayer1Devices);
            pi2.SwitchCurrentControlScheme(savedPlayer2Devices);
        }
        
    }

    /// <summary>
    /// register player with his ID
    /// </summary>
    /// <param name="state"></param>
    public void RegisterPlayer(int ID)
    {
        if (!playersScoreCount.ContainsKey(ID))
        {
            playersScoreCount[ID] = 0;
        }
    }
    /// <summary>
    /// If players were set properly [0] is the head and [1] is the tail
    /// </summary>


    public void KillPlayer()
    {
        if (lastKillFrame == Time.frameCount)
        {
            Debug.Log("already spawned");
            return;
        }

        lastKillFrame = Time.frameCount;

        OnPlayerDie?.Invoke();

        if (cameraPriorityManager == null)
        {
            RefreshSceneReferences();
        }

        if (cameraPriorityManager != null)
        {
            cameraPriorityManager.ForceReturnToDefault();
        }

        OnPlayersDespawned?.Invoke();

        Transform root = players[0].transform.root.transform;
        Destroy(root.gameObject);

        foreach (var p in players)
        {
            Destroy(p.gameObject);
        }

        SpawnPlayers();
    }

    /// <summary>
    /// call to invoke the event 
    /// </summary>
    /// <param name="PlayerID"></param>
    /// <param name="Score"></param>
    public void NotifyScoreChanged(int PlayerID, int Score)
    {
        if (!playersScoreCount.ContainsKey(PlayerID))
        {
            Debug.LogError($"Player {PlayerID} not registered already, so registration wasn't called on awake, BUG");
            return;
        }
        
        playersScoreCount[PlayerID] += Score;
        combinedScore += Score;
        
        // if (playersScoreCount[PlayerID] >= scorePalier)
        // {
        //     UpdateCrown(PlayerID);
        // }
        
        if (GetPlayerScore(0) > GetPlayerScore(1))
        {
            UpdateCrown(0);
        }
        else if(GetPlayerScore(0) < GetPlayerScore(1))
        {
            UpdateCrown(1);
        }
        
        //UpdateScoreUI();
        OnScoreChanged?.Invoke(PlayerID, Score, combinedScore);
    }

    public int GetCurrentCrownOwner()
    {
        return currentCrownOwner;
    }

    public string GetNextSceneName()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (levelNames == null || levelNames.Length == 0)
            return string.Empty;

        for (int i = 0; i < levelNames.Length; i++)
        {
            if (levelNames[i] == currentScene)
            {
                if (i + 1 < levelNames.Length)
                    return levelNames[i + 1];

                return levelNames[0];
            }
        }

        return levelNames[0];
    }

    public void FinishEndScreenAndLoadNextScene()
    {
        Time.timeScale = 1f;

        ResetAllScores();
        currentCrownOwner = -1;

        string targetScene = GetNextSceneName();

        if (Application.CanStreamedLevelBeLoaded(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogError($"{targetScene} not found in build settings");
        }
    }
    
    /** Function to return to main menu, can be called from UI button */
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        ResetAllScores();
        currentCrownOwner = -1;
        coinAtTheEnd = 0;
        ScreenshotManager.Instance.ClearScreenshots();
        //OnGameEnd?.Invoke();
        SceneManager.LoadScene(0);
    }
    
    private void UpdateCrown(int ID)
    {
        if(GetPlayerScore(ID) < 10) return;
        int previousOwner = currentCrownOwner;
        bool isSameOwner = previousOwner == ID;

        if (isSameOwner)
            return; 

        currentCrownOwner = ID;
        //update crown
        GameObject player = players[ID];

        foreach (var p in players)
        {
            p.GetComponent<PlayerBrain>().SetCrown(false);
        }
        GameObject crown = player.GetComponent<PlayerBrain>().SetCrown(true);
        foreach (GameCue cue in onCrownChangeCues)
        {
            cue?.Execute(crown.transform.position);
        }

        // --- UI Crown update (multiple crowns) ---
        foreach (var c in crownsP1)
        {
            if (c != null)
            {
                c.SetActive(ID == 0);
                Animator crownAnim = c.GetComponent<Animator>();
                if (ID == 0 && crownAnim != null && currentCrownOwner != previousOwner)
                {
                    crownAnim.SetTrigger("CrownChange");
                }
            }
        }

        foreach (var c in crownsP2)
        {
            if (c != null)
            {
                c.SetActive(ID == 1);
                Animator crownAnim = c.GetComponent<Animator>();
                if (ID == 1 && crownAnim != null && currentCrownOwner != previousOwner)
                {
                    crownAnim.SetTrigger("CrownChange");
                }
            }
        }
        
        OnCrownUpdate?.Invoke(ID);
    }
    

    public int GetPlayerScore(int playerID)
    {
        return playersScoreCount[playerID];
    }

    public void AddCoins(int amount)
    {
        totalCoinsCollected += amount;
        //UpdateScoreUI();
        OnCoinCollected?.Invoke(totalCoinsCollected);
    }

    private void UpdateScoreUI()
    {
        if (player1ScoreText != null)
        {
            int player1Score = playersScoreCount.ContainsKey(0) ? playersScoreCount[0] : 0;
            player1ScoreText.text = player1Score.ToString();
        }

        if (player2ScoreText != null)
        {
            int player2Score = playersScoreCount.ContainsKey(1) ? playersScoreCount[1] : 0;
            player2ScoreText.text = player2Score.ToString();
        }

        if (totalCoinsText != null)
        {
            totalCoinsText.text = totalCoinsCollected.ToString();
        }
    }

    private bool FindSceneIntroSetup()
    {
        // Find placed player prefab root in scene
        GameObject rootByTag = GameObject.FindWithTag(sceneSlinkyRootTag);
        if (rootByTag != null)
        {
            slinkyParentGO = rootByTag;
        }
        else
        {
            // Fallback: find any object in scene that has at least 2 PlayerBrain children
            PlayerBrain[] allBrains = FindObjectsByType<PlayerBrain>(FindObjectsSortMode.None);
            foreach (PlayerBrain brain in allBrains)
            {
                Transform root = brain.transform.root;
                PlayerBrain[] brainsInRoot = root.GetComponentsInChildren<PlayerBrain>(true);

                if (brainsInRoot.Length >= 2)
                {
                    slinkyParentGO = root.gameObject;
                    break;
                }
            }
        }

        // Find toybox animator
        GameObject toyBoxGO = GameObject.FindWithTag(toyBoxTag);
        if (toyBoxGO != null)
        {
            toyBoxAnimator = toyBoxGO.GetComponent<Animator>();
        }
        else
        {
            // Fallback: search animator by object name contains "ToyBox"
            Animator[] animators = FindObjectsByType<Animator>(FindObjectsSortMode.None);
            foreach (Animator anim in animators)
            {
                if (anim.gameObject.name.ToLower().Contains("toybox"))
                {
                    toyBoxAnimator = anim;
                    break;
                }
            }
        }

        bool foundRoot = slinkyParentGO != null;
        bool foundToyBox = toyBoxAnimator != null;

       

        return foundRoot && foundToyBox;
    }

    private bool SetupScenePlayersFromRoot()
    {
        if (slinkyParentGO == null)
        {
            Debug.LogWarning("[WorldManager] No scene slinky root found.");
            return false;
        }

        players.Clear();

        PlayerBrain[] found = slinkyParentGO.GetComponentsInChildren<PlayerBrain>(true);

        if (found.Length < 2)
        {
            Debug.LogWarning("[WorldManager] Scene root does not contain 2 PlayerBrain components.");
            return false;
        }

        foreach (PlayerBrain p in found)
        {
            players.Add(p.gameObject);

            PlayerState ps = p.GetComponent<PlayerState>();
            if (ps == null)
                ps = p.gameObject.AddComponent<PlayerState>();

            ps.Init(p.GetPlayerId());
        }

        players = players
            .OrderBy(p => p.GetComponent<PlayerBrain>().GetPlayerId())
            .ToList();

        if (players.Count >= 2)
        {
            player1 = players[0];
            player2 = players[1];
        }

        RegisterPlayersToCamera();
        OnPlayerSpawn?.Invoke();
        ForceDeviceAssignment();
        SavePlayerDevices();
        return true;
    }

    private void SavePlayerDevices()
    {
        if (player1 != null)
        {
            PlayerInput pi1 = player1.GetComponent<PlayerInput>();
            if (pi1 != null)
                savedPlayer1Devices = pi1.devices.ToArray();
        }

        if (player2 != null)
        {
            PlayerInput pi2 = player2.GetComponent<PlayerInput>();
            if (pi2 != null)
            {
                savedPlayer2Enabled = pi2.enabled;
                savedPlayer2Devices = pi2.enabled ? pi2.devices.ToArray() : null;
                
            }
                
        }
    }

    private void RegisterPlayersToCamera()
    {
        GameObject targetGroup = GameObject.FindGameObjectWithTag("TargetGroup");
        if (targetGroup == null) return;

        CinemachineTargetGroup group = targetGroup.GetComponent<CinemachineTargetGroup>();
        if (group == null) return;

        // ✅ Clear existing targets safely
        group.Targets.Clear();

        if (player1 != null)
            group.AddMember(player1.transform, 0.5f, 1f);

        if (player2 != null)
            group.AddMember(player2.transform, 0.5f, 1f);
    }

    public int GetTotalCoins()
    {
        return totalCoinsCollected;
    }
    public void SetRespawnLocation(GameObject newRespawnLocation)
    {
        respawnLocation = newRespawnLocation;
    }
    public void RegisterUI(WorldUIBinding.UIRole role, TMP_Text text)
    {
        switch (role)
        {
            case WorldUIBinding.UIRole.Player1Score: player1ScoreText = text; break;
            case WorldUIBinding.UIRole.Player2Score: player2ScoreText = text; break;
            case WorldUIBinding.UIRole.TotalCoins:   totalCoinsText = text;   break;
        }
        UpdateScoreUI();
    }

    public void RequestEndLevel()
    {
        EndLevel();
    }

    private void EndLevel()
    {
        Time.timeScale = 0f;
        OnGameEnd?.Invoke();
       
    }
    

    void SetSpawnPointDebug()
    {
        respawnPoints = GameObject.FindGameObjectsWithTag("RespawnPoint");
        respawnIndex = 0;
    }

    public void StunPlayers(bool value)
    {
        foreach (var p in players)
        {
            if (value)
            {
                p.GetComponent<SlinAndKyControllerBase>().CantMove();
            }
            else
            {
                p.GetComponent<SlinAndKyControllerBase>().CanMove();
            }
        }
    }
    
    //Force player to be assign to right controls the first setup
    private void ForceDeviceAssignment()
    {
        PlayerInput pi1 = player1?.GetComponent<PlayerInput>();
        PlayerInput pi2 = player2?.GetComponent<PlayerInput>();
        if (pi1 == null || pi2 == null) return;

        var gamepads = Gamepad.all;

        if (gamepads.Count >= 2)
        {
            // Two controllers : first → Sly, second → Ky
            pi1.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
            pi2.SwitchCurrentControlScheme("Gamepad", gamepads[1]);
        }
        else if (gamepads.Count == 1)
        {
            // 1 Contoller → Sly, Keyboard → Ky
            pi1.SwitchCurrentControlScheme("Gamepad", gamepads[0]);
            pi2.SwitchCurrentControlScheme("Keyboard", Keyboard.current);
        }
        else
        {
            // No controller
            pi1.SwitchCurrentControlScheme("Keyboard", Keyboard.current);
            pi2.enabled = false;
        }
    }
    //public fonction to enable/disable the PlayerInput
    public void SetPlayersInputEnabled(bool enabled)
    {
        foreach (GameObject p in players)
        {
            if (p == null) continue;
            PlayerInput pi = p.GetComponent<PlayerInput>();
            if (pi == null) continue;

            if (enabled && p == player2 && !savedPlayer2Enabled)
                continue; // if player 2 was originally disabled, don't re-enable him

            pi.enabled = enabled;
        }
    
        if (enabled)
            RestorePlayerDevices();
    }
}