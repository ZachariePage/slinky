using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BlocLego : MonoBehaviour, IChompable
{
    private Rigidbody _rigidbody;
    
    private bool createJointOnChomp = true;
    [SerializeField] private ChompableSpringValue springValue;
    
    [Tooltip("Si le truc bouge ou non")]
    private bool IsKinematic;
    public int requiredPlayers = 1;

    [Header("Retroaction")]
    [SerializeField] protected GameCue[] OnChompCues;
    [SerializeField] protected GameCue[] OnReleaseCues;
    private Rigidbody rb;
    
    private List<PlayerChomp> players = new List<PlayerChomp>();
    
    [Header("Chomp")]
    [SerializeField] private AttachmentConfig notEnoughPlayersConfig;
    [SerializeField] private AttachmentConfig enoughPlayersConfig; 
    public AttachmentType attachmentType = AttachmentType.ChildTransform;

    private bool CanPlayerMoveItAlone = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createJointOnChomp = true;
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            IsKinematic = rb.isKinematic;
        }

        if (requiredPlayers > 1)
        {
            CanPlayerMoveItAlone = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnChomped(GameObject chomper)
    {
        if (rb == null) return;

        if (!players.Contains(chomper.GetComponent<PlayerChomp>()))
            players.Add(chomper.GetComponent<PlayerChomp>());

        rb.isKinematic = false;
        
        if (players.Count == requiredPlayers && !CanPlayerMoveItAlone)
        {
            ActivateJointForAll();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (CanPlayerMoveItAlone)
        {
            GetComponent<Collider>().isTrigger = true;
        }

        foreach (GameCue c in OnChompCues)
        {
            c?.Execute(transform.position);
        }
    }
    
    void ActivateJointForAll()
    {
        foreach (PlayerChomp p in players)
        {
            p.SetCantMove(false);
            p.AttachJointTo(rb, springValue);
        }
    }

    public void OnReleased(GameObject chomper)
    {
        PlayerChomp pc = chomper.GetComponent<PlayerChomp>();

        if (players.Contains(pc))
            players.Remove(pc);

        rb.isKinematic = IsKinematic;
        rb.constraints = RigidbodyConstraints.FreezePositionX 
                         | RigidbodyConstraints.FreezePositionZ 
                         | RigidbodyConstraints.FreezeRotation;
        
        GetComponent<Collider>().isTrigger = false;
        foreach (GameCue c in OnReleaseCues)
        {
            c?.Execute(transform.position);
        }
    }

    public bool AllowsAttachment()
    {
        return createJointOnChomp;
    }

    public AttachmentType GetAttachmentType()
    {
        if (players.Count < requiredPlayers)
        {
            return AttachmentType.CantMove;
        }
        else
        {
            return attachmentType;
        }
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
        if (players.Count < requiredPlayers)
        {
            return notEnoughPlayersConfig;
        }
        else
        {
            return enoughPlayersConfig;
        }
    }
}