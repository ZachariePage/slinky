using UnityEngine;

public class RelativeTeleporter : MonoBehaviour
{
    [Header("Teleport Colliders")]
    public BoxCollider sourceCollider;
    public BoxCollider destinationCollider;

    [Header("Tag Filter")]
    public string[] allowedTags;

    [Header("Velocity")]
    public bool resetVelocity = false;

    public float spawnHeightOffset = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        bool validTag = false;
        foreach (string tag in allowedTags)
        {
            if (other.CompareTag(tag))
            {
                validTag = true;
                break;
            }
        }

        if (!validTag || sourceCollider == null || destinationCollider == null)
            return;

        // Convert world position to source local space
        Vector3 local = sourceCollider.transform.InverseTransformPoint(other.transform.position);
        local -= sourceCollider.center;

        // Normalize position inside the source box
        Vector3 normalized = new Vector3(
            local.x / sourceCollider.size.x,
            0f,
            local.z / sourceCollider.size.z
        );

        // Clamp so extreme offsets don't explode
        normalized.x = Mathf.Clamp(normalized.x, -0.5f, 0.5f);
        normalized.z = Mathf.Clamp(normalized.z, -0.5f, 0.5f);

        // Apply to destination box
        Vector3 destLocal = new Vector3(
            normalized.x * destinationCollider.size.x,
            destinationCollider.size.y * 0.5f + spawnHeightOffset,
            normalized.z * destinationCollider.size.z
        );

        destLocal += destinationCollider.center;

        Vector3 newWorldPos = destinationCollider.transform.TransformPoint(destLocal);

        other.transform.position = newWorldPos;

        if (resetVelocity)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}