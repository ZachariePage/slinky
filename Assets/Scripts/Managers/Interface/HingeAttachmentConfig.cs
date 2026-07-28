using UnityEngine;

[CreateAssetMenu(menuName = "Chomp/AttachmentConfig/HingeAttachmentConfigSO")]
public class HingeAttachmentConfig : AttachmentConfig
{
    public override AttachmentType Type => AttachmentType.Hinge;
    //put whatever value you need here
    
    public float damping = 0.5f;
    public float mass = 0.1f;
    public float springForce = 25f;
    public float angleLimits = 45f;
    
}
