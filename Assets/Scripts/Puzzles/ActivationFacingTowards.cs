using System.Collections.Generic;
using UnityEngine;

public class ActivationFacingTowards : ActivationMechanism
{
    private List<SlinAndKyControllerBase> players = new List<SlinAndKyControllerBase>();
    [SerializeField] private Transform target;
    [Range(-1f, 1f)]
    [SerializeField] private float dotTreshold = 0.5f;

    [SerializeField] private int screenshotID = 0;
    [SerializeField] private bool takingPicture = false;

    private int timer = 3;
    
    protected override void Start()
    {
        activated = false;
    }
    
    protected override void Update()
    {
        if(activated) return;
        if (players.Count == 2)
        {
            if (CheckIfPlayersFace() && !takingPicture)
            {
                PrepareToTakePicture();
            }
        }

        if ((players.Count < 2 || !CheckIfPlayersFace()) && takingPicture)
        {
            CancelPicture();
        }
    }


    void PrepareToTakePicture()
    {
        takingPicture = true;
        CountdownTimer.StartCountdown(3, TakePicture);
    }

    void CancelPicture()
    {
        takingPicture = false;
        CountdownTimer.Cancel();
    }

    void TakePicture()
    {
        ScreenshotManager.TakeScreenshot(screenshotID);
        activated = true;
    }
    private bool CheckIfPlayersFace()
    {
        Collider targetCollider = target.GetComponent<Collider>();

        foreach (SlinAndKyControllerBase player in players)
        {
            Vector3 closestPoint = Vector3.zero;
            if (targetCollider != null)
            {
                closestPoint = targetCollider.ClosestPoint(player.transform.position);
            }
            else
            {
               closestPoint = target.transform.position; 
            }

            Vector3 direction = (closestPoint - player.transform.position).normalized;
            float dot = Vector3.Dot(player.transform.forward, direction);

            if (dot < dotTreshold)
            {
                return false;
            }
        }
        return true;
    }
    public override void Activate()
    {
        base.Activate();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override bool IsActivated()
    {
        return base.IsActivated();
    }

    public override bool ActivateMessage()
    {
        if (!base.ActivateMessage())
        {
            return false;
        }
        

        return false;
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!players.Contains(other.gameObject.GetComponent<SlinAndKyControllerBase>()))
            {
                players.Add(other.gameObject.GetComponent<SlinAndKyControllerBase>());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (players.Contains(other.gameObject.GetComponent<SlinAndKyControllerBase>()))
            {
                players.Remove(other.gameObject.GetComponent<SlinAndKyControllerBase>());
            }
        }
    }
}
