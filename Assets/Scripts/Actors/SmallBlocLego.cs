using UnityEngine;

public class SmallBlocLego : MonoBehaviour, IChompable
{
    private bool createJointOnChomp = true;
    [SerializeField] private ChompableSpringValue springValue;

    [Header("Retroaction")]
    [SerializeField] private GameCue[] OnChompCues;
    [SerializeField] private GameCue[] OnReleaseCues;

    [Header("Chomp")]
    [SerializeField] private AttachmentConfig chompConfig;
    public AttachmentType attachmentType = AttachmentType.ChildTransform;

    private Rigidbody rb;
    private bool wasKinematic;
    private float startingMass;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startingMass = rb.mass;
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
        }
    }

    public void OnChomped(GameObject chomper)
    {
        if (rb == null) return;
        rb.mass = 2f;
        foreach (GameCue c in OnChompCues)
        {
            c?.Execute(transform.position);
        }
    }

    public void OnReleased(GameObject chomper)
    {
        if (rb == null) return;
        rb.mass = startingMass;
        foreach (GameCue c in OnReleaseCues)
        {
            c?.Execute(transform.position);
        }
    }

    public bool AllowsAttachment()
    {
        return createJointOnChomp;
    }

    public AttachmentType GetAttachmentType()
    {
        return attachmentType;
    }

    public ChompableSpringValue GetSpringValue()
    {
        return springValue;
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