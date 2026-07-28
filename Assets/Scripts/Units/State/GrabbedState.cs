using UnityEngine;

public class GrabbedState : State
{
    private GrabbedStateSO config;
    
    public GrabbedState(Unit unit, StateMachine stateMachine, GrabbedStateSO config) : base(unit, stateMachine)
    {
        this.config = config;
    }
    public override void EnterState()
    {
        base.EnterState();
        stateMachine.DebugPrintStateName("I am in state : ");
        unit.Agent.ResetPath();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    public override void PhysicUpdate()
    {
        base.PhysicUpdate();
    }

    public override void EvaluateTransition()
    {
        base.EvaluateTransition();
    }

    public override void OnChomped(GameObject chomper)
    {
        base.OnChomped(chomper);
        Debug.Log("im chomped grabbed");
        unit.rb.isKinematic = true;
        unit.GetComponent<Collider>().isTrigger = true;
        unit.Agent.enabled = false;
        unit.collisionTimer = 20f;
    }

    public override void OnChompedRelease(GameObject chomper)
    {
        base.OnChompedRelease(chomper);
        unit.GetComponent<Collider>().isTrigger = false;
        unit.collisionTimer = 0.5f;
    }

    public override void OnUnitCollisionEnter(Collision collision)
    {
        base.OnUnitCollisionEnter(collision);
    }

    public override void OnUnitTriggerEnter(Collider other)
    {
        base.OnUnitTriggerEnter(other);
    }

    public override StateScriptableObject GetConfig()
    {
        return base.GetConfig();
    }
}
