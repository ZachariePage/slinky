using UnityEngine;

public class killzoneAll : MonoBehaviour
{
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
            WorldManager.Instance.KillPlayer();
        }
        else
        {
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            WorldManager.Instance.KillPlayer();
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}
