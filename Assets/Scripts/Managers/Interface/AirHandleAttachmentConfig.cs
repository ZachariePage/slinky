using UnityEngine;

[CreateAssetMenu(menuName = "Chomp/AttachmentConfig/AirHandleAttachmentConfigSO")]
public class AirHandleAttachmentConfig : AttachmentConfig
{
    public override AttachmentType Type => AttachmentType.Hinge;
    
    public float damping = 0.5f;
    public float springForce = 25f;
    public float angleLimits = 45f;
    public Vector3 swingAxis = new Vector3(1, 0, 0);
    public bool collisionEnabled = false;
}
