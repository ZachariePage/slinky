using UnityEngine;

[CreateAssetMenu(menuName = "AI/Attack/AttackStateSO")]
public class AttackStateSO : StateScriptableObject
{
    public LayerMask layerMask;
    public float AttackRadius = 5;
    public AttackTemplateSO attackStrategy;
}
