using UnityEngine;

public class Script_AnimatorController_Pusher : MonoBehaviour
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
    
    public void SetIsGrabbed(bool grounded)
    {
        animator.SetBool("IsGrabbed", grounded);
    }

    public void TriggerHit()
    {
        animator.SetTrigger("Hit");
    }
    
    /// <summary>
    /// Set the speed to play the right walking animation
    /// </summary>
    /// <param name="currentSpeed">value from 0 to 1 (1 move forward / 0 idle)</param>
    public void SetSpeedAndDirection(float currentSpeed)
    {
        
    }
}
