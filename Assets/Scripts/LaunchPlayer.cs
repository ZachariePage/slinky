using UnityEngine;

public class LaunchPlayer : MonoBehaviour
{
    [Header("Launch Settings")]
    public float launchForce = 20f;
    public float cooldown = 0.5f;

    [Header("Direction")]
    public bool useTransformForward = true;
    public Vector3 worldDirection = Vector3.forward;

    private float lastLaunchTime = -999f;

    private void OnCollisionEnter(Collision collision)
    {
        TryLaunch(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryLaunch(other);
    }

    private void TryLaunch(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Time.time < lastLaunchTime + cooldown)
            return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        lastLaunchTime = Time.time;

        // --- Choose launch direction ---
        Vector3 dir = useTransformForward
            ? transform.forward
            : worldDirection.normalized;

        // Reset velocity for consistent launch
        rb.linearVelocity = Vector3.zero;

        // Launch player
        rb.AddForce(dir * launchForce, ForceMode.Impulse);
    }
}