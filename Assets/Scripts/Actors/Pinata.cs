using System;
using System.Collections.Generic;
using UnityEngine;

public class Pinata : MonoBehaviour, IChompable
{
    public int requiredPlayers = 2;

    [Tooltip("A player can pull around 200, why? no idea")]
    public float RequiredPowerToBreak = 300;
    [Tooltip("seconds of pulling to destroy")]
    public float Durability = 5;

    public bool allowsAttachment = true;
    [SerializeField] private ChompableSpringValue springValue;
    
    public List<GameObject> Players = new List<GameObject>();

    [Header("Retroaction")]
    [SerializeField] protected GameCue[] OnChompCues;
    [SerializeField] protected GameCue[] OnReleaseCues;
    [SerializeField] protected GameCue[] OnCompletionCues;
    
    [Header("Chomp")]
    [SerializeField] private SpringAttachmentConfig chompConfig;
    public AttachmentType attachmentType = AttachmentType.Spring;

    private void Start()
    {
        allowsAttachment = true;
    }

    private void Update()
    {
        if (CheckBreakCondition())
        {
            Durability -= Time.deltaTime;
            if (Durability <= 0) BreakPinata();
        }
    }

    public void OnChomped(GameObject chomper)
    {
        if(!Players.Contains(chomper)) 
        {
            Players.Add(chomper);
        }
        
        foreach (GameCue c in OnChompCues)
        {
            c?.Execute(transform.position);
        }
    }

    public void OnReleased(GameObject chomper)
    {
        if (Players.Contains(chomper))
        {
            Players.Remove(chomper);
        }
        
        foreach (GameCue c in OnReleaseCues)
        {
            c?.Execute(transform.position);
        }
    }
    
    public bool CheckBreakCondition()
    {
        if(Players.Count < requiredPlayers) return false;
        
        float totalForce = 0f;

        foreach (var player in Players)
        {
            PlayerChomp chomper = player.GetComponent<PlayerChomp>();
            if (chomper == null) continue;

            SpringJoint joint = chomper.chompJoint as SpringJoint;
            if (joint == null) continue;

            totalForce += joint.currentForce.magnitude;
        }

        return totalForce >= RequiredPowerToBreak;
    }


    private void BreakPinata()
    {
        List<GameObject> playersCopy = new List<GameObject>(Players);

        foreach (GameObject player in playersCopy)
        {
            PlayerChomp chomper = player.GetComponent<PlayerChomp>();
            chomper.RemoveJoint();
        }
        
        foreach (GameCue c in OnCompletionCues)
        {
            c?.Execute(transform.position);
        }

        CollectibleFactory.Instance.SpawnCollectiblesInBurst("BonBonWithRB", transform.position, 10);
        Destroy(gameObject);
    }

    public bool AllowsAttachment()
    {
        return allowsAttachment;
    }

    public AttachmentType GetAttachmentType()
    {
        return attachmentType;
    }

    public ChompableSpringValue GetSpringValue()
    {
        return springValue;
    }
    public void OnSetupFinish(GameObject chomper)
    {
        
    }
    public AttachmentLocation GetAttachmentLocation()
    {
        return AttachmentLocation.location1;
    }
    
    public AttachmentConfig GetAttachmentConfig()
    {
        return chompConfig;
    }
}

