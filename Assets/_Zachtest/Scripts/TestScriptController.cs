using System;
using UnityEngine;

public class TestScriptController : MonoBehaviour
{
    public Vector3 controls;
    public float speed;
    public ForceMode forceMode;
    public Rigidbody rb;
    void Start()
    {
     rb = GetComponent<Rigidbody>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        move();
    }

    void move()
    {
        rb.AddForce(controls * speed, forceMode);
    }
}
