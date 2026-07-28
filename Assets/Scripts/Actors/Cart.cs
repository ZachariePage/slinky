using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

public class Cart : ActivationMechanism
{
    [Header("Spline")]
    [SerializeField] private SplineContainer spline;

    [Range(0f, 1f)]
    [Tooltip("starting location of minecart on the puzzle MINECRAFTTTTTTT")]
    [SerializeField] private float t;

    [Tooltip("how much the minecart move MINECRAFTTTTTTTTTTTTTTTTT")]
    [SerializeField] private float stepSize = 0.001f;

    [Header("Push Settings")]
    [Range(-0.5f, -0.2f)]
    [Tooltip("see if player is behind by how much")]
    [SerializeField] private float pushThreshold = -0.3f;
    
    [Header("Modifiers")]
    public float forwardMultiplier = 2f;
    [SerializeField] private float forwardCheckOffset = 1f;
    [SerializeField] private float verticalCheckOffset = 0.2f;
    [SerializeField] private LayerMask includeMask;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip movingSound;
    [SerializeField] private AudioClip blockedSound;

    [SerializeField] private float blockedCooldown = 10f;

    private float blockedTimer = 0f;

    private Vector3 boxSize;
    private List<Collider> frontHits = new List<Collider>();
    private List<Collider> rearHits  = new List<Collider>();

    void UpdateBoxSize()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;

        boxSize = Vector3.Scale(col.size, transform.lossyScale);
        boxSize.z *= forwardMultiplier;
    }

    protected override void Start()
    {
        UpdateBoxSize();
        UpdateCartPosition();
    }

    protected override void Update()
    {
        base.Update();
        
        if (blockedTimer > 0)
        {
            blockedTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        Vector3 frontCenter = transform.position
            + transform.forward * (boxSize.z * forwardCheckOffset)
            + Vector3.up * verticalCheckOffset;

        frontHits.Clear();
        foreach (Collider col in Physics.OverlapBox(frontCenter, boxSize * 0.5f, transform.rotation, includeMask, QueryTriggerInteraction.Ignore))
        {
            if (col.gameObject == gameObject) continue;
            frontHits.Add(col);
        }

        Vector3 rearCenter = transform.position
            - transform.forward * (boxSize.z * forwardCheckOffset)
            + Vector3.up * verticalCheckOffset;

        rearHits.Clear();
        foreach (Collider col in Physics.OverlapBox(rearCenter, boxSize * 0.5f, transform.rotation, includeMask, QueryTriggerInteraction.Ignore))
        {
            if (col.gameObject == gameObject) continue;
            rearHits.Add(col);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (activated) return;
        
        if (!other.CompareTag("Player")) return;

        Vector3 tangent = spline.EvaluateTangent(t);
        tangent = tangent.normalized;
        Vector3 toPlayer = (other.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(tangent, toPlayer);
        
        if (dot < pushThreshold)
        {
            if (frontHits.Count > 0)
            {
                PlayBlockedSound();
                return;
            }
            MoveForward();
        }

        else if (dot > -pushThreshold)
        {
            if (rearHits.Count > 0)
            {
                PlayBlockedSound();
                return;
            }
            MoveBackward();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (activated) return;

        if (other.gameObject.CompareTag("Player"))
        {
            // todo: add player bounce
        }
    }
    void MoveForward()
    {
        PlayMovingSound();

        t = Mathf.Clamp01(t + stepSize);
        UpdateCartPosition();

        if (t >= 1f)
        {
            Activate();
            audioSource.Stop();
        }
    }

    void MoveBackward()
    {
        PlayMovingSound();

        t = Mathf.Clamp01(t - stepSize);
        UpdateCartPosition();
        
        if (t <= 0f && audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void UpdateCartPosition()
    {
        if (spline != null)
        {
            transform.position = spline.EvaluatePosition(t);
            transform.rotation = Quaternion.LookRotation(spline.EvaluateTangent(t));
        }
        
    }

    void PlayMovingSound()
    {
        if (audioSource == null || movingSound == null) return;
        if (!audioSource.isPlaying)
        {
            audioSource.clip = movingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void PlayBlockedSound()
    {
        if (audioSource == null || blockedSound == null) return;
        
        if (audioSource.isPlaying && audioSource.clip == movingSound)
            audioSource.Stop();

        if (blockedTimer <= 0f)
        {
            audioSource.PlayOneShot(blockedSound);
            blockedTimer = blockedCooldown;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        UpdateBoxSize();

        Vector3 frontCenter = transform.position
            + transform.forward * (boxSize.z * forwardCheckOffset)
            + Vector3.up * verticalCheckOffset;

        Gizmos.color  = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(frontCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        Vector3 rearCenter = transform.position
            - transform.forward * (boxSize.z * forwardCheckOffset)
            + Vector3.up * verticalCheckOffset;

        Gizmos.color  = Color.blue;
        Gizmos.matrix = Matrix4x4.TRS(rearCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}