using UnityEngine;

public abstract class ChaseTemplateSO : ScriptableObject
{
    public abstract IChaseStrategy CreateStrategy(Unit unit);
}