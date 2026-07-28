using UnityEngine;
using UnityEngine.AI;

public class PhysicsNavMeshChaseStrategy : IChaseStrategy
{
    private Unit unit;

    private NavMeshPath path;
    private int currentCornerIndex;

    private float repathTimer;
    private float repathRate = 0.5f;

    private float moveForce = 20f;

    public PhysicsNavMeshChaseStrategy(Unit unit, float force)
    {
        this.unit = unit;
        this.moveForce = force;
        path = new NavMeshPath();
    }

    public bool HasTarget()
    {
        return unit.targetGO != null;
    }

    public void Execute()
    {
        if (!HasTarget()) return;

        repathTimer -= Time.deltaTime;
        
        if (repathTimer <= 0f)
        {
            repathTimer = repathRate;

            NavMesh.CalculatePath(
                unit.transform.position,
                unit.targetGO.transform.position,
                NavMesh.AllAreas,
                path
            );

            currentCornerIndex = 0;
        }

        if (path.corners.Length == 0) return;
        if (currentCornerIndex >= path.corners.Length) return;

        Vector3 targetPoint = path.corners[currentCornerIndex];

        Vector3 dir = (targetPoint - unit.transform.position);
        dir.y = 0f;

        float dist = dir.magnitude;
        
        if (dist < 0.5f)
        {
            currentCornerIndex++;
            return;
        }

        dir.Normalize();
        
        unit.gameObject.GetComponent<Rigidbody>().AddForce(dir * moveForce, ForceMode.Force);
    }
}