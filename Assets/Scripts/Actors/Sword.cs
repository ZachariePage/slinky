using System;
using UnityEngine;

public class Sword : MonoBehaviour, IDealDamage, IChompable
{
    [Header("Chomp")]
    [SerializeField] private AttachmentConfig chompConfig;
    
    public int damage = 10;
    public bool instantKill = false;
    public float stunDuration = 1f;
    
    [Header("Retroaction")]
    [SerializeField] protected float timerBeforeRetroaction;

    private float timer;
    private Transform playerTransform;

    [SerializeField]
    private float slerpSpeed = 5.0f;
    [SerializeField] protected GameCue[] OnCollisionCues;
    
    //private Quaternion currentRotation;
    private Vector3 localGripOffset;
    public AttachmentType attachmentType = AttachmentType.ChildTransform;
    
    public ChompableSpringValue springValue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer =  timerBeforeRetroaction;
    }
    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        
    }

    /*void LateUpdate()
    {
        if (playerTransform == null)
        {
            return;
        }
        currentRotation = Quaternion.Slerp(currentRotation, playerTransform.rotation, Time.deltaTime * slerpSpeed);
        transform.rotation = currentRotation;
        
    }*/
    private void OnCollisionEnter(Collision other)
    {
        if (CanPlaySound())
        {
            foreach (GameCue c in OnCollisionCues)
            {
                c?.Execute(transform.position);
            } 
        }
        
        
        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            DealDamage(damageable, other);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (CanPlaySound())
        {
            foreach (GameCue c in OnCollisionCues)
            {
                c?.Execute(transform.position);
            }
        }
        

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            DealDamage(damageable, null);
        }
    }
    public void DealDamage(IDamageable toObj, Collision collision)
    {
        DamageInfo info = new DamageInfo(damage, this.gameObject, Vector3.zero, Vector3.zero, instantKill, stunDuration);
        toObj.TakeDamage(info);
    }

    bool CanPlaySound()
    {
        if(timer > 0) return false;
        return true;
    }

    public void OnChomped(GameObject chomper)
    {
        
        /* J-F Was here
        Vector3 hitPoint = chomper.GetComponent<PlayerChomp>().lastChompHitPoint;
        localGripOffset = transform.InverseTransformPoint(hitPoint);
        */
        GetComponent<Collider>().isTrigger = true;
        playerTransform = chomper.GetComponent<Transform>();
    }

    public void OnReleased(GameObject chomper)
    {
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Collider>().isTrigger = false;
        playerTransform = null;
    }

    public bool AllowsAttachment()
    {
        throw new NotImplementedException();
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
        
        //J-F was here
        /*
        transform.localPosition = -localGripOffset;
        currentRotation = transform.rotation;
        
        
        
        Vector3 hitPoint = chomper.GetComponent<PlayerChomp>().lastChompHitPoint;
        Vector3 offset = transform.InverseTransformPoint(hitPoint);
        transform.localPosition = -offset;
        currentRotation = transform.rotation;*/
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
