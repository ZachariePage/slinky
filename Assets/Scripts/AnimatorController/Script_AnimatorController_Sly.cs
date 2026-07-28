using UnityEngine;

public class Script_AnimatorController_Sly : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool isJumping = false;

    private void Start()
    {
        if (animator == null)
        {
            Debug.LogError("Animator must be assigned.");
        }
    }

    public void SetJumping(bool jumping)
    {
        animator.SetBool("IsJumping", jumping);
        isJumping = jumping;
    }
    
    public void SetGrounded(bool grounded)
    {
        animator.SetBool("IsGrounded", grounded);
    }

    public void TriggerBite()
    {
        animator.SetTrigger("Bite");
    }

    public void SetBiteHolding(bool holding)
    {
        animator.SetBool("IsHolding", holding);
    }
    
    public void SetEating(bool eating)
    {
        animator.SetBool("IsEating", eating);
    }
    
    public void TriggerHurt()
    {
        animator.SetTrigger("Hurt");
    }

    public void TriggerDeath()
    {
        animator.SetTrigger("Death");
    }

    public void SetPulling(bool pulling)
    {
        animator.SetBool("IsPulling", pulling);
    }
    
    public void TriggerSlide()
    {
        animator.SetTrigger("Slide");
    }

    public void SetCinematic(bool active)
    {
        animator.SetBool("InCinematic", active);
    }

    public void TriggerCinematicEnd()
    {
        animator.SetTrigger("EndCinematic");
    }

    /// <summary>
    /// Set the speed and direction to play the right walking animation
    /// </summary>
    /// <param name="currentSpeed">value from -1 to 1 (1 move forward / -1 move backward)</param>
    /// <param name="currentDirection">value from -1 to 1 (1 go right / -1 go left)</param>
    public void SetSpeedAndDirection(float currentSpeed, float currentDirection)
    {
        
    }
}

