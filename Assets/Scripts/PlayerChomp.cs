using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.VFX;

struct ChompHitInfo
{
    public Collider col;
    public Vector3 pos;
}
public class PlayerChomp : MonoBehaviour
{
    private Animator anim;
    [Header("PlayerBrain")]
    private PlayerBrain playerBrain;

    [Header("Chomp")]
    [SerializeField] private float firstRaycast = 2;
    public Transform ChompHoldingSpot;
    [SerializeField] private Transform[] biteHoldingSpot;
    public Transform ChompFromSpot;
    [SerializeField] private LayerMask raycastMask;
    [Tooltip("Default chomp value si le chomp object ne le override pas")]
    public ChompableSpringValue defaultChompValue;
    public ChompRuleSet chompRuleSet;
    private Vector3 chompHitPoint;
    public GameObject ChompedObject;
    
    [HideInInspector]
    public Joint chompJoint;

    [Header("bark")] [SerializeField] private float barkRadius = 5;
    public ChompableSpringValue chompEverythingValue;
    [SerializeField] private float barkColdown = 0.5f;
    private float lastBarkTime = 999f;
    [Header("FOV chomp")]
    [SerializeField] private float chompSampleRadius = 5f;
    [SerializeField] private float fovAngle = 90f;

    private bool Chomping = false;
    private PlayerVFXPlayer VFXPlayer;
    
    private Vector3 chompInitialDirection;
    private bool WasObjectKinetic = false;
    public bool lockPlayerDegree;

    private Rigidbody playerRb;
    //event
    public event Action onChompEvent;
    /// <summary>
    /// notify when an object is chomped
    /// </summary>
    /// <param name="Hitpoint"></param>
    /// <param name="hit object"></param>
    public event Action<Vector3, GameObject> onChompHit;
    public event Action onReleaseChompedEvent;

    private float originalObjectMass;
    private float originalObjectAngularDrag;
    
    private SlinAndKyControllerBase controller;
    private StayUpOnoController  stayUpOnoController;
    private SlinkyManager slinkyManager;

    
    //debnugg
    [Header("debugs")]
    [SerializeField] private bool debugChomp = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (defaultChompValue == null)
        {
            Debug.LogError("NO CHOMP VALUE ASSIGN. Go in both player chomp script and assign baseValue or w.e SO you want");
        }

        if (biteHoldingSpot.Length != Enum.GetValues(typeof(AttachmentLocation)).Length)
        {
            //Debug.LogError("biteHoldingSpot IS NOT EQUAL TO THE AttachmentLocation ENUM NUMBER OF LOCATION");
        }
        
