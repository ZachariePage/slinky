using System;
using UnityEngine;

public class DestroyWithSword : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Sword"))
        {
            Destroy(gameObject);
        }
    }
}
