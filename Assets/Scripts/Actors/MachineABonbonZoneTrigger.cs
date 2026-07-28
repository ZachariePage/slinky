using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MachineABonbonZoneTrigger : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CameraPriorityManager cameraManager;
    [SerializeField] private CinemachineCamera cameraWhenPlayersAreFrozen;
    [SerializeField] private CinemachineCamera cameraBeforePlayersGetControlBack;

    [SerializeField] private GameObject[] objectToEnable;
    [SerializeField] private int secondsPerCoins = 5;
    [SerializeField] private int seconds = 5;
    [SerializeField] private Transform vfxLocation;
    [SerializeField] private float radius = 3f;
    [SerializeField] private float maxDelayBetweenCues = 0.2f;
    [SerializeField] private float minDelayBetweenCues = 2f;
    [Range(1,3)]
    [SerializeField] private int instanceRunningAtSameTime = 1;
    [SerializeField] protected GameCue[] OnTriggeredCue;
    [SerializeField] protected GameCue[] OnCountDownStartCues;
    
    [SerializeField] private GameObject machineABonBon;
    [SerializeField] private GameObject coins;
    [SerializeField] private Transform coinsSpawnLocation;

    [SerializeField] private float safetyDuration = 5f;
    private bool triggered = false;


    [SerializeField] protected GameCue[] coinCues;


    private float totalTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (vfxLocation == null)
        {
            vfxLocation = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!triggered)
            {
                foreach (GameCue cue in OnTriggeredCue)
                {
                    cue?.Execute(vfxLocation.position);
                }
                StartCoroutine(PutCoinsInMachine());
            }
            triggered = true;
        }
    }

    int CalculateTime()
    {
        int totalCoins = WorldManager.Instance.GetTotalCoins();
        return seconds + (totalCoins * secondsPerCoins);
    }

    IEnumerator PutCoinsInMachine()
    {
        // calculate time for the bonbon ending
        totalTime = CalculateTime();
        int totalCoins = WorldManager.Instance.GetTotalCoins();
        WorldManager.Instance.coinAtTheEnd = WorldManager.Instance.GetTotalCoins();
        //WorldManager.Instance.StunPlayers(true);

        if (cameraManager != null && cameraWhenPlayersAreFrozen != null)
        {
            cameraManager.MakeLive(cameraWhenPlayersAreFrozen);
        }

        yield return new WaitForSeconds(1.5f);

        //put coins inside machine
        for (int i = 0; i < totalCoins; i++)
        {
            //WorldManager.Instance.AddCoins(-1);
            GameObject coin = Instantiate(coins, coinsSpawnLocation.position, coinsSpawnLocation.rotation);


            yield return new WaitForSeconds(1.45f);

            foreach (GameCue cue in coinCues)
            {
                cue?.Execute(coin.transform.position);
            }

            Destroy(coin);
        }

        //bonbon should play its animation
        //this abomination could be so much better done but no tiem
        Animator machineAnim = machineABonBon.GetComponent<Animator>();
        machineAnim.enabled = true;
        
        float safety = 0f;
        
        while (!machineAnim.GetCurrentAnimatorStateInfo(0).IsName("Gumball_Cinematic") && safety < safetyDuration)
        {
            safety += Time.deltaTime;
            yield return null;
        }

        safety = 0f;
        while (machineAnim.GetCurrentAnimatorStateInfo(0).IsName("Gumball_Cinematic") && safety < safetyDuration)
        {
            safety += Time.deltaTime;
            yield return null;
        }
        //normal abomination continue below

        if (cameraManager != null && cameraBeforePlayersGetControlBack != null)
        {
            cameraManager.MakeLive(cameraBeforePlayersGetControlBack);
        }

        yield return new WaitForSeconds(1.5f);
        //WorldManager.Instance.StunPlayers(false);
        //end level countdown
        CountdownTimer.StartCountdown((int)totalTime, EndLevel);
        foreach (GameObject enable in objectToEnable)
        {
            enable.SetActive(true);
        }
        StartCues();
        
        yield return null;
    }

    public void StartCues()
    {
        for (int i = 0; i < instanceRunningAtSameTime; i++)
        {
            StartCoroutine(CueRoutine());
        }
    }
    
    IEnumerator CueRoutine()
    {
        float totalDuration = totalTime;
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            foreach (GameCue cue in OnCountDownStartCues)
            {
                Vector2 random2D = UnityEngine.Random.insideUnitCircle * radius;

                Vector3 spawnPos = new Vector3(
                    vfxLocation.position.x + random2D.x,
                    vfxLocation.position.y,
                    vfxLocation.position.z + random2D.y
                );

                cue?.Execute(spawnPos);
            }

            float delay = UnityEngine.Random.Range(minDelayBetweenCues, maxDelayBetweenCues);
            
            if (elapsed + delay > totalDuration)
            {
                delay = totalDuration - elapsed;
            }

            elapsed += delay;

            yield return new WaitForSeconds(delay);
        }
    }
    void EndLevel()
    {
        CollectibleFactory.Instance.StopAllCoroutines();
        StopAllCoroutines();
        WorldManager.Instance.RequestEndLevel();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (vfxLocation != null)
        {
            Gizmos.DrawWireSphere(vfxLocation.position, radius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        
    }
}
