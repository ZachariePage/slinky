using UnityEngine;

public class ActivationTrigger : ActivationMechanism
{
    [SerializeField] private int NumberOfPeopleRequired = 6;
    [SerializeField] private LayerMask acceptedLayer;
    [SerializeField] private int goals;
    [SerializeField] protected GameCue[] OnTriggerCue;
    
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }

    public override void Activate()
    {
        base.Activate();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override bool IsActivated()
    {
        return base.IsActivated();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (acceptedLayer == (acceptedLayer | (1 << other.gameObject.layer)))
        {
            goals++;
            if (goals >= NumberOfPeopleRequired)
            {
                Activate();
            }
        }
    }

    private void OnGoal(Transform goalTransform)
    {
        foreach (GameCue cue in OnTriggerCue)
        {
            cue?.Execute(goalTransform.position);
        }
    }
}
