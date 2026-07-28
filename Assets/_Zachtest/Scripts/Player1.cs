using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum player
{
    player1,
    player2
}
public class Player1 : MonoBehaviour
{
    public Vector3 forceDirection;
    public Vector3 velocity;
    public float force;
    public Rigidbody rb;
    
    public Vector2 moveInput;
    public float rotationSpeed = 180f;
    
    public bool IsJumping = false;
    public bool Grounded = false;

    public float JumpTimer = 0;

    public float JumpHeight;
    public float JumpTime;
    public float JumpForce;
    
    [Tooltip("0.1f last ring to jump will have 0.10% power. 0.5 0.50%. Diminish from 1 to float"), Range(0.1f, 1f)]
    public float JumpDiminishingPower = 0.1f;
    
    private float movementYaw; 
    
    [Tooltip("two on top kinda same thing ngl but work off jumpheight and jumptime. JustAddForce is simply add force with jumpforce")]
    public enum JumpMode
    {
        DesignerControlled,   
        PhysicsBased,
        JustAddForce
    }
    
    [Tooltip("Which part of slinky jump")]
    public enum JumpDistribution
    {
        TopOnly,      
        WholeSlinky     
    }
    
    [Tooltip("Direction base is just top down right left, Rotation AD rotate while WS is forwad backward")]
    public enum MoveControl
    {
        DirectionBased,   
        RotationBased     
    }

    public MoveControl moveControl = MoveControl.DirectionBased;

    public player player = player.player1;
    
    public JumpDistribution jumpDistribution = JumpDistribution.WholeSlinky;

    public JumpMode jumpMode = JumpMode.DesignerControlled;
    
    Rigidbody[] slinkyBodies;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        slinkyBodies = transform.parent.GetComponentsInChildren<Rigidbody>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        velocity = rb.linearVelocity;
        if(JumpTimer > 0 ) JumpTimer -= Time.deltaTime;
        
        checkGround();
        
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void checkGround()
    {
        Grounded = Physics.Raycast(transform.position, Vector3.down, 0.6f);

        if (Grounded) IsJumping = false;
    }
    
    public void onMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void onJump(InputAction.CallbackContext context)
    {
        if(context.performed) Jump();
    }
    
    
    public void MovePlayer()
    {
        Vector3 vel = rb.linearVelocity;

        switch (moveControl)
        {
            case MoveControl.DirectionBased:
                ApplyDirectionBasedMovement(vel);
                break;

            case MoveControl.RotationBased:
                ApplyDirectionBasedMovement(vel);
                break;
        }
    }


    void ApplyDirectionBasedMovement(Vector3 vel)
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        vel.x = movement.x * force;
        vel.z = movement.z * force;
        
        rb.linearVelocity = vel;
    }
    
    void Jump()
    {
        if (IsJumping || JumpTimer > 0) return;

        IsJumping = true;
        JumpTimer = 1;

        float jumpVelocity;

        switch (jumpMode)
        {
            case JumpMode.DesignerControlled:
                jumpVelocity = (2f * JumpHeight) / JumpTime;
                ApplyJump(jumpVelocity);
                break;

            case JumpMode.PhysicsBased:
                float gravity = Physics.gravity.y;
                jumpVelocity = Mathf.Sqrt(2f * JumpHeight * -gravity);
                ApplyJump(jumpVelocity);
                break;
            case JumpMode.JustAddForce:
                rb.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
                break;
            default:
                jumpVelocity = 0;
                break;
        }
    }
    
    void ApplyJump(float jumpVelocity)
    {
        switch (jumpDistribution)
        {
            case JumpDistribution.TopOnly:
                BoostBody(slinkyBodies[0], jumpVelocity);
                break;

            case JumpDistribution.WholeSlinky:
                ApplyDistributedJump(jumpVelocity);
                break;
        }
    }

    
    void ApplyDistributedJump(float jumpVelocity)
    {
        int count = slinkyBodies.Length / 2;

        for (int i = 0; i < count; i++)
        {
            float t = (count > 1) ? 1f - i / (count - 1) : 1f;
            float strength = Mathf.Lerp(JumpDiminishingPower, 1f, t);

            int index = 0;

            switch (player)
            {
                case player.player1:
                    index = i;
                    break;

                case player.player2:
                    int startOfSecondHalf = count;
                    int localReversed = (count - 1) - i;
                    index = startOfSecondHalf + localReversed;
                    break;
            }

            BoostBody(slinkyBodies[index], jumpVelocity * strength);
        }
    }


    void BoostBody(Rigidbody rb, float vel)
    {
        rb.AddForce(Vector3.up * vel, ForceMode.VelocityChange);
    }


}
