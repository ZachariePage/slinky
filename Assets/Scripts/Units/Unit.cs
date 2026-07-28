using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;

public class Unit : MonoBehaviour, IChompable,IDamageable
{
    public StateMachine stateMachine;

    public ChaseState chaseState;
    public AttackState attackState;
    public GrabbedState grabbedState;
    [Header("Unit")]
    [Header("State Configs")]
    [SerializeField] private ChaseStateSO chaseConfig;
    [SerializeField] private AttackStateSO attackConfig;
    [SerializeField] private GrabbedStateSO grabbedConfig;
    
    [Header("Chomp")]
    [SerializeField] private AttachmentConfig chompConfig;

    [SerializeField] private float stunDurationAfterChompRelease = 1f;
    [Header("agent")]
    [HideInInspector]
    public NavMeshAgent Agent;
    [SerializeField] public float speed = 2;
    [HideInInspector]
    public Rigidbody rb;
    private bool isStunned = false;

    [Header("Physics")]
    [Tooltip("Low Speed threshold where it will regain control")]
    [SerializeField] protected float RegainControlThreshold;

    public float JustGotGrabbedTimer = 5;

    [Header("anim")]
    public Animator anim;
    [SerializeField] public float attackDelay = 0f;


    [Header("Target")]
    public GameObject targetGO;
    public Vector3 target;
    
    [Header("Retroaction")]
    [SerializeField] private GameCue[] OnTakeDamageCues;
    [SerializeField] private GameCue[] onGrabbedCues;

    public float collisionTimer;
    public bool isContactingSlinky;
    
    private Coroutine contactingSlinkyRoutine;
    
    protected virtual void Initialize()
    {
        stateMachine.Init(chaseState);
    }

    public virtual void Start()
    {
        rb = GetComponent<Rigidbody>();

        anim = GetComponentInChildren<Animator>();
        
        Agent = GetComponent<NavMeshAgent>();

        Agent.speed = speed;
        targetGO = FindClosestPlayer();

        stateMachine = new StateMachine();
            
        chaseState = new ChaseState(this,  stateMachine, chaseConfig);
        attackState = new AttackState(this, stateMachine, attackConfig);
        grabbedState = new GrabbedState(this,  stateMachine, grabbedConfig);
        
        Initialize();
    }

    public GameObject FindClosestPlayer()
    {
        GameObject closestPlayer = null;
        float maxDistance = float.MaxValue;
        if (WorldManager.Instance != null)
        {
            foreach (GameObject player in WorldManager.Instance.Players)
            {
                if (player == null) continue;
            
                float distance = Vector3.SqrMagnitude(player.transform.position - transform.position);
                if (distance < maxDistance)
                {
                    maxDistance = distance;
                    closestPlayer = player;
                }
            }
        }
        
        
        return closestPlayer;
    }

    public virtual void Update()
    {
        float dt = Time.deltaTime;
        if (collisionTimer > 0)
        {
            collisionTimer -= Time.deltaTime;

            if (stateMachine.CurrentEnemyState != grabbedState)
            {
                Agent.enabled = false;     
                rb.isKinematic = false;
            }  
        }
        else
        {
            if (isContactingSlinky)
            {
                isContactingSlinky = false;
            }
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
            {
                rb.isKinematic = true;
                Agent.enabled = true;
            }    
        }
        
        if (JustGotGrabbedTimer > 0)
        {
            JustGotGrabbedTimer -= dt;
        }
        
        UpdateAnimation();
        
        stateMachine.CurrentEnemyState.FrameUpdate();
        
        stateMachine.CurrentEnemyState.EvaluateTransition();
    }
    
    public virtual void FixedUpdate()
    {
        stateMachine.CurrentEnemyState.PhysicUpdate();
    }

    public bool IsStun()
    {
        return isStunned;
    }

    private void PrepareForceAddition()
    {
        isStunned = true;
        rb.isKinematic = false;
        Agent.enabled = false;  
        rb.linearVelocity = Vector3.zero;
    }

    public void SetStun(float duration)
    {
        Debug.Log("hi stun");
        isStunned = true;
        if (Agent.enabled)
        {
            Agent.ResetPath();
        }
        StartCoroutine(ResetStun(duration));
    }
    
