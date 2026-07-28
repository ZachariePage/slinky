using UnityEngine;

public class State
{
    protected Unit unit;
    protected StateMachine stateMachine;

    public State(Unit unit, StateMachine stateMachine)
    {
        this.unit = unit;
        this.stateMachine = stateMachine;
    }

    public virtual void EnterState()
    {
        StateScriptableObject config = stateMachine.CurrentEnemyState.GetConfig();
    }

    public virtual void ExitState()
    {
    }

    public virtual void FrameUpdate()
    {

    }
    public virtual void PhysicUpdate()
    {
    
    }

    public virtual void EvaluateTransition()
    {
        
    }

    public virtual void OnChomped(GameObject chomper)
    {
        
    }
    public virtual void OnChompedRelease(GameObject chomper)
    {
        
    }

    public virtual void OnUnitCollisionEnter(Collision collision)
    {
        
    }

    public virtual void OnUnitTriggerEnter(Collider other)
    {
        
    }
    public virtual StateScriptableObject GetConfig()
    {
        return null;
    }
}
