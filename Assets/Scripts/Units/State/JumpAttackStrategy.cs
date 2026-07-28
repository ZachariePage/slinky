using UnityEngine;

public class JumpAttackStrategy : IAttackStrategy
{
    public float coldown;
    public float HorizontalJumpForce;
    public float VerticalJumpForce;

    public JumpAttackStrategy(Unit unit, float coldown, float horizontalJumpForce, float verticalJumpForce)
    {
        this.coldown = coldown;
        HorizontalJumpForce = horizontalJumpForce;
        VerticalJumpForce = verticalJumpForce;
    }

    public void Execute()
    {

    }
}
