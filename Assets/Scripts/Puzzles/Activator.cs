using UnityEngine;

public class Activator : MonoBehaviour
{
    public ActivationMechanism[] conditions;

    public bool completed;
    protected Animator anim;

    [Header("Dirty debugs")] 
    [SerializeField] bool AlwayPlayCues = true;
    [SerializeField] protected GameCue[] completionCues;
    [SerializeField] private Transform completionCuesLocation;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.enabled = false;
        }

        if (completionCuesLocation == null)
        {
            completionCuesLocation = transform;
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(completed) return;
        if (CheckConditions())
        {
            Completion();
        }
    }

    bool CheckConditions()
    {
        foreach (ActivationMechanism condition in conditions)
        {
            if (condition == null)
            {
                return false;
            }
            if(!condition.IsActivated()) return false;
        }
        
        return true;
    }

    protected virtual void Completion()
    {
        completed = true;
        if (anim != null)
        {
            anim.enabled = true;
        }
        
        if (AlwayPlayCues)
        {
            foreach (GameCue cue in completionCues)
            {
                cue?.Execute(completionCuesLocation.position);
            }
        }
    }
}
