using System.Collections.Generic;
using UnityEngine;

public class Pulley : ActivationMechanism, IChompable
{
    [Header("Pulley Settings")]
    [SerializeField] private Vector3 endPoint;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float pullForceThreshold = 20f;
    [SerializeField] private float maxPullAngle = 35f;
    public int requiredPlayers = 1;
    private List<GameObject> Players = new List<GameObject>();
    [SerializeField] private bool GoBackWhenNotPulled = false;

    private float t;                     
    private bool chomped;
    private SpringJoint joint;           
    private Transform chomperTransform;  
    private Vector3 startPos;
    private Vector3 endPos;
    private bool complete;
    private Rigidbody chomperRb;
    [Header("Chomp")]
    [SerializeField] private AttachmentConfig notEnoughPlayersConfig;
    [SerializeField] private AttachmentConfig enoughPlayersConfig;
    public AttachmentType attachmentType = AttachmentType.Spring;
    
    [SerializeField] private ChompableSpringValue springValue;
    
    private bool CanPlayerMoveItAlone = true;

    
    protected override void Start()
    {
        base.Start();
        startPos = transform.localPosition;
        endPos = startPos + endPoint;
        
        if (requiredPlayers > 1)
        {
            CanPlayerMoveItAlone = false;
        }
    }

    protected override void Update()
    {
        base.Update();
        if(GoBackWhenNotPulled && !chomped)
        {
            t -= Time.deltaTime * moveSpeed;
            t = Mathf.Clamp01(t);
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
        }
        
        if (activated && t < 1)
        {
            Deactivate();
        }

        if (!chomped || joint == null)
            return;
        
        float pullForce = joint.currentForce.magnitude;
        SlinAndKyControllerBase controller = chomperTransform.GetComponent<SlinAndKyControllerBase>();
        Vector3 playerTransform = new Vector3(chomperTransform.position.x, 0f, chomperTransform.position.z);
        Vector3 pulleyTransform = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 ropeDir = (pulleyTransform - playerTransform).normalized;
        Vector3 inputDir = new Vector3(-controller.GetPlayerInput().y, 0, controller.GetPlayerInput().x);

        bool isPullingTowardPulley = Vector3.Dot(inputDir, ropeDir) > 0f;
        
        if (isPullingTowardPulley && Players.Count >= requiredPlayers)
        {
            t += Time.deltaTime * moveSpeed;
            t = Mathf.Clamp01(t);
        }
        
        transform.localPosition = Vector3.Lerp(startPos, endPos, t);
        
        if (t >= 1f && pullForce > pullForceThreshold && !activated)
        {
            Activate();
        }
    }

    public override void Activate()
    {
        base.Activate();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override bool ActivateMessage()
    {
        return base.ActivateMessage();
    }

    public void OnChomped(GameObject chomper)
    {
        chomped = true;
        chomperTransform = chomper.transform;
        
        if(!Players.Contains(chomper)) 
        {
            Players.Add(chomper);
        }
        if (Players.Count >= requiredPlayers && !CanPlayerMoveItAlone)
        {
            ActivateJointForAll();
        }
    }
    
    void ActivateJointForAll()
    {
        foreach (GameObject p in Players)
        {
            PlayerChomp playerChomp = p.GetComponent<PlayerChomp>();
            playerChomp.SetCantMove(false);
            playerChomp.AttachJointTo(GetComponent<Rigidbody>(), springValue);
        }
    }

    public void OnReleased(GameObject chomper)
    {
        chomped = false;
        joint = null;
        chomperRb =  null;
        chomperTransform = null;
        
        if (Players.Contains(chomper))
        {
            Players.Remove(chomper);
        }
    }

    public bool AllowsAttachment()
    {
        return true;
    }

    public AttachmentType GetAttachmentType()
    {
        if (Players.Count < requiredPlayers)
        {
            return AttachmentType.CantMove;
        }

        return AttachmentType.Spring;
    }

    public ChompableSpringValue GetSpringValue()
    {
        return springValue;
    }

    public void OnSetupFinish(GameObject chomper)
    {
        chomperRb = chomper.GetComponent<Rigidbody>();
        joint = chomper.GetComponent<PlayerChomp>().chompJoint as SpringJoint;
    }
    public AttachmentLocation GetAttachmentLocation()
    {
        return AttachmentLocation.location1;
    }
    
    public AttachmentConfig GetAttachmentConfig()
    {
        if (Players.Count < requiredPlayers)
        {
            return notEnoughPlayersConfig;
        }

        return enoughPlayersConfig;
    }
}