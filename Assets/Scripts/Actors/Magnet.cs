using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private ForceMode forceMode = ForceMode.Acceleration;
    [SerializeField] private Vector3 pushDirection;

    [Header("Single Rigidbody Settings")]
    public float singleBodyForce = 5f;
    public float singleBodyMaxSpeed = 6f;

    [Header("Slinky Settings")]
    public float slinkyForcePerBody = 20f;
    public float maxTotalForcePerSlinky = 200f;

    [Header("Modifiers")]
    public float movingAwayDiminisher = 0.5f;
    public float forwardMultiplier = 2f;

    private Vector3 boxSize;
    private HashSet<SlinAndKyControllerBase> magnetizedLastFrame = new();
    void UpdateBoxSize()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;

        boxSize = Vector3.Scale(col.size, transform.lossyScale);
        boxSize.z *= forwardMultiplier;
    }

    void FixedUpdate()
    {
        HashSet<SlinAndKyControllerBase> magnetizedThisFrame = new();
        
        UpdateBoxSize();

        Vector3 frontCenter = transform.position + transform.forward * (boxSize.z * 0.5f);

        Collider[] hits = Physics.OverlapBox(
            frontCenter,
            boxSize * 0.5f,
            transform.rotation,
            targetLayer
        );

        Dictionary<Transform, HashSet<Rigidbody>> bodiesPerRoot = new();
        
        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null || rb.isKinematic) continue;

            Transform root = rb.transform.root;

            if (!bodiesPerRoot.TryGetValue(root, out var set))
            {
                set = new HashSet<Rigidbody>();
                bodiesPerRoot[root] = set;
            }

            set.Add(rb); 
        }
        
        foreach (var entry in bodiesPerRoot)
        {
            var rbs = entry.Value;
            int count = rbs.Count;
            
            bool isSingle = (count == 1);
            float forcePerBody;
            if (isSingle)
            {
                forcePerBody = singleBodyForce;
            }
            else
            {
                forcePerBody = Mathf.Min(slinkyForcePerBody, maxTotalForcePerSlinky / count);
            }
            
            
            foreach (Rigidbody rb in rbs)
            {
               
                Vector3 vel = rb.linearVelocity;
                
                Vector3 dir = pushDirection.normalized;
                float forwardSpeed = Vector3.Dot(vel, dir);

                float alignment = Mathf.Sign(forwardSpeed);

                float strength = forcePerBody;

                if (rb.gameObject.CompareTag("Player"))
                {
                    
                }

                if (alignment < 0f)
                {
                    strength *= movingAwayDiminisher;
                }

                rb.AddForce(pushDirection * strength);

                if (rb.CompareTag("Player"))
                {
                    SlinAndKyControllerBase player = rb.GetComponent<SlinAndKyControllerBase>();
                    if (player != null)
                    {
                        player.SetIsMagnetized(true);
                        magnetizedThisFrame.Add(player);
                    }
                }
                
            }

        }

        foreach (var p in magnetizedLastFrame)
        {
            if (!magnetizedThisFrame.Contains(p)) p.SetIsMagnetized(false);
        }
        magnetizedLastFrame = magnetizedThisFrame;

    }

    void OnDrawGizmosSelected()
    {
        UpdateBoxSize();
        Gizmos.color = Color.red;

        Vector3 frontCenter = transform.position + transform.forward * (boxSize.z * 0.5f);
        Gizmos.matrix = Matrix4x4.TRS(frontCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}