    public IEnumerator ResetStun(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (Agent.enabled)
        {
            Agent.ResetPath();
        }
        
        anim.SetBool("Stun", false);
        isStunned = false;
        yield return null;
    }

    public void AddForce(Vector3 force, ForceMode mode)
    {
        PrepareForceAddition();
        StartCoroutine(ResetKinematic());
        rb.AddForce(force, mode);
        Debug.Log("add force enemy");
    }
    
    public void AddForce(Vector3 force, float duration, ForceMode mode)
    {
        PrepareForceAddition();
        StartCoroutine(ResetKinematic());
        rb.AddForce(force);
    }

    public IEnumerator ResetKinematic(float waitTime)
    {
        Debug.Log("start esetr");
        yield return new WaitForSeconds(waitTime);
        Debug.Log("end esetr");
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        isStunned = false;
        Agent.enabled = true;  
        Agent.ResetPath();
        yield return null;
    }
    
    public IEnumerator ResetKinematic()
    {
        float timeout = 10f;
        float timer = 0f;
        yield return new WaitForSeconds(0.5f);
        Debug.Log(rb.linearVelocity.magnitude );
        while (rb.linearVelocity.magnitude > RegainControlThreshold && timer < timeout)
        { 
            timer += Time.deltaTime;
            yield return null;
            
        }
        
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        isStunned = false;
        Agent.enabled = true;  
        Agent.isStopped = false;
        Agent.ResetPath();
        
        yield return null;
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    //aniamtions
    void UpdateAnimation()
    {
        anim.SetFloat("Speed", Agent.velocity.magnitude);
    }

    public void OnChomped(GameObject chomper)
    {
        stateMachine.ChangeState(grabbedState);
        stateMachine.CurrentEnemyState.OnChomped(chomper);
        
        anim.SetTrigger("Hit");
        anim.SetBool("Stun", true);
        foreach (GameCue c in onGrabbedCues)
        {
            c?.Execute(transform.position);
        } 
    }

    public void OnReleased(GameObject chomper)
    {
        stateMachine.CurrentEnemyState.OnChompedRelease(chomper);
        stateMachine.ChangeState(chaseState);
        
        SetStun(stunDurationAfterChompRelease);
        JustGotGrabbedTimer = 5;
    }

    public bool AllowsAttachment()
    {
        return false;
    }

    public AttachmentType GetAttachmentType()
    {
        return AttachmentType.ChildTransform;
    }

    public ChompableSpringValue GetSpringValue()
    {
        return null;
    }

    public void OnSetupFinish(GameObject chomper)
    {
        
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        stateMachine.CurrentEnemyState.OnUnitCollisionEnter(other);
        
        if (other.gameObject.layer == LayerMask.NameToLayer("SlinkySegment"))
        {
            if (contactingSlinkyRoutine != null)
            {
                //StopCoroutine(contactingSlinkyRoutine);
                //contactingSlinkyRoutine = null;
            }

            //PrepareForceAddition();
            //contactingSlinkyRoutine = StartCoroutine(ResetKinematic(0.5f));
            //isContactingSlinky = true;
            collisionTimer = 0.5f;
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("SlinkySegment"))
        {
            if (contactingSlinkyRoutine != null)
            {
                //StopCoroutine(contactingSlinkyRoutine);
                //contactingSlinkyRoutine = null;
            }

            //PrepareForceAddition();
            //contactingSlinkyRoutine = StartCoroutine(ResetKinematic(0.5f));
            //isContactingSlinky = true;
            collisionTimer = 0.5f;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        stateMachine.CurrentEnemyState.OnUnitTriggerEnter(other);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 30);
    }

    public AttachmentLocation GetAttachmentLocation()
    {
        return AttachmentLocation.location1;
    }

    public AttachmentConfig GetAttachmentConfig()
    {
        return chompConfig;
    }

    public void TakeDamage(DamageInfo info)
    {
        if (isStunned)
        {
            return;
        }
        foreach (GameCue c in OnTakeDamageCues)
        {
            c?.Execute(transform.position);
        } 
        SetStun(info.StunDuration);
    }


}
