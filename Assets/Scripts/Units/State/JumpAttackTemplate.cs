using UnityEngine;

[CreateAssetMenu(menuName = "AI/Attack/JumpTemplate")]
public class JumpAttackTemplate : AttackTemplateSO
{
    [Header("Coldown settings")]
    public float coldown;
    [Header("Jump settings")]
    public float HorizontalJumpForce;
    public float VerticalJumpForce;
    
    public override IAttackStrategy CreateStrategy(Unit unit)
    {
        return new JumpAttackStrategy(
            unit,
            coldown,
            HorizontalJumpForce,
            VerticalJumpForce
        );
    }
}
