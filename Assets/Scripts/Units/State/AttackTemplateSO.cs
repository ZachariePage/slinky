using UnityEngine;

public abstract class AttackTemplateSO : ScriptableObject
{
    public abstract IAttackStrategy CreateStrategy(Unit unit);
}
