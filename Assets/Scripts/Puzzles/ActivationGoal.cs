using UnityEngine;

public class ActivationGoal : ActivationMechanism
{
    [SerializeField] private int NumberOfGoalRequired = 1;
    [SerializeField] protected GameCue[] goalCues;
    private int goals;
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
        if (other.GetComponent<Football>())
        {
            goals++;
            OnGoal(other.transform);
            if (goals >= NumberOfGoalRequired)
            {
                Activate();
            }
        }
    }

    private void OnGoal(Transform goalTransform)
    {
        foreach (GameCue cue in goalCues)
        {
            cue?.Execute(goalTransform.position);
        }
    }
}
