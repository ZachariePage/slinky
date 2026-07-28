using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;


public class SlinAndKyControllerBase : MonoBehaviour
{
    public enum PlayerNumber
    {
        Player1,
        Player2
    }

    private bool gameplayEnabled = true;
    private bool ignoreInputUntilRelease = false;

    //Serialized fields
    [Header("Player Settings")] [SerializeField]
    private PlayerNumber playerNumber = PlayerNumber.Player1;

    [SerializeField] private PlayerControllerData playerControllerData;

    // Runtime values — copied from SO at Start, modified by effects, reset to initial values
    private float moveSpeed;
    private float maxSpeed;
    private float smoothTurnSpeed;
    private float jumpForce;
    private float jumpHeight;
    private float jumpGravity;
    private float jumpGravityMultiplier;
    private float coneAngle;
    private Vector3 slinkyDirection;
    float movementMultiplier = 1f;

    // Initial values — copied from SO at Start, never modified, used for resetting
    private float initialSpeed;
    private float initialMaxSpeed;
    private float initialSmoothTurnSpeed;
    private float initialJumpForce;
    
    // private variable for the controller
    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    private int _slingshotStartFrame;
    private Rigidbody playerRb;
    private Collider playerCollider;
    private Vector2 moveInput;
    private PlayerChomp playerChomp;
    private Vector3 slideNormal;
    private Vector3 desiredDirection;
    private PlayerInput playerInput;
    private Coroutine speedModifierCoroutine;
    private Coroutine jumpModifierCoroutine;
    private Coroutine turnModifierCoroutine;
    
    private PauseMenuManager  pauseMenuManager;

    //private bools
    private bool isGrounded;
    private bool isStunned;
    private bool isMagnetized;
    private bool jumpHeld;
    private bool jumpCutApplied;
    private bool isSlingshotting;
    private bool isBigChomping;
    private bool isMovementPressed;
    private bool isDecelerating;
   
    private bool isOnHandle;
    private bool isOnDecelLessZone;
    
    
    
    //zach was here. Adding a landing delegate
    private bool isSliding;
    public LayerMask slideLayer;
    public event Action OnLanded;
    public event Action OnSlingshot;
    public event Action OnEndSlingshot;

    private const int MinUngroundedFrames = 3;
    private int ungroundedFrameCount;
    private bool wasGrounded;
    private bool usingMaxClamp = true;
    [SerializeField] private Animator anim;
    
    //zach wasnt here

    [ReadOnly, SerializeField] private RawImage blackFadeImage;

    public event Action<PlayerNumber> OnPlayerJump;
    public event Action<PlayerNumber> OnPlayerTurn;
    public event Action<PlayerNumber> OnPlayerMove;
    public event Action<PlayerNumber> OnPlayerStopMove;
    public event Action<PlayerNumber> OnPlayerLanding;


    
    
    
    private void OnDisable()
    {
        ExplodeThis.OnExplosion -= ExplosionRumbleStart;
    }

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        playerChomp = GetComponent<PlayerChomp>();
        
        if (playerRb == null)
        {
            Debug.LogError("Rigidbody component missing from player.");
        }

        if (playerCollider == null)
        {
            Debug.LogError("Collider component missing from player.");
        }

        if (playerChomp == null)
        {
            Debug.LogError("PlayerChomp component missing from player.");
        }

        if (playerControllerData == null)
        {
            Debug.LogError("PlayerControllerData missing from player.");
        }

        // Copy SO values into runtime variables
        moveSpeed = playerControllerData.moveSpeed;
        maxSpeed = playerControllerData.maxSpeed;
        smoothTurnSpeed = playerControllerData.smoothTurnSpeed;
        jumpForce = playerControllerData.jumpForce;
        jumpHeight = playerControllerData.jumpHeight;
        coneAngle = playerControllerData.coneMaxAngle / 2;
        // Store initial values for resetting after effects
        initialSpeed = playerControllerData.moveSpeed;
        initialMaxSpeed = playerControllerData.maxSpeed;
        initialSmoothTurnSpeed = playerControllerData.smoothTurnSpeed;
        initialJumpForce = playerControllerData.jumpForce;

