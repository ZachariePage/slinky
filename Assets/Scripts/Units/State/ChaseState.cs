using UnityEngine;
using UnityEngine.AI;

public class ChaseState : State
{
    private ChaseStateSO config;
    
    IChaseStrategy strategy;
    
    public ChaseState(Unit unit, StateMachine stateMachine, ChaseStateSO config) : base(unit, stateMachine)
    {
        this.config = config;
        strategy = config.chaseStrategy.CreateStrategy(unit);
    }
    
    public override void EnterState()
    {
        base.EnterState();
        unit.Agent.stoppingDistance = config.stoppingDistance;

        unit.targetGO = unit.FindClosestPlayer();
        
        //stateMachine.DebugPrintStateName("I am in state : ");
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
        if (unit.IsStun() || unit.isContactingSlinky)
        {
            return;
        }
        
        if (unit.targetGO != null && unit.Agent.isOnNavMesh)
        {
            unit.transform.LookAt(unit.targetGO.transform);
            float distanceSquare = Vector3.SqrMagnitude(unit.transform.position - unit.targetGO.transform.position);

            if (distanceSquare < 30 * 30)
            {
                strategy.Execute();
            }
            else
            {
                unit.Agent.ResetPath();
            }
        }
        else
        {
            unit.targetGO = unit.FindClosestPlayer();
        }
    }

    public override void EvaluateTransition()
    {
        base.EvaluateTransition();
        if (unit.JustGotGrabbedTimer > 0 || unit.isContactingSlinky) return;
        if (unit.targetGO != null)
        {
            float distanceSquare = Vector3.SqrMagnitude(unit.targetGO.transform.position - unit.transform.position);
            if (distanceSquare < (config.transitionToAttackDistance * config.transitionToAttackDistance))
            {
                unit.stateMachine.ChangeState(unit.attackState);
            }
        }
    }

    public override void OnChomped(GameObject chomper)
    {
        base.OnChomped(chomper);
    }

    public override void OnChompedRelease(GameObject chomper)
    {
        base.OnChompedRelease(chomper);
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
