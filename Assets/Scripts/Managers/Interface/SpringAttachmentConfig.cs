using UnityEngine;

[CreateAssetMenu(menuName = "Chomp/AttachmentConfig/SpringAttachmentConfigSO")]
public class SpringAttachmentConfig : AttachmentConfig
{
    public override AttachmentType Type => AttachmentType.Spring;
    public ChompableSpringValue springValue;
    public bool CannotRotateAround;
    public bool collisionWithPlayer = true;
}