        playerChomp.onChompHit += ChompVibration;
        playerChomp.onReleaseChompedEvent += StopChompVibration;
        ExplodeThis.OnExplosion += ExplosionRumbleStart;
        RecalculateJumpGravity();
    }

    


    private void Start()
    {
        
        isMagnetized = false;
        jumpCutApplied = false;
        isSlingshotting = false;
        isBigChomping = false;
        isOnHandle = false;
        isMovementPressed = false;
        isDecelerating = false;
        
        playerInput = GetComponent<PlayerInput>();
        
        
        // IMPORTANT: only auto-find if nothing is assigned in Inspector
        if (anim == null)
            anim = GetComponentInChildren<Animator>(true);
    }

    private void Update()
    {
        if (!isGrounded)
        {
            CalculateBlackFade();
        }
        
    }

    private void LateUpdate()
    {
        if (isSlingshotting )
        {
            Vector3 hVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
            if (hVel.magnitude > playerControllerData.maxSlingshotSpeed)
            {
                hVel = hVel.normalized * playerControllerData.maxSlingshotSpeed;
                playerRb.linearVelocity = new Vector3(hVel.x, playerRb.linearVelocity.y, hVel.z);
            }
            
        }
        
        playerRb.linearVelocity = Vector3.ClampMagnitude(playerRb.linearVelocity, playerControllerData.maxVelocity);
    }

    // Physics
    private void FixedUpdate()
    {
        //zach changes from the below line
        //isGrounded = IsGrounded();
        //to this
        bool groundedThisFrame = IsGrounded();

        if (!groundedThisFrame)
        {
            ungroundedFrameCount++;
        }
        else
        {
            if (!wasGrounded && ungroundedFrameCount >= MinUngroundedFrames)
            {
                anim.SetBool("IsJumping", false);
                OnLanded?.Invoke();
                OnPlayerLanding?.Invoke(playerNumber);
            }
            
            ungroundedFrameCount = 0;
        }

        wasGrounded = groundedThisFrame;
        isGrounded = groundedThisFrame;
        

        if (!gameplayEnabled || IsBlockedByIntro())
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            UpdateAnimation();
            return;
        }

        if (isStunned) return;
        

        if (isOnHandle)
        {
            ApplyGravity();
            return;
        }

        // Relative movement calculation
        desiredDirection = GetCameraRelativeDirection();
        
        UpdateAnimation();
        

        var chompedObject = playerChomp.ChompedObject;
        var chompable = chompedObject != null ? chompedObject.GetComponent<IChompable>() : null;
        var attachmentConfig = chompable?.GetAttachmentConfig();

        if (!playerChomp.IsChomping() ||
            (attachmentConfig != null && attachmentConfig.allowRotation))
        {
            Vector3 clampedDirection = ClampDirectionToCone(desiredDirection);
            ApplyRotation(clampedDirection);
        }
        //Apply movement base if character is sliding or not
        if (desiredDirection.magnitude > 0.1f)
        {
            TryStepUp();
            if (isSliding)
            {
                Vector3 projectedDir = Vector3.ProjectOnPlane(
                    desiredDirection, slideNormal).normalized;
                Vector3 slideDown = Vector3.ProjectOnPlane(Vector3.down, slideNormal).normalized;
                float slideDot = Vector3.Dot(projectedDir, slideDown);
                if (slideDot > 0)
                {
                    Vector3 slideDownComponent = slideDown*slideDot;
                    projectedDir = (projectedDir-slideDownComponent*0.8f).normalized;
                    anim.SetBool("IsSliding", true);
                }
                else
                {
                    anim.SetBool("IsSliding", false);
                }
                ApplyMovement(projectedDir, movementMultiplier);
            }
            else
            {
                ApplyMovement(desiredDirection, movementMultiplier);
            }
        }

        if (isGrounded && !isOnDecelLessZone)
        {   
            
            jumpCutApplied = false;
            if (isDecelerating)
            {
                ApplyDeceleration();
            }
        }
        
        ApplyJump();
        ApplyGravity();
        
    }

    public Animator GetAnimator()
    {
        return anim;
    }

    private void CalculateBlackFade()
    {
        if (GetBlackFadeImage() == null)
            return;

        Ray ray = new Ray(transform.position, Vector3.down);
        const float maxDistance = 10f;

        // Cast against everything so it's blocked by the first hit, but include triggers if KillZone is a trigger.
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, ~0, QueryTriggerInteraction.Collide) &&
            hit.collider.gameObject.layer == LayerMask.NameToLayer("KillZone"))
        {
            float alpha = Mathf.Clamp01(hit.distance / maxDistance);
            float fade = 1f - alpha;

            Color color = blackFadeImage.color;
            color.a = Mathf.Max(color.a, fade);
            blackFadeImage.color = color;
        }
        else
        {
            // Gradually fade back to transparent when not above a KillZone
            Color color = blackFadeImage.color;
            color.a = Mathf.Lerp(color.a, 0f, Time.deltaTime * 5f);
            blackFadeImage.color = color;
        }
    }

    private object GetBlackFadeImage()
    {
        if (blackFadeImage == null)
        {
            GameObject obj = GameObject.FindWithTag("BlackFade");
            if (obj != null)
            {
                blackFadeImage = obj.GetComponent<RawImage>();
            }
        }

        return blackFadeImage;
    }

    public void SetGameplayEnabled(bool enabled)
    {
        
        gameplayEnabled = enabled;

        moveInput = Vector2.zero;
        jumpHeld = false;
        jumpBufferCounter = 0f;
        isMovementPressed = false;
        isDecelerating = false;

        ignoreInputUntilRelease = true;

        if (!enabled)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;

            if (playerInput != null)
                playerInput.enabled = false;

            CantMove();
            return;
        }

        if (playerInput != null)
        {
            playerInput.enabled = false;
            playerInput.enabled = true;
        }

        playerRb.linearVelocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;

        CanMove();
    }

    private bool IsBlockedByIntro()
    {
        return WorldManager.Instance != null && WorldManager.Instance.IsIntroControlLocked();
    }

    public void SetIntroCinematic(bool active)
    {
        if (anim == null)
        {
            Debug.LogWarning($"[{name}] No Animator assigned.");
            return;
        }

        if (anim.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[{name}] Animator has no Runtime Animator Controller assigned.");
            return;
        }

        anim.SetBool("InCinematic", active);
    }

    public void TriggerIntroCinematicEnd()
    {
        if (anim == null)
        {
            Debug.LogWarning($"[{name}] No Animator assigned.");
            return;
        }

        if (anim.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[{name}] Animator has no Runtime Animator Controller assigned.");
            return;
        }

        anim.SetTrigger("EndCinematic");
    }
    
    /** Character Movement Fonctions! */
    
    private void ApplyGravity()
    {
        if (isSliding)
        {
            Vector3 projectedDir = Vector3.ProjectOnPlane(
                desiredDirection, slideNormal).normalized;
            Vector3 slideDown = Vector3.ProjectOnPlane(Vector3.down, slideNormal).normalized;
            float slideDot = Vector3.Dot(projectedDir, slideDown);
            if (slideDot < 0)
            {
                playerRb.AddForce(slideDown * playerControllerData.slideGravityForce, ForceMode.Acceleration);
            }
            
            
        }
        
        if(isGrounded) return;
        
        if (GetVelocity().y > 0 && jumpHeld && !jumpCutApplied)
        {
            playerRb.AddForce(Physics.gravity * (jumpGravityMultiplier - 1f),
                ForceMode.Acceleration);
        }

        if (GetVelocity().y > 0 && !jumpHeld && !jumpCutApplied)
        {
            Vector3 vel = playerRb.linearVelocity;

            vel.y *= playerControllerData.jumpCutMultiplier;

            jumpCutApplied = true;
            playerRb.linearVelocity = vel;
        }

        if (playerRb.linearVelocity.y >= 0f && jumpCutApplied)
        {
            playerRb.AddForce(Physics.gravity * (playerControllerData.jumpCutGravityMultiplier - 1f),
                ForceMode.Acceleration);
        }
        else if (playerRb.linearVelocity.y < 0f)
        {
            playerRb.AddForce(Physics.gravity * (playerControllerData.fallGravityMultiplier - 1f),
                ForceMode.Acceleration);
        }
        
    }

    //Movement depends of the camera direction
    private Vector3 GetCameraRelativeDirection()
    {
        UnityEngine.Camera cam = UnityEngine.Camera.main;
        if (!cam) return Vector3.zero;

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        // moveInput.y = stick haut/bas, moveInput.x = stick gauche/droite
        return (camForward * moveInput.y + camRight * moveInput.x).normalized;
    }

    private void ApplyDeceleration()
    {
        Vector3 horizontalVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        
        if (!isMagnetized && SlinkyManager.CurrentZone != SlinkyManager.SlinkyZone.Hard && 
            horizontalVel.magnitude > 0.1f)
        {
            
            Vector3 deceleration = -horizontalVel.normalized *
                                   (playerControllerData.decelerationForce * Time.fixedDeltaTime);

            if (deceleration.magnitude > horizontalVel.magnitude)
            {
                deceleration = -horizontalVel;
            }
        
            playerRb.AddForce(deceleration, ForceMode.VelocityChange);
        }
        else if(!isMagnetized)
        {
            float dotProduct = Vector3.Dot(horizontalVel, transform.right);
            
            Vector3 deceleration = -transform.right * (dotProduct * (playerControllerData.decelerationForce * Time.fixedDeltaTime));

            if (deceleration.magnitude > Mathf.Abs(dotProduct))
            {
                deceleration = -transform.right * dotProduct;
            }
            playerRb.AddForce(deceleration, ForceMode.VelocityChange);
            
        }
        
        if (horizontalVel.magnitude <= 0.1f)
        {
            isDecelerating = false;
        }
    }

    private void ApplyRotation(Vector3 clampedDirection)
    {
        
        Quaternion targetRotation = Quaternion.LookRotation(clampedDirection);
        float step = smoothTurnSpeed * Time.fixedDeltaTime;
        Quaternion newRotation = Quaternion.RotateTowards(playerRb.rotation, targetRotation, step);
        Vector3 direction = newRotation * Vector3.forward;

        playerRb.MoveRotation(Quaternion.LookRotation(ClampDirectionToCone(direction)));
    }

    private void ApplyMovement(Vector3 desiredDirection, float multiplier)
    {
        if (multiplier <= 0f) return;
        Vector3 movement = Vector3.zero;

        if (isBigChomping)
        {
            float forwardAmount = Vector3.Dot(desiredDirection, transform.forward);
            movement = transform.forward * (forwardAmount * (moveSpeed * multiplier));
        }
        else if(isSlingshotting)
        {
            Vector3 hVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
            if (hVel.magnitude > 0.1f)
            {
                Vector3 slingDir = hVel.normalized;
                float parallelAmount = Vector3.Dot(desiredDirection, slingDir);
                Vector3 parallelComponent = slingDir * parallelAmount;
                movement = (desiredDirection - parallelComponent) * (moveSpeed * multiplier);
            }
            
        }
        else
        {
            movement = desiredDirection * (moveSpeed * multiplier);
        }
        
        playerRb.AddForce(movement, ForceMode.Acceleration);
        
        Vector3 horizontalVel = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
        
        // Speed cap
        if (!isSlingshotting)
        {
            if (horizontalVel.magnitude > maxSpeed)
            {
                horizontalVel = horizontalVel.normalized * maxSpeed;
                playerRb.linearVelocity = new Vector3(horizontalVel.x, playerRb.linearVelocity.y, horizontalVel.z);
            }
        }
        
    }

    private void ApplyJump()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = playerControllerData.coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
        }

        // Jump logic
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && (SlinkyManager.CurrentZone != SlinkyManager.SlinkyZone.Hard
            || SlinkyManager.CurrentWrappingMode == SlinkyManager.SlinkyWrapMode.SideWrap))
        {
            anim.SetBool("IsJumping", true);
            OnPlayerJump?.Invoke(playerNumber);
            Vector3 vel = playerRb.linearVelocity;
            vel.y = jumpForce;
            playerRb.linearVelocity = vel;
            
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
        else if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            anim.SetBool("IsJumping", true);
            OnPlayerJump?.Invoke(playerNumber);
            Vector3 vel = playerRb.linearVelocity;
            vel.y += 0.1f;
            playerRb.linearVelocity = vel;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }
    }

    private Vector3 ClampDirectionToCone(Vector3 desiredDir)
    {
        Vector3 chosenDir;

        if (desiredDir.magnitude > 0.1f)
        {
            chosenDir = desiredDir;
        }
        else
        {
            chosenDir = transform.forward;
        }

        if (slinkyDirection == Vector3.zero)
            return chosenDir;

        Vector3 coneCenter = -slinkyDirection;
        coneCenter.y = 0;
        coneCenter.Normalize();


        float calculatedAngle = Vector3.Angle(coneCenter, chosenDir);

        if (calculatedAngle <= coneAngle)
        {
            return chosenDir;
        }
        else
        {
            return Vector3.RotateTowards(coneCenter, chosenDir, coneAngle * Mathf.Deg2Rad, 0f);
        }
    }

    private void RecalculateJumpGravity()
    {
        // g = v₀² / (2h)
        jumpGravity = (jumpForce * jumpForce) / (2 * jumpHeight);
        jumpGravityMultiplier = jumpGravity / Physics.gravity.magnitude;
    }
    
    
    /** Input Events Functions! */
    
    public void OnMoveEvent(InputAction.CallbackContext context)
    {
        if (!gameplayEnabled || IsBlockedByIntro())
        {
            moveInput = Vector2.zero;
            isMovementPressed = false;
            isDecelerating = false;
            return;
        }

        Vector2 inputValue = context.ReadValue<Vector2>();

        if (ignoreInputUntilRelease)
        {
            if (inputValue.sqrMagnitude > 0.0001f)
            {
                moveInput = Vector2.zero;
                isMovementPressed = false;
                isDecelerating = false;
                return;
            }

            ignoreInputUntilRelease = false;
        }

        moveInput = inputValue;

        if (context.performed)
        {
            isMovementPressed = true;
            isDecelerating = false;
        }

        if (context.canceled)
        {
            isMovementPressed = false;
            isDecelerating = true;
        }
    }

    public void OnJumpEvent(InputAction.CallbackContext context)
    {
        if (!gameplayEnabled || IsBlockedByIntro() || ignoreInputUntilRelease)
        {
            jumpHeld = false;
            jumpBufferCounter = 0f;
            return;
        }

        if (context.performed)
        {
            jumpHeld = true;
            jumpBufferCounter = playerControllerData.jumpBufferTime;
        }

        if (context.canceled)
        {
            jumpHeld = false;
        }
    }

    public void OnPauseEvent(InputAction.CallbackContext context)
    {
        
        if (context.performed)
        {
            GetPauseMenuManager()?.TogglePause();
        }
        
    }
    private PauseMenuManager GetPauseMenuManager()
    {
        if (pauseMenuManager == null)
            pauseMenuManager = FindFirstObjectByType<PauseMenuManager>();
        return pauseMenuManager;
    }
    
    /** Utility Fonctions! */
    private bool IsGrounded()
    {
        CapsuleCollider capsule = (CapsuleCollider)playerCollider;
        Vector3 bottom = new Vector3(capsule.bounds.center.x, capsule.bounds.min.y + capsule.radius,
            capsule.bounds.center.z);

        RaycastHit hit;
        bool groundHit = Physics.SphereCast(bottom, capsule.radius * 0.9f, Vector3.down, out hit,
            playerControllerData.groundCheckDistance, ~LayerMask.GetMask("Player", "SlinkySegment"),QueryTriggerInteraction.Ignore);

        if (groundHit && ((1 << hit.collider.gameObject.layer) & playerControllerData.decelerationLessLayers) != 0)
        {
            isOnDecelLessZone = true;
        }
        else
        {
            isOnDecelLessZone = false;
               
        }
        
        if(groundHit && ((1 << hit.collider.gameObject.layer)  & slideLayer) != 0)
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit slideHit;

            if (Physics.Raycast(ray, out slideHit, 4f, slideLayer))
            {
                slideNormal = slideHit.normal;
                isSliding = true;
                anim.SetBool("IsSliding", true);
            }
            else
            {
                isSliding = false;
                anim.SetBool("IsSliding", false);
            }
            
        }
        else
        {
            
            isSliding = false;
            anim.SetBool("IsSliding", false);
        }
        
        return groundHit;
    }
    private void TryStepUp()
    {
        if (!isGrounded || isSlingshotting || isOnHandle) return;

        Vector3 forward = transform.forward;
        Vector3 footOrigin = transform.position + Vector3.up * 0.05f; // slightly above ground
        Vector3 stepOrigin = transform.position + Vector3.up * playerControllerData.maxStepHeight;

        // Raycast 1 — obstacle at foot level?
        bool footHit = Physics.Raycast(footOrigin, forward, out RaycastHit footHitInfo, 
            playerControllerData.stepCheckDistance, ~LayerMask.GetMask("Player", "SlinkySegment"));

        if (!footHit) return;

        // Raycast 2 — clear at step height?
        bool stepHit = Physics.Raycast(stepOrigin, forward, playerControllerData.stepCheckDistance, ~LayerMask.GetMask("Player", "SlinkySegment"));

        if (stepHit) return; // obstacle too tall

        // Raycast 3 — find the actual surface height to land on
        Vector3 aboveStep = stepOrigin + forward * playerControllerData.stepCheckDistance;
        if (Physics.Raycast(aboveStep, Vector3.down, out RaycastHit surfaceHit, 
                playerControllerData.maxStepHeight, ~LayerMask.GetMask("Player", "SlinkySegment")))
        {
            float heightDiff = surfaceHit.point.y - transform.position.y;
            if (heightDiff > 0.01f)
            {
                playerRb.MovePosition(playerRb.position + Vector3.up * (heightDiff + 0.01f));
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCollider == null) return;

        CapsuleCollider capsule = (CapsuleCollider)playerCollider;
        Vector3 bottom = new Vector3(
            capsule.bounds.center.x,
            capsule.bounds.min.y + capsule.radius,
            capsule.bounds.center.z
        );

        // Ground check spheres
        Gizmos.color = isGrounded ? Color.green : Color.blue;
        Gizmos.DrawWireSphere(bottom, capsule.radius * 0.9f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bottom + Vector3.down * playerControllerData.groundCheckDistance, capsule.radius * 0.9f);

        // Forward direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(capsule.bounds.center, transform.forward * 1.5f);
        if (slinkyDirection != Vector3.zero)
        {
            //Player rotation cone edge
            Gizmos.color = Color.blue;
            Vector3 coneCenter = -slinkyDirection;
            coneCenter.y = 0;
            coneCenter.Normalize();

            Gizmos.DrawLine(capsule.bounds.center,
                capsule.bounds.center + ((Quaternion.Euler(0, coneAngle, 0) * coneCenter) * 10));
            Gizmos.DrawLine(capsule.bounds.center,
                capsule.bounds.center + ((Quaternion.Euler(0, -coneAngle, 0) * coneCenter) * 10));

            // Cone center
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(capsule.bounds.center, capsule.bounds.center + coneCenter * 10);
        }
    }


    // Public getters
    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    public bool GetIsStunned()
    {
        return isStunned;
    }

    public bool GetIsMagnetized()
    {
        return isMagnetized;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public float GetMovementSpeed()
    {
        return moveSpeed;
    }

    public Vector2 GetPlayerInput()
    {
        return moveInput;
    }

    public Vector3 GetVelocity()
    {
        return playerRb.linearVelocity;
    }

    public float GetPlayerGravity()
    {
        return jumpGravity;
    }

    public bool IsBigChomping()
    {
        return isBigChomping;
    }

    public bool IsOnHandle()
    {
        return isOnHandle;
    }

    public bool IsSlingshotting()
    {
        return isSlingshotting;
    }

    //Public setters
    
    public void ChompHandle(bool value)
    {
        isOnHandle = value;
        
        if (!value)
        {
            StartCoroutine(ResetRotation());
            
        }
        else
        {
            playerRb.freezeRotation = !value;
        }
    }

    public void SetIsMagnetized(bool value)
    {
        isMagnetized = value;
    }

    public void BigChomping()
    {
        isBigChomping = true;
    }

    public void BigChomped()
    {
        isBigChomping = false;
    }

    public void StartSlingshot(float airControlMultiplier)
    {
        
        movementMultiplier = airControlMultiplier;
        isSlingshotting = true;
        _slingshotStartFrame = Time.frameCount;
        OnSlingshot?.Invoke();
    }

    public void EndSlingshot()
    {
        
        movementMultiplier = 1f;
        isSlingshotting = false;
        playerRb.linearVelocity = Vector3.zero;
        //zach added
        OnEndSlingshot?.Invoke();
    }

    // Coroutines — modify runtime local variables, never the SO
    private IEnumerator ApplyStun(float waitTime)
    {
        isStunned = true;
        yield return new WaitForSeconds(waitTime);
        isStunned = false;
    }

    private IEnumerator ApplySpeedEffectSequence(float effectMultiplier, float waitTime)
    {
        maxSpeed *= effectMultiplier;
        moveSpeed *= effectMultiplier;
        yield return new WaitForSeconds(waitTime);
        ResetSpeed();
    }

    private IEnumerator ApplyTurnEffectSequence(float effectMultiplier, float waitTime)
    {
        smoothTurnSpeed *= effectMultiplier;
        yield return new WaitForSeconds(waitTime);
        ResetTurnEffect();
    }

    private IEnumerator ApplyJumpEffectSequence(float effectMultiplier, float waitTime)
    {
        jumpForce *= effectMultiplier;
        RecalculateJumpGravity();
        yield return new WaitForSeconds(waitTime);
        ResetJumpEffect();
    }
    private IEnumerator ResetRotation()
    {
        
        Quaternion initialRotation = playerRb.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, initialRotation.eulerAngles.y, 0);
        
        float t = 0f;
        while (t < 1f)
        {
            t+=Time.deltaTime;
            playerRb.MoveRotation(Quaternion.Slerp(initialRotation, targetRotation, t));
            yield return null;
        }
        playerRb.MoveRotation(targetRotation);
        playerRb.freezeRotation = true;
    }
    
    
    /** Vibration Functions */
    
    public IEnumerator ExplosionRumble()
    {
        Gamepad thisGamepad = GetPlayerGamepad();
        
        if (thisGamepad != null)
        {
            thisGamepad.SetMotorSpeeds(playerControllerData.vibrationExplosionLowIntensity, playerControllerData.vibrationExplosionHighIntensity);
                    yield return new WaitForSeconds(playerControllerData.vibrationExplosionTime);
                    
                    thisGamepad.SetMotorSpeeds(0,0);
        }
        
    }
    private void ExplosionRumbleStart()
    {
        StartCoroutine(ExplosionRumble());
    }
    
    private void ChompVibration(Vector3 arg1, GameObject arg2)
    {
        Gamepad thisGamepad = GetPlayerGamepad();
        
        if (thisGamepad != null)
        {
            thisGamepad.SetMotorSpeeds(playerControllerData.chompVibrationLowIntensity,
                playerControllerData.chompVibrationHighIntensity);
        }
    }
    private void StopChompVibration()
    {
        Gamepad thisGamepad = GetPlayerGamepad();
        if (thisGamepad != null)
        {
            thisGamepad.SetMotorSpeeds(0,0);
        }
    }
    private Gamepad GetPlayerGamepad()
    {
        if (playerInput == null) return null;
    
        foreach (var device in playerInput.devices)
        {
            if (device is Gamepad gamepad)
                return gamepad;
        }
        return null;
    }
    
    // Public methods
    public void StunPlayer(float duration)
    {
        StartCoroutine(ApplyStun(duration));
    }

    public void ApplySpeedEffect(float effectMultiplier, float duration)
    {
        if (speedModifierCoroutine != null)
        {
            StopCoroutine(speedModifierCoroutine);
            ResetSpeed();
        }

        speedModifierCoroutine = StartCoroutine(ApplySpeedEffectSequence(effectMultiplier, duration));
    }

    public void ApplyTurnEffect(float effectMultiplier, float duration)
    {
        if (turnModifierCoroutine != null)
        {
            StopCoroutine(turnModifierCoroutine);
            ResetTurnEffect();
        }

        turnModifierCoroutine = StartCoroutine(ApplyTurnEffectSequence(effectMultiplier, duration));
    }

    public void ApplyJumpEffect(float effectMultiplier, float duration)
    {
        if (jumpModifierCoroutine != null)
        {
            StopCoroutine(jumpModifierCoroutine);
            ResetJumpEffect();
        }

        jumpModifierCoroutine = StartCoroutine(ApplyJumpEffectSequence(effectMultiplier, duration));
    }


    public void ResetSpeed()
    {
        maxSpeed = initialMaxSpeed;
        moveSpeed = initialSpeed;
    }

    public void ResetTurnEffect()
    {
        smoothTurnSpeed = initialSmoothTurnSpeed;
    }

    public void ResetJumpEffect()
    {
        jumpForce = initialJumpForce;
        RecalculateJumpGravity();
    }

    public void CantMove()
    {
        playerRb.isKinematic = true;
        isStunned = true;
    }

    public void CanMove()
    {
        playerRb.isKinematic = false;
        isStunned = false;
    }

    public void ApplySlingShotVelocity(float force, Vector3 direction)
    {
        Vector3 velocity = direction.normalized * force;
        playerRb.linearVelocity = Vector3.zero; // Reset current velocity before applying slingshot velocity
        playerRb.AddForce(velocity, ForceMode.VelocityChange);
    }
    
    public void ApplyForceVelocity(float force, Vector3 direction)
    {
        Vector3 velocity = direction.normalized * force;

        playerRb.AddForce(velocity, ForceMode.Acceleration);
    }


    public void SetSlinkyPlayerDirection(Vector3 direction)
    {
        slinkyDirection = direction;
    }

    //Animations
    void UpdateAnimation()
    {
        //movement and strafing
        Vector3 localVelocity = transform.InverseTransformDirection(new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z));

        float speed = Mathf.Clamp(localVelocity.z, -1f, 1f); 
        float directions = Mathf.Clamp(localVelocity.x, -1f, 1f); 

        Vector3 input = GetPlayerInput();

        if (input != Vector3.zero)
        {
            anim.SetFloat("Speed",      speed);
            anim.SetFloat("Directions", directions);
        }
        else
        {
            anim.SetFloat("Speed",      0);
            anim.SetFloat("Directions", 0);
        }
        
        //jumping falling
        anim.SetBool("IsGrounded", GetIsGrounded());
    }
    
    public void ClampVelocityAtDistance(Vector3 outwardDir)
    {
        if(!usingMaxClamp) return;
        
        // Grace period: skip clamp for first frames of slingshot
        if (isSlingshotting && (Time.frameCount - _slingshotStartFrame) < 100) return;

        Vector3 playerVelocity = playerRb.linearVelocity;
        Vector3 outNorm = outwardDir.normalized;
        float dot = Vector3.Dot(playerVelocity, outNorm);

        if (dot > 0)
        {
            playerRb.linearVelocity -= outNorm * dot;
            
        }
            
        
    }

    public void SetMaxClamp(bool value)
    {
        usingMaxClamp = value;
    }
}  