        VFXPlayer =  GetComponent<PlayerVFXPlayer>();
    }

    void Start()
    {
        playerBrain = GetComponent<PlayerBrain>();

        if (ChompHoldingSpot == null || ChompFromSpot == null)
        {
            Debug.LogError("NO CHOMPHOLDINGSPOT OR CHOMPFROMSPOT ASSIGN THEM NOWWWWWWWWWWWWWW");
        }
        
        anim = GetComponentInChildren<Animator>();
        playerRb = GetComponent<Rigidbody>();
        controller = GetComponent<SlinAndKyControllerBase>();
        slinkyManager = FindFirstObjectByType<SlinkyManager>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();

        /*
        if(controller ==null|| slinkyManager == null) return;
        
        if (IsChomping()  && (controller.IsSlingshotting() || (slinkyManager.IsAtMaxDistance() && !controller.GetIsGrounded())) )
        {
            if (ChompedObject != null)
            {
                Rigidbody objRb = ChompedObject.GetComponent<Rigidbody>();
                if (objRb != null)
                {
                    objRb.linearVelocity = Vector3.zero;
                }
                
            }
            ChompRelease();
            playerRb.linearVelocity = Vector3.zero;
        }
        */
    }

    private void UpdateAnimation()
    {
        Vector3 input = controller.GetPlayerInput();

        if (ChompedObject != null && input != Vector3.zero)
        {
            bool isPulling = false;
            if (chompJoint)
            {
                if (chompJoint is SpringJoint sj)
                {
                    Vector3 playerAnchor = transform.TransformPoint(sj.anchor);
                    Vector3 objectAnchor = sj.connectedBody != null ? sj.connectedBody.transform.TransformPoint(sj.connectedAnchor) : sj.connectedAnchor;

                    float currentDist = Vector3.Distance(playerAnchor, objectAnchor);
                    isPulling = currentDist > sj.minDistance + 0.1f;
                }
            }
            

            anim.SetBool("IsPulling", isPulling);
        }
        else
        {
            anim.SetBool("IsPulling", false);
        }
    }

    private void FixedUpdate()
    {
        /*
        if (chompJoint == null) return;
        IChompable chompable = ChompedObject.GetComponent<IChompable>();
        if (chompable == null) return;
        if (ChompedObject != null)
        {
            AttachmentConfig config = chompable.GetAttachmentConfig();
            if (config != null)
            {
                if (ChompedObject.GetComponent<IChompable>().GetAttachmentConfig() is SpringAttachmentConfig springConfig &&
                    springConfig.CannotRotateAround)
                {
                    Vector3 worldAnchor = chompJoint.connectedBody != null
                        ? chompJoint.connectedBody.transform.TransformPoint(chompJoint.connectedAnchor)
                        : chompJoint.connectedAnchor;

                    Vector3 toPlayer = transform.position - worldAnchor;
                    float angle = Vector3.Angle(chompInitialDirection, toPlayer.normalized);

                    if (angle > 90f) 
                    {
                        Rigidbody playerRb = GetComponent<Rigidbody>();
        
                        Vector3 clampedDir = Vector3.RotateTowards(toPlayer.normalized, chompInitialDirection, Mathf.Deg2Rad * (angle - 90f), 0f);
                        transform.position = worldAnchor + clampedDir * toPlayer.magnitude;
        
                        Vector3 axis = Vector3.Cross(chompInitialDirection, toPlayer).normalized;
                        Vector3 tangent = Vector3.Cross(axis, toPlayer.normalized).normalized;

                        float badVelocity = Vector3.Dot(playerRb.linearVelocity, tangent);
                        if (badVelocity > 0f)
                        {
                            playerRb.linearVelocity -= tangent * badVelocity;
                        }
                    }
                }
            }
        }
        */
    }

    public void onChomp(InputAction.CallbackContext context)
    {
        if (debugChomp)
        {
            if(context.performed)
            {
                if (!AlreadyBiting())
                {
                    Chomp();
                }
                return;
            }
            
            if (context.started)
            {
                ChompRelease();
            }

        }
        else
        {
            if (context.started)
            {
            
            }
            if(context.performed)
            {
                Chomp();
            }

            if (context.canceled)
            {
                ChompRelease();
            }
        }
    }

    void Chomp()
    {
        if (AlreadyBiting()) return;
        anim.SetTrigger("Bite");
        onChompEvent?.Invoke();
        Collider[] hitObjects = Physics.OverlapSphere(ChompFromSpot.position, chompSampleRadius);
        
        ChompHitInfo bestChompHitInfo = default;
        
        foreach (var rule in chompRuleSet.rules)
        {
            RaycastHit hitInfo;
            bool rayHit = Physics.Raycast(
                transform.position,
                transform.forward,
                out hitInfo,
                firstRaycast,
                raycastMask
            );

            if (hitInfo.collider != null)
            {
                if (((1 << hitInfo.collider.gameObject.layer) & rule.layerMask) == 0)
                    continue;
                
                bestChompHitInfo.col =  hitInfo.collider;
                bestChompHitInfo.pos = hitInfo.point;
            }

            if (bestChompHitInfo.col != null)
            {
                //Debug.Log($"raycast {bestChompHitInfo.col.gameObject.name}");
                break;
            }
        }

        if (bestChompHitInfo.col == null)
        {
            foreach (var rule in chompRuleSet.rules)
            {
                bestChompHitInfo = FindBestHit(hitObjects, rule);

                if (bestChompHitInfo.col != null)
                {
                    //Debug.Log($"overlap {bestChompHitInfo.col.gameObject.name}");
                    break;
                }
                    
            }
        }
        
        
        if (bestChompHitInfo.col != null)
        {
            //Debug.Log(bestChompHitInfo.col.gameObject.name);
            Chomping = true;
            chompHitPoint = bestChompHitInfo.pos;
            Rigidbody rb = bestChompHitInfo.col.attachedRigidbody;
            IChompable chompable = bestChompHitInfo.col.GetComponent<IChompable>();
            
            ChompedObject = bestChompHitInfo.col.gameObject;
            if (rb != null && chompable != null)
            {
               
                onChompHit?.Invoke(bestChompHitInfo.pos, bestChompHitInfo.col.gameObject);
                
                chompable.OnChomped(gameObject);
                AttachmentType type = chompable.GetAttachmentType();
                AttachmentLocation location = chompable.GetAttachmentLocation();
                AttachmentConfig config = chompable.GetAttachmentConfig();
                /*
                switch (type)
                {
                    case AttachmentType.Spring:
                        ChompableSpringValue springVal = chompable.GetSpringValue();
                        AttachJoint(rb, bestChompHitInfo.col.transform, springVal, bestChompHitInfo.pos);
                        break;
                    case AttachmentType.CantMove:
                        GetComponent<SlinAndKyControllerBase>().CantMove();
                        break;
                    case AttachmentType.ChildTransform:
                        PrepareObjectToTransport(bestChompHitInfo.col.gameObject, location);
                        SetChildTransform(bestChompHitInfo.col.gameObject, location);
                        break;
                    case AttachmentType.NoAttachment:
                        ChompedObject = null;
                        break;
                }
                */
                
                switch (config)
                {
                    case SpringAttachmentConfig springConfig:
                        AttachJoint(rb, bestChompHitInfo.col.transform, springConfig.springValue, bestChompHitInfo.pos, springConfig.collisionWithPlayer);
                        break;
                    case ChildTransformAttachmentConfig childConfig:
                        PrepareObjectToTransport(bestChompHitInfo.col.gameObject, location);
                        SetChildTransform(bestChompHitInfo.col.gameObject, location);
                        break;
                    case CantMoveAttachmentConfig:
                        GetComponent<SlinAndKyControllerBase>().CantMove();
                        break;
                    case NoAttachmentConfig:
                        ChompedObject = null;
                        break;
                    case HingeAttachmentConfig hingeConfig:
                        originalObjectMass = rb.mass;
                        originalObjectAngularDrag = rb.angularDamping;
                        AttachHingeTo(rb, bestChompHitInfo.col.transform,bestChompHitInfo.pos , hingeConfig);
                        break;
                    case AirHandleAttachmentConfig airHandleConfig:
                        GetComponent<SlinAndKyControllerBase>().ChompHandle(true);
                        AttachUniversalHingeJoint(rb, bestChompHitInfo.col.transform, bestChompHitInfo.pos,
                            airHandleConfig.damping, airHandleConfig.springForce, airHandleConfig.angleLimits,
                            airHandleConfig.swingAxis, airHandleConfig.collisionEnabled);
                        break;
                }
                
                
                chompable.OnSetupFinish(gameObject);
            }
            else
            {
                GetComponent<SlinAndKyControllerBase>().CantMove();
                //AttachToWorld(bestChompHitInfo.col);
            }

            GameObject go = VFXPlayer.PlayStayOnomatopeia(playerBrain.GetPlayerVFXBank().BiteSprite, VFXPlayer.aboveHead);
            go.GetComponent<StayUpOnoController>().owner = this;
            
            go.transform.SetParent(transform);
                
            //SoundManager.Instance.PlaySFX(chompSfx, GameObject.FindGameObjectWithTag("MainCamera").transform.position);
            //playerBrain.GetPlayerVFXBank().Bite?.Execute(transform.position, playerBrain.GetPlayerVFXBank().BiteSprite);
            //VFXFactory.Instance.SpawnParticleSystem("FloatingParticle", transform.position, Quaternion.identity, true, 2f);
            SoundManager.Instance.PlaySFX(playerBrain.GetPlayerSoundBank().Bite, transform.position);
            
            
            anim.SetBool("IsHolding", true);
        }
        else
        {
            Bark();
        }
    }

    private void Bark()
    {
        //playerBrain.GetPlayerVFXBank().BiteEmpty?.Execute(transform.position, playerBrain.GetPlayerVFXBank().Bark);
        
            
        Collider[] barkHit = Physics.OverlapSphere(transform.position, barkRadius);

        foreach (Collider hit in barkHit)
        {
            IListeneable listener = hit.gameObject.GetComponent<IListeneable>();
            if(listener == null) continue;
                
            listener.OnHearingBark();
        }

        if (math.abs(lastBarkTime - Time.time) > barkColdown)
        {
            SoundManager.Instance.PlaySFX(playerBrain.GetPlayerSoundBank().BiteEmpty, transform.position);
            VFXPlayer.PlayOnomatopeia(playerBrain.GetPlayerVFXBank().Bark, VFXPlayer.aboveHead);
            lastBarkTime = Time.time;
        }
        
    }

    private bool AlreadyBiting()
    {
        if (chompJoint != null)
        {
            if (ChompedObject != null)
            {
                IChompable chompable = ChompedObject.GetComponent<IChompable>();
                if (chompable != null)
                {
                    chompable.OnReleased(gameObject);
                }
            }

            Destroy(chompJoint);
            ChompedObject = null;
            return true;
        }

        return false;
    }

    void PlayerCantMove()
    {
        
    }

    void PrepareObjectToTransport(GameObject go, AttachmentLocation location)
    {
        Rigidbody rb = go.GetComponent<Rigidbody>();
        WasObjectKinetic = rb.isKinematic;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        go.GetComponent<Collider>().isTrigger = true;
        
        go.transform.position = ChompHoldingSpot.transform.position;
    }
    void SetChildTransform(GameObject child, AttachmentLocation location)
    {
        child.transform.SetParent(ChompHoldingSpot);
    }

    void OnReleaseChild(GameObject go)
    {
        ChompedObject.transform.SetParent(null);
        Rigidbody rb = go.GetComponent<Rigidbody>();
        rb.isKinematic = WasObjectKinetic;
        rb.linearVelocity = Vector3.zero;
        go.GetComponent<Collider>().isTrigger = false;
    }
    
    Vector3 GetClosestPoint(Collider col, Vector3 position)
    {
        if (col is MeshCollider meshCollider && !meshCollider.convex)
            return col.ClosestPointOnBounds(position);

        return col.ClosestPoint(position);
    }

    ChompHitInfo FindBestHit(Collider[] hits, ChompRule rule)
    {
        float bestDist = float.MaxValue;
        ChompHitInfo best = default;
        
        foreach (var hit in hits)
        {
            if (((1 << hit.gameObject.layer) & rule.layerMask) == 0)
                continue;

            Vector3 point = GetClosestPoint(hit, transform.position);
            
            if (rule.useFOV && !IsInFOV(point))
                continue;

            if (rule.requiresChompable && hit.GetComponent<IChompable>() == null)
                continue;
            
            float dist = Vector3.Distance(transform.position, point);

            IChompable chompable = hit.GetComponent<IChompable>();
            float maxDist = rule.GetMaxDistance(chompable);
            
            if (dist > maxDist)
                continue;

            Vector3 direction = (point - transform.position).normalized;

            RaycastHit hitInfo;
            bool rayHit = Physics.Raycast(
                transform.position,
                direction,
                out hitInfo,
                chompSampleRadius,
                raycastMask,
                QueryTriggerInteraction.Ignore
            );

            if (!rayHit)
                continue;
            
            if (hitInfo.collider.gameObject != hit.gameObject) continue;
            
            if (Vector3.Dot(direction, Vector3.up) < -0.5)
                continue;

            if (dist < bestDist)
            {
                bestDist = dist;
                best.col = hitInfo.collider;
                best.pos = hitInfo.point;
                Debug.DrawLine(transform.position, hitInfo.point, Color.green, 5f);
            }
        }

        return best;
    }
    
    void ChompRelease()
    {
        onReleaseChompedEvent?.Invoke();
        anim.SetBool("IsHolding", false);
        anim.SetBool("IsPulling", false);
        
        if (ChompedObject != null)
        {
            IChompable chompable = ChompedObject.GetComponent<IChompable>();
            if (chompable != null)
            {
                /*
                AttachmentType type = chompable.GetAttachmentType();   
                switch (type)
                {
                    case AttachmentType.Spring:
                        break;
                    case AttachmentType.CantMove:
                        GetComponent<SlinAndKyControllerBase>().CanMove();
                        break;
                    case AttachmentType.ChildTransform:
                        ChompedObject.transform.SetParent(null);
                        break;
                    case AttachmentType.NoAttachment:
                        break;
                }*/
                AttachmentConfig config = chompable.GetAttachmentConfig();
                switch (config)
                {
                    case SpringAttachmentConfig springConfig:
                        break;
                    case ChildTransformAttachmentConfig childConfig:
                        OnReleaseChild(ChompedObject);
                        break;
                    case CantMoveAttachmentConfig:
                        GetComponent<SlinAndKyControllerBase>().CanMove();
                        break;
                    case NoAttachmentConfig:
                        break;
                    case HingeAttachmentConfig hingeConfig:
                        OnHingeRemoval();
                        break;
                    case AirHandleAttachmentConfig airHandleConfig:
                        GetComponent<SlinAndKyControllerBase>().ChompHandle(false);
                        break;
                }
                chompable.OnReleased(gameObject);
            }
            else
            {
                GetComponent<SlinAndKyControllerBase>().CanMove();
                GetComponent<SlinAndKyControllerBase>().ResetSpeed();
            }
        }
        if (chompJoint != null)
        {
            Destroy(chompJoint);
        }
        
        
        ChompedObject = null;
        Chomping = false;
        chompHitPoint = Vector3.zero;
    }
    
    void AttachToWorld(Collider hit)
    {
        SpringJoint sj = gameObject.AddComponent<SpringJoint>();
        sj.connectedBody = null;
        sj.autoConfigureConnectedAnchor = false;
        sj.anchor = transform.InverseTransformPoint(ChompHoldingSpot.position);
        sj.connectedAnchor = GetClosestPoint(hit, transform.position);

        sj.spring = chompEverythingValue.spring;
        sj.damper = chompEverythingValue.damper;
        sj.maxDistance = chompEverythingValue.maxDistance;
        sj.minDistance = chompEverythingValue.minDistance;
        sj.breakForce = chompEverythingValue.breakForce;
        sj.enableCollision = true;
        chompJoint = sj;
    }

    bool IsInFOV(Vector3 worldPoint)
    {
        Vector3 dirToTarget = (worldPoint - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);
        return angle <= fovAngle / 2;
    }


    public void RemoveJoint()
    {
        if (chompJoint != null)
        {
            IChompable chompable = ChompedObject.GetComponent<IChompable>();
            chompable.OnReleased(gameObject);
            
            Destroy(chompJoint);
            ChompedObject = null;
            return;
        }
    }

    private void AttachJoint(Rigidbody rb, Transform target, ChompableSpringValue spring, Vector3 hitPoint, bool collisionEnabled = true)
    {
        if (chompJoint != null)
        {
            Destroy(chompJoint);
        }
        SpringJoint sj = gameObject.AddComponent<SpringJoint>();
        if (spring != null)
        {
            
            sj.connectedBody = rb;
            sj.autoConfigureConnectedAnchor = false;
            sj.anchor = transform.InverseTransformPoint(ChompHoldingSpot.position);
            sj.connectedAnchor = target.InverseTransformPoint(hitPoint); 
            sj.spring = spring.spring;
            sj.damper = spring.damper;
            sj.maxDistance = spring.maxDistance;
            sj.minDistance = spring.minDistance;
            sj.breakForce = spring.breakForce;
            sj.enableCollision = collisionEnabled;
            
            chompJoint = sj;
        }
        else
        {
            sj.connectedBody = rb;
            sj.autoConfigureConnectedAnchor = false;
            sj.anchor = transform.InverseTransformPoint(ChompHoldingSpot.position);
            sj.connectedAnchor = target.InverseTransformPoint(ChompHoldingSpot.position);
            sj.spring = defaultChompValue.spring;
            sj.damper = defaultChompValue.damper;
            sj.maxDistance = defaultChompValue.maxDistance;
            sj.minDistance = defaultChompValue.minDistance;
            sj.breakForce = defaultChompValue.breakForce;
            sj.enableCollision = collisionEnabled;
            chompJoint = sj;
        }
        
        Vector3 worldAnchor = target.TransformPoint(chompJoint.connectedAnchor);
        chompInitialDirection = (transform.position - worldAnchor).normalized;
    }

    private void AttachHingeTo(Rigidbody rb, Transform target, Vector3 hitPoint, HingeAttachmentConfig hingeConfig, bool collisionEnabled = true)
    {
        //TODO MY HINGE
        
        //joint settup
        rb.mass = hingeConfig.mass;
        
        
        HingeJoint hj = gameObject.AddComponent<HingeJoint>();
        hj.connectedBody = rb;
        hj.axis = new Vector3(0, 1, 0);
        hj.anchor = transform.InverseTransformPoint(ChompHoldingSpot.position);
        hj.connectedAnchor = target.InverseTransformPoint(hitPoint);
        hj.enableCollision = false;
        
        //spring setup
        hj.useSpring = true;
        JointSpring js = hj.spring;
        js.targetPosition = 0f;
        js.spring = hingeConfig.springForce;
        js.damper = hingeConfig.damping;
        hj.spring = js;
        
        //limits setup
        hj.useLimits = true;
        JointLimits limits = hj.limits;
        limits.min = -hingeConfig.angleLimits;
        limits.max = hingeConfig.angleLimits;
        hj.limits = limits;
        chompJoint = hj;
    }
    
    //J-F Added this function
    private void AttachUniversalHingeJoint(Rigidbody rb, Transform target, Vector3 hitPoint, float damping, float springForce,
        float angleLimits, Vector3 swingAxis, bool collisionEnabled)
    {
        HingeJoint hj = gameObject.AddComponent<HingeJoint>();
        hj.connectedBody = rb;
        hj.axis = swingAxis;
        hj.autoConfigureConnectedAnchor = false;
        hj.anchor = transform.InverseTransformPoint(ChompHoldingSpot.position);
        hj.connectedAnchor = target.InverseTransformPoint(hitPoint);
        hj.enableCollision = collisionEnabled;
        
        
        //spring setup
        hj.useSpring = true;
        JointSpring js = hj.spring;
        js.targetPosition = 0f;
        js.spring = springForce;
        js.damper = damping;
        hj.spring = js;
        
        //limits setup
        hj.useLimits = true;
        JointLimits limits = hj.limits;
        limits.min = -angleLimits;
        limits.max = angleLimits;
        hj.limits = limits;
        chompJoint = hj;
    }
    //J-F NOT HERE
    
    
    private void OnHingeRemoval()
    {
        
        Rigidbody rb = ChompedObject.GetComponent<Rigidbody>();
        rb.mass = originalObjectMass;
        rb.angularDamping = originalObjectAngularDrag;
    }
    
    public void SetCantMove(bool value)
    {
        if(value)
        {
            GetComponent<SlinAndKyControllerBase>().CantMove();
        }
        else
        {
            GetComponent<SlinAndKyControllerBase>().ResetSpeed();
            GetComponent<SlinAndKyControllerBase>().CanMove();
        }
    }

    public void AttachJointTo(Rigidbody targetRb, ChompableSpringValue springVal, bool collisionEnabled = true)
    {
        AttachJoint(targetRb, targetRb.transform, springVal, targetRb.transform.position, collisionEnabled);
        ChompedObject = targetRb.gameObject;
    }
    public bool IsChomping()
    {
        return Chomping;
    }
    
    private void OnDrawGizmos()
    {
        if (chompJoint != null)
        {
            Vector3 worldAnchor;
            if (chompJoint.connectedBody != null)
            {
                worldAnchor = chompJoint.connectedBody.transform.TransformPoint(chompJoint.connectedAnchor);
            }
            else
            {
                worldAnchor = chompJoint.connectedAnchor; 
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(worldAnchor, 0.2f);
            
            Vector3 playerAnchor = transform.TransformPoint(chompJoint.anchor);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerAnchor, 0.2f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(playerAnchor, worldAnchor);
        }
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * firstRaycast);
        
        if (chompHitPoint != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(chompHitPoint, 0.3f);
        }
    }
}
