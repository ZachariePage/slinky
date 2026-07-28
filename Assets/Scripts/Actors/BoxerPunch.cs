using UnityEngine;

public class BoxerPunch : MonoBehaviour
{
    private Boxer boxer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxer = gameObject.GetComponentInParent<Boxer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" || other.GetComponent<StickyEnemies>() != null)
        {
            boxer.OnPlayerHit(other.gameObject);
        }
    }
}
