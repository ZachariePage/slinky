using UnityEngine;

public class RespawnCheckpoint : MonoBehaviour
{
    [SerializeField] private bool activateOnceOnly;
    public GameObject newRespawnLocation;
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
            if (newRespawnLocation != null)
            {
                WorldManager.Instance.SetRespawnLocation(newRespawnLocation);
            }
            
            if (activateOnceOnly)
            {
                Destroy(gameObject);
            }
        }
    }
}
