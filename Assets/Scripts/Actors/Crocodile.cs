using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crocodile : MonoBehaviour
{
    [SerializeField] private float catchDelay = 2;
    [SerializeField] private float radius;
    [SerializeField] private float force;
    [SerializeField] private LayerMask mask;
    [SerializeField] private Transform overlapLocation;
    
    [SerializeField] private Vector3 localDirection = Vector3.forward;
    [SerializeField] private Collider[] colliderToDisable;

    [SerializeField] private GameCue[] catchCues;
    [SerializeField] private GameCue[] releaseCues;

    [SerializeField] private float chompSpeed = 1;

    private float timer = 1;
    private bool activated;
    private Coroutine coroutine;
    void Start()
    {
        
    }

    private void Update()
    {
        if (timer >= 0)
        {
            timer -= Time.deltaTime;
        }
        else if (activated && timer <= 0)
        {
            EnableCollisions();
        }
    }


    void PlayerCue(GameCue[] cuesList, Transform location)
    {
        foreach (GameCue cue in cuesList)
        {
            cue?.Execute(location.position);
        }
    }

    public void ThrowPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(overlapLocation.position, radius,mask);
        
        foreach (Collider col in hitColliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null && coroutine == null)
            {
                coroutine = StartCoroutine(CatchPlayer(col, catchDelay));
            }
        }
    }

    private void DisableCollisions()
    {
        foreach (Collider myCol in colliderToDisable)
        {
            Debug.Log(myCol.gameObject.name);
           myCol.enabled = false;
        }

        timer = 1;
        activated = true;
    }
    
    private void EnableCollisions()
    {
        foreach (Collider myCol in colliderToDisable)
        {
            myCol.enabled = true;
        }
    }

    IEnumerator CatchPlayer(Collider other, float delay)
    {
        GetComponent<Animator>().speed = 0;
        PlayerCue(catchCues, transform);

        yield return new WaitForSeconds(delay);
        Rigidbody rb = other.GetComponent<Rigidbody>();
        
        GetComponent<Animator>().speed = chompSpeed;
        if (other != null)
            DisableCollisions();


        yield return new WaitForSeconds(.5f);

        if (other != null)
        {
            PlayerCue(releaseCues, transform); 

            Vector3 beltDir = transform.TransformDirection(localDirection).normalized;
            rb.AddForce(beltDir * force, ForceMode.Impulse);
            Debug.Log(other.gameObject.name);
        }

        coroutine = null;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if (overlapLocation != null)
        {
            Gizmos.DrawWireSphere(overlapLocation.position, radius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        
    }
}

/*
    [SerializeField] private float stunDuration = 3;
    [SerializeField] private float coldownDuration = 5;
    private Dictionary<GameObject, float> cooldowns = new Dictionary<GameObject, float>();
    [Tooltip("If player is velocity is set to 0 on stun")]
    [SerializeField] private bool stopPlayer = true;

    [Header("Retroactions")]
    [SerializeField] protected GameCue[] stunPlayer;
    [SerializeField] protected GameCue[] hitPlayerButCantStun;
    */
/*
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
            PlayerCue(hitPlayerButCantStun, player.transform);
            return;
        }

        if (player.gameObject.tag == "Player")
        {
            SlinAndKyControllerBase controller = player.GetComponent<SlinAndKyControllerBase>();
            controller.StunPlayer(stunDuration);
        }

        Unit enemy = player.GetComponent<Unit>();
        if (enemy != null)
        {
            enemy.SetStun(stunDuration);
        }

        if (stopPlayer)
        {
            player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }

        PlayerCue(stunPlayer, player.transform);
        cooldowns[player] = coldownDuration;
    }
    */