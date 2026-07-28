using UnityEngine;

public class ActivationBark : ActivationMechanism, IListeneable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Activate()
    {
        base.Activate();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override bool ActivateMessage()
    {
        return base.ActivateMessage();
    }

    public void OnHearingBark()
    {
        Debug.Log("Hearing Bark");
    }
}
