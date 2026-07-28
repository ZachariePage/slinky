using UnityEngine;

[CreateAssetMenu(menuName = "Chomp/AttachmentConfig/NoAttachmentConfigSO")]
public class NoAttachmentConfig : AttachmentConfig
{
    public override AttachmentType Type => AttachmentType.NoAttachment;
}
