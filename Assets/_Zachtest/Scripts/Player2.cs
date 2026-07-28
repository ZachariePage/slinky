using UnityEngine;

public class Player2 : MonoBehaviour
{
    public float force = 5f;
    public Rigidbody rb;

    public float changeDirectionTime = 10f;
    private float timer;

    public Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        timer = changeDirectionTime;
        ChooseNewDirection();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ChooseNewDirection();
            timer = changeDirectionTime;
        }

        MoveAI();
    }

    void MoveAI()
    {
        rb.linearVelocity = moveDirection * force;
    }

    void ChooseNewDirection()
    {
        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);

        moveDirection = new Vector3(x, 0f, z).normalized;
    }
}