using UnityEngine;

public class ActivationPlate : ActivationMechanism
{
    private Animator anim;
    public int playSpeed = 1;
    protected override void Start()
    {
        anim = GetComponent<Animator>();
        anim.enabled = false;
        activated = false;
    }
    
    protected override void Update()
    {
        
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
            anim.enabled = true;
            anim.Play("PlateAnim");
            Activate();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.Play("PlateAnimBackward");
            Deactivate();
        }
    }
}