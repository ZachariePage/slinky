using System;
using Unity.VisualScripting;
using UnityEngine;

public class KillPlayerOnTouch : MonoBehaviour, IDealDamage
{
    public GameObject newRespawnLocation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (newRespawnLocation != null)
            {
                WorldManager.Instance.SetRespawnLocation(newRespawnLocation);
            }
            WorldManager.Instance.KillPlayer();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (newRespawnLocation != null)
            {
                WorldManager.Instance.SetRespawnLocation(newRespawnLocation);
            }
            WorldManager.Instance.KillPlayer();
        }
    }


    public void DealDamage(IDamageable toObj, Collision collision)
    {
        
    }
}
