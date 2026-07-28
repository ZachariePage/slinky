using UnityEngine;

public struct LayerObjectConveyorBelt
{
    LayerMask mask;
    float force;
}
public class ConveyorBelt : MonoBehaviour
{
    [Tooltip("How fast the object will move on the conveyor (max speed)")]
    [SerializeField] private float force = 5f;
    [SerializeField] private float forceOther = 5f;
    [Tooltip("It should move towards the vector forward of the belt conveyor. If doesnt work just try swapping the 1 around")]
    [SerializeField] private Vector3 localDirection = Vector3.forward;
    
    [SerializeField] private ForceMode forceMode = ForceMode.Force;
    
    [SerializeField] private float scrollSpeed = 0.5f;
    
    private Renderer render;

    void Start()
    {
        render = GetComponent<Renderer>();
    }
    
    void Update()
    {
        if (render != null)
        {
            //when we will add texture this should offset the texture to make it seem like a conveyor belt
            float offset = Time.time * scrollSpeed;
            render.material.mainTextureOffset = new Vector2(offset, 0);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        Vector3 beltDir = transform.TransformDirection(localDirection).normalized;
        
        SlinAndKyControllerBase player = rb.GetComponent<SlinAndKyControllerBase>();
        float dot = 0f;
        float cap;
        if (player != null)
        {
            cap = force;
        }
        else
        {
            cap = forceOther;
        }
        
        if (player != null)
        {
            Vector3 inputDir = player.GetPlayerInput();
            float maxSpeed = player.GetMaxSpeed();
            dot = Mathf.Clamp01(Vector3.Dot(inputDir, beltDir));
            cap = force + maxSpeed * dot;
        }

        if (rb.linearVelocity.magnitude < cap)
        {
            rb.AddForce(beltDir * cap, ForceMode.Acceleration);
        }
    }
}



