using UnityEngine;

[CreateAssetMenu(menuName = "AI/Chase/ChaseStateSO")]
public class ChaseStateSO : StateScriptableObject
{
    public float stoppingDistance;
    public float chaseDistance = 30;
    public float transitionToAttackDistance = 5;
    public ChaseTemplateSO chaseStrategy;
}
