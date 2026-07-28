using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class BigBlocLego : MonoBehaviour, IChompable
{
    private bool createJointOnChomp = true;
    [SerializeField] private ChompableSpringValue springValue;

    [Tooltip("Nombre de joueurs requis pour bouger le bloc")]
    public int requiredPlayers = 2;

    [Header("Retroaction")]
    [SerializeField] private GameCue[] OnChompCues;
    [SerializeField] private GameCue[] OnReleaseCues;

    [Header("Chomp")]
    [SerializeField] private AttachmentConfig notEnoughPlayersConfig;
    [SerializeField] private AttachmentConfig enoughPlayersConfig;
    public AttachmentType attachmentType = AttachmentType.ChildTransform;

    private Rigidbody rb;
    private bool wasKinematic;
    private List<PlayerChomp> players = new List<PlayerChomp>();

    RigidbodyConstraints startingConstraint;
    
    private enum ChompAxis { None, X, Z }
    [SerializeField] private ChompAxis manualAxis = ChompAxis.None;
    private ChompAxis lockedAxis = ChompAxis.None;


    private float startingMass;
    private bool IsKinematic;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
        }
        
        startingConstraint = rb.constraints;
        startingMass = rb.mass;
        IsKinematic = rb.isKinematic;
    }

    public void OnChomped(GameObject chomper)
    {
        if (rb == null) return;
        
        PlayerChomp pc = chomper.GetComponent<PlayerChomp>();
        if (!players.Contains(pc))
        {
            players.Add(pc);
        }
        
        
        lockedAxis = GetChompAxis(chomper.transform.position);
        
        if (players.Count >= requiredPlayers)
        {
            ActivateJointForAll();
            rb.isKinematic = false;
            ApplyAxisConstraints();
        }

        foreach (GameCue c in OnChompCues)
        {
            c?.Execute(transform.position);
        }
        rb.mass = 2f;
    }
    
    private ChompAxis GetChompAxis(Vector3 chomperPosition)
    {
        if(manualAxis != ChompAxis.None) return manualAxis;
        Vector3 localOffset = transform.InverseTransformPoint(chomperPosition);
        return Mathf.Abs(localOffset.x) >= Mathf.Abs(localOffset.z) ? ChompAxis.X : ChompAxis.Z;
    }
    
    private void ApplyAxisConstraints()
    {
        RigidbodyConstraints constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;

        if (lockedAxis == ChompAxis.X)
            constraints |= RigidbodyConstraints.FreezePositionZ;
        else
            constraints |= RigidbodyConstraints.FreezePositionX; 

        rb.constraints = constraints;
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
        
        //rb.isKinematic = true;
        rb.isKinematic = IsKinematic;
        if (players.Count == 0)
        {
            lockedAxis = ChompAxis.None;
            rb.constraints = startingConstraint;
        }

        foreach (GameCue c in OnReleaseCues)
        {
            c?.Execute(transform.position);
        }
        
        rb.mass = startingMass;
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
            return  attachmentType;
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
            return  enoughPlayersConfig;
        }
    }
}