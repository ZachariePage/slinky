using System;
using UnityEngine;

public class CrocodileTeeth : MonoBehaviour
{
    private Crocodile croc;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        croc = GetComponentInParent<Crocodile>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" || other.GetComponent<StickyEnemies>() != null)
        {
            //croc.OnPlayerHit(other.gameObject);
        }

    }
}
