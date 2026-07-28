using UnityEngine;
using UnityEngine.Serialization;

public class ChompAttach : MonoBehaviour, IChompable
{
    private Rigidbody _rigidbody;

    [FormerlySerializedAs("createJointOn")] [Tooltip("Si le joueur s'attache avec un joint dessus when chomped")]
    public bool createJointOnChomp = true;
    [SerializeField] private ChompableSpringValue springValue;
    
    [Tooltip("Si le truc bouge ou non")]
    public bool IsKinematic;

    private Rigidbody rb;
    [Header("Chomp")]
    [SerializeField] private AttachmentConfig chompConfig;
    public AttachmentType attachmentType = AttachmentType.CantMove;

    
    [Header("Retroaction")]
    [SerializeField] protected GameCue[] OnChompCues;
    [SerializeField] protected GameCue[] OnReleaseCues;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createJointOnChomp = true;
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = IsKinematic;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnChomped(GameObject chomper)
    {
        if (rb == null) return;
        
        foreach (GameCue c in OnChompCues)
        {
            c?.Execute(transform.position);
        }
    }

    public void OnReleased(GameObject chomper)
    {
        if (rb == null) return;
        
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
