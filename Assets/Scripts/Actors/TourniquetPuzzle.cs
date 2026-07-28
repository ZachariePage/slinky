using System;
using UnityEngine;

public class TourniquetPuzzle : ActivationMechanism
{
    [Header("Settings")]
    public float totalRotationsRequired = 3f;

    private float totalAngle = 0f;
    private float lastYAngle;
    private Rigidbody rb;
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        lastYAngle = transform.eulerAngles.y;
    }

    protected override void Update()
    {
        base.Update();
    }

    private void FixedUpdate()
    {
        if(activated) return;
        float currentY = transform.eulerAngles.y;
        float delta = Mathf.DeltaAngle(lastYAngle, currentY);

        if (delta < 0)
        {
            totalAngle = Mathf.Max(0f, totalAngle + delta);

            if (totalAngle <= 0f)
            {
                rb.angularVelocity = Vector3.zero;
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    lastYAngle,
                    transform.eulerAngles.z
                );
            }
        }
        else if (delta > 0)
        {
            totalAngle += delta;

            if (totalAngle >= 360f * totalRotationsRequired)
            {
                totalAngle = 360f * totalRotationsRequired;
                Activate();
            }
        }

        lastYAngle = transform.eulerAngles.y;
    }

    public override void Activate()
    {
        base.Activate();
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override bool ActivateMessage()
    {
        return base.ActivateMessage();
    }

    // Update is called once per frame
   

}
