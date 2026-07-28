using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Chase/NavMeshTemplate")]
public class NavMeshChaseTemplate : ChaseTemplateSO
{
    [Header("Movement")]
    public float angularSpeed = 120f;
    public float acceleration = 8f;

    public override IChaseStrategy CreateStrategy(Unit unit)
    {
        return new NavMeshChaseStrategy(
            unit,
            angularSpeed,
            acceleration
        );
    }
}