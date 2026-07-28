using UnityEngine;

public class GrabbedTemplateSO : StateScriptableObject
{
    public float stoppingDistance;
    public float transitionToAttackDistance = 5;
    public ChaseTemplateSO chaseStrategy;
}
