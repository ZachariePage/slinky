using UnityEngine;

public class AttackState : State
{
    private AttackStateSO config;
    
    IAttackStrategy strategy;
    public AttackState(Unit unit, StateMachine stateMachine, AttackStateSO config) : base(unit, stateMachine)
    {
        this.config = config;
        strategy = config.attackStrategy.CreateStrategy(unit);
    }
    
    public override void EnterState()
    {
        base.EnterState();
        //stateMachine.DebugPrintStateName("I am in state : ");
        unit.anim.SetTrigger("Win");
        AttackPlayer();
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
        strategy.Execute();
    }

    private void AttackPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(unit.transform.position, config.AttackRadius, LayerMask.GetMask("Player"));

        foreach (Collider hit in hits)
        {
            if (hit.gameObject.CompareTag("Player"))
            {
                StickyEnemies sticky = unit as StickyEnemies;
                
                if (sticky != null)
                {
                    sticky.OnPlayerHit(hit.gameObject);
                }
            }
        }

        unit.JustGotGrabbedTimer = 5;
        unit.stateMachine.ChangeState(unit.chaseState);
    }
    public override void EvaluateTransition()
    {
        base.EvaluateTransition();
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
        return config;
    }
}
