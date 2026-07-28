using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
public enum DirectionSpace
{
    World,
    Local
}
public class Trampoline : MonoBehaviour
{
    [Header("Layer")]
    [SerializeField] protected LayerMask includeLayers;
    
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 14f;
    [Range(0.1f, 1f)]
    [SerializeField] private float forceEnemyReducerMultiplier = 0.5f;
    [Tooltip("If thing need a certain falling velocity on it")]
    [SerializeField] private float minImpactToBounce = 0f;
    [Tooltip("Coldown before a thing can bounce again")]
    [SerializeField] private float cooldownDuration = 0.15f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;
    
    [Header("Bounce Direction")]
    [SerializeField] private bool useBounceDirection = false;
    [Tooltip("If the bounce is using world direction or not")]
    [SerializeField] private DirectionSpace directionSpace = DirectionSpace.World;
    
    [Range(0,1)]
    [Tooltip("Amount of force in the bounceDirection. 0 is 100% up 0.5 is 50% up 50% direction 1 is 100% direction")]
    [SerializeField] private float pourcentageInThatDirection = 0f;
    
    [Range(0,30)]
    [Tooltip("The force in that direction will be multiplied by this amount. So if 0.5% of 10 = 5 then 5 force multiply by this")]
    [SerializeField] private float multiplierInDirection = 1f;
    [Tooltip("local direction")]
    [SerializeField] private Vector3 bounceDirection = Vector3.forward;

    private Dictionary<Rigidbody, float> cooldowns = new Dictionary<Rigidbody, float>();
    
    [Header("Retroactions")] 
    [SerializeField] protected GameCue[] bounce;

    private Animator anim;

    private Coroutine coroutineBounce;

    public bool animStop;

    public float overlapRadius = 2f;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (cooldowns.Count > 0)
        {
            var keys = new List<Rigidbody>(cooldowns.Keys);
            foreach (Rigidbody rb in keys)
            {
                cooldowns[rb] -= Time.deltaTime;
                if (cooldowns[rb] <= 0f)
                {
                    cooldowns.Remove(rb);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    private void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & includeLayers) == 0) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (animStop)
        {
            if (collision.gameObject.CompareTag("Player") && stateInfo.IsName("Spring_Activate"))
            {
                return;
            }
        }
        
        GameObject obj = collision.gameObject;
        ThrowObject(obj);

        if (animStop)
        {
            Collider[] hitCollider = Physics.OverlapSphere(
                transform.position,
                overlapRadius,
                LayerMask.GetMask("Player")
            );
            foreach (Collider col in hitCollider)
            {
                if (col.gameObject.CompareTag("Player") && col.gameObject != collision.gameObject)
                {
                    ThrowObject(col.gameObject); 
                }
            }
        }
    }

    private void ThrowObject(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return;
        if (cooldowns.ContainsKey(rb)) return;

        float verticalVelocity = rb.linearVelocity.y;
        
        if (verticalVelocity > -minImpactToBounce) return;

        float finalBounce = bounceForce + Mathf.Abs(verticalVelocity);

        Vector3 finalForce = Vector3.up * finalBounce;

        if (useBounceDirection)
        {
            Vector3 dir;

            if (directionSpace == DirectionSpace.World)
            {
                dir = bounceDirection.normalized;
            }
            else
            {
                dir = transform.TransformDirection(bounceDirection).normalized;
            }

            float upForce = finalBounce * (1f - pourcentageInThatDirection);
            float directionalForce = finalBounce * pourcentageInThatDirection;

            directionalForce *= multiplierInDirection;

            Vector3 upwardPart = Vector3.up * upForce;
            Vector3 directionalPart = dir * directionalForce;

            finalForce = upwardPart + directionalPart;
        }

        PlayerCue(bounce, rb.transform);

        if (obj.GetComponent<Unit>() != null)
        {
            obj.GetComponent<Unit>().AddForce(finalForce * forceEnemyReducerMultiplier,  ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(finalForce, forceMode);
        }
        
        SlinAndKyControllerBase controller = obj.GetComponent<SlinAndKyControllerBase>();
        if (controller != null)
        {
            Debug.Log("maxclamp");
            controller.SetMaxClamp(false);
            if (coroutineBounce != null)
            {
                StopAllCoroutines();
                coroutineBounce = null;
            }
            coroutineBounce = StartCoroutine(resetMaxClamp(controller));
        }
        cooldowns[rb] = cooldownDuration;

        if (anim != null)
        {
            anim.SetTrigger("Spring");
        }
    }

    void PlayerCue(GameCue[] cuesList, Transform location)
    {
        foreach (GameCue cue in cuesList)
        {
            cue?.Execute(location.position);
        }
    }

    IEnumerator resetMaxClamp(SlinAndKyControllerBase controller)
    { 
        yield return new WaitForSeconds(1f);
        Debug.Log("unset");
        controller.SetMaxClamp(true);
        coroutineBounce = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, overlapRadius);
    }
}