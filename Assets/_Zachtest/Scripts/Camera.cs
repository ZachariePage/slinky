using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform target;

    public float yOffset = 10f;
    public float xOffset = 0;
    public float zOffset = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            Vector3 pos = new Vector3(target.position.x + xOffset, target.position.y + yOffset, target.position.z + zOffset);
            transform.position = pos;
        }
        else
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
    }
}
