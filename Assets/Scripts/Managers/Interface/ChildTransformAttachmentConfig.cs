using UnityEngine;

[CreateAssetMenu(menuName = "Chomp/AttachmentConfig/ChildTransformAttachmentConfigSO")]
public class ChildTransformAttachmentConfig : AttachmentConfig
{
    public override AttachmentType Type => AttachmentType.ChildTransform;
    public AttachmentLocation location;
}
