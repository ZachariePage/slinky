using System;
using UnityEngine;

public class TourniquetMotor : MonoBehaviour
{
    private Rigidbody rb;
    
    [SerializeField] private float torque = 10f;
    [SerializeField] private float maxAngularVelocity = 5f;
    [SerializeField] private ForceMode forceMode = ForceMode.VelocityChange;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private float stunDuration = 0.2f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = maxAngularVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (rb.angularVelocity.y < maxAngularVelocity)
        {
            rb.AddTorque(Vector3.up * torque, forceMode);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if(launchForce == 0) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb == null) return;
        
        if (collision.gameObject.tag == "Player")
        {
            SlinAndKyControllerBase controller = collision.gameObject.GetComponent<SlinAndKyControllerBase>();
            if(controller == null) return;
            
            controller.StunPlayer(stunDuration);
        }
        
        Vector3 launchDir = (collision.gameObject.transform.position - collision.GetContact(0).point).normalized;
        
        rb.AddForce(launchDir * launchForce, ForceMode.Impulse);
    }
}
