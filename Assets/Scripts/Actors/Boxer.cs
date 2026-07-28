using System.Collections.Generic;
using UnityEngine;

public class Boxer : MonoBehaviour
{
    [SerializeField] private float force = 3;
    [SerializeField] private float forceEnemy = 3;
    [SerializeField] private float coldownDuration = 5;
    [SerializeField] private float stunDuration = 0.2f;
    private Dictionary<GameObject, float> cooldowns = new Dictionary<GameObject, float>();
    
    [Header("Retroactions")] 
    [SerializeField] protected GameCue[] hitPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (cooldowns.Count > 0)
        {
            var keys = new List<GameObject>(cooldowns.Keys);
            foreach (GameObject go in keys)
            {
                cooldowns[go] -= Time.deltaTime;
                if (cooldowns[go] <= 0f)
                {
                    cooldowns.Remove(go);
                }
            }
        } 
    }
    
    public void OnPlayerHit(GameObject player)
    {
        if (cooldowns.ContainsKey(player))
        {
            return;
        }

        if (player.gameObject.tag == "Player")
        {
            SlinAndKyControllerBase controller = player.GetComponent<SlinAndKyControllerBase>();
            if(controller == null) return;
        
            player.GetComponent<Rigidbody>().AddForce(transform.forward * force, ForceMode.Impulse);
            controller.StunPlayer(stunDuration);
        }

        Unit enemy = player.GetComponent<Unit>();
        if (enemy != null)
        {
            enemy.AddForce(transform.forward * forceEnemy,  ForceMode.Impulse);
        }
        
        cooldowns[player] = coldownDuration;
        PlayerCue(hitPlayer, player.transform);
    }
    
    void PlayerCue(GameCue[] cuesList, Transform location)
    {
        foreach (GameCue cue in cuesList)
        {
            cue?.Execute(location.position);
        }
    }
}
