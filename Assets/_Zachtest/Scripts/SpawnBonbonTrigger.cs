using System;
using UnityEngine;

public class SpawnBonbonTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            spawnBonBon();
        }
    }

    void spawnBonBon()
    {
        CollectibleFactory.Instance.SpawnCollectiblesInBurst("BonBonWithRB", transform.position, 20);
        Destroy(this);
    }
}
