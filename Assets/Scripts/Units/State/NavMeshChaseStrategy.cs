using UnityEngine;
using UnityEngine.AI;

public class NavMeshChaseStrategy : IChaseStrategy
{
    Unit unit;
    NavMeshAgent agent;

    public NavMeshChaseStrategy(
        Unit unit,
        float angularSpeed,
        float acceleration)
    {
        this.unit = unit;

        agent = unit.GetComponent<NavMeshAgent>();
        agent.angularSpeed = angularSpeed;
        agent.acceleration = acceleration;
        agent.updateRotation = true;
    }

    public void Execute()
    {
        GameObject currentTarget = unit.targetGO;
        if (currentTarget != null && agent.isOnNavMesh)
        {
            agent.SetDestination(currentTarget.transform.position); 
        }
    }
    
    public bool HasTarget()
    {
        return unit.targetGO != null;
    }
}