using UnityEngine;

[CreateAssetMenu(menuName = "AI/Chase/PhysicsNavMeshTemplate")]
public class PhysicsNavMeshChaseTemplate : ChaseTemplateSO
{
    public float moveForce;
    public override IChaseStrategy CreateStrategy(Unit unit)
    {
        var strategy = new PhysicsNavMeshChaseStrategy(
            unit,
            moveForce
        );

        return strategy;
    }
}