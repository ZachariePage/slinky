using UnityEngine;

public class Football : MonoBehaviour, IChompable
{
    [Header("Chomp")]
    [SerializeField] private AttachmentConfig chompConfig;
    private AttachmentType attachmentType = AttachmentType.NoAttachment;
    [SerializeField] private float thrownForce;
    private Rigidbody rb;

    [SerializeField] private float maxDistanceFromStart = 40;
    private Vector3 startPosition;
    private Animator anim;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.SqrMagnitude(transform.position - startPosition) > maxDistanceFromStart * maxDistanceFromStart)
        {
            Respawn();
        }

        if (anim != null)
        {
            if (rb.linearVelocity.magnitude > 1f)
            {
                anim.SetBool("InBall", true);
            }
            else
            {
                anim.SetBool("InBall", false);
            }
        }
        
    }

    private void Respawn()
    {
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ActivationGoal>())
        {
            Respawn();
        }
    }

    public void OnChomped(GameObject chomper)
    {
        return;
        Vector3 direction = transform.position - chomper.transform.position; 
        rb.AddForce(direction.normalized * thrownForce, ForceMode.Impulse);
    }

    public void OnReleased(GameObject chomper)
    {

    }

    public bool AllowsAttachment()
    {
        return false;
    }

    public AttachmentType GetAttachmentType()
    {
        return attachmentType;
    }

    public ChompableSpringValue GetSpringValue()
    {
        return null;
    }
    public void OnSetupFinish(GameObject chomper)
    {
        
    }
    public AttachmentLocation GetAttachmentLocation()
    {
        return AttachmentLocation.location1;
    }
    
    public AttachmentConfig GetAttachmentConfig()
    {
        return chompConfig;
    }
}
