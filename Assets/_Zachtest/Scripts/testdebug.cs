using UnityEngine;

public class testdebug : MonoBehaviour
{
    public float moveForce = 10f;
    
    private Rigidbody rb;
    public float he;
    public float hhe;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        he = h;
        hhe = v;
        rb.AddForce(new Vector3(h, 0, v) * moveForce, ForceMode.Acceleration);
    }
}
