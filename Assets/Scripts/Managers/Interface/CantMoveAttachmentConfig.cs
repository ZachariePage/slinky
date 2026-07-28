using UnityEngine;

[CreateAssetMenu(menuName = "Chomp/AttachmentConfig/CantMoveAttachmentConfigSO")]
public class CantMoveAttachmentConfig : AttachmentConfig
{
    public override AttachmentType Type => AttachmentType.CantMove;
}
