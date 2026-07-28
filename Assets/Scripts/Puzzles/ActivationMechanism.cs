using System;
using UnityEngine;

public abstract class ActivationMechanism : MonoBehaviour, IActivable
{
    [Header("Puzzle base settings")]
    [SerializeField] protected bool activated;
    [SerializeField] protected bool canBeExternallyActivated;
    
    [Header("Dirty debugs")] 
    [SerializeField] bool AlwayPlayCues = true;
    [SerializeField] protected GameCue[] activationCues;
    [SerializeField] protected GameCue[] deactivationCues;

    [SerializeField] private Transform vfxLocation;

    protected virtual void Awake()
    {
        if (vfxLocation == null)
        {
            vfxLocation = transform;
        }
    }

    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    public virtual void Activate()
    {
        activated = true;
        if (AlwayPlayCues)
        {
            foreach (GameCue cue in activationCues)
            {
                cue?.Execute(vfxLocation.position);
            }
        }
    }

    public virtual void Deactivate()
    {
        activated = false;
        if (AlwayPlayCues)
        {
            foreach (GameCue cue in deactivationCues)
            {
                cue?.Execute(vfxLocation.position);
            }
        }
    }

    public virtual bool ActivateMessage()
    {
        if(canBeExternallyActivated) return true;
        return false;
    }

    public virtual bool IsActivated()
    {
        return activated;
    }

    public bool CanBeExternallyActivated()
    {
        return canBeExternallyActivated;
    }
}
