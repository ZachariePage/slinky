using UnityEngine;

public class MachineABonBon : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int CoinsAmount = 999;
        //CollectibleFactory.Instance.SpawnCollectiblesInCone("BonBonWithRB", transform.position, transform.forward, CoinsAmount);
        CollectibleFactory.Instance.SpawnCollectiblesInBurst("BonBonWithRB", transform.position, CoinsAmount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
