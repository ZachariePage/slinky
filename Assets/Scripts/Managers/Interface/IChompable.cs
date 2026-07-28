using UnityEngine;

public enum AttachmentType
{
    CantMove,
    ChildTransform,
    Spring,
    NoAttachment,
    Hinge,
}

public enum AttachmentLocation
{
    location1,
    location2,
    location3,
    location4,
}
public interface IChompable
{
    void OnChomped(GameObject chomper);
    void OnReleased(GameObject chomper);
    bool AllowsAttachment();    
    AttachmentType GetAttachmentType();
    public ChompableSpringValue GetSpringValue();

    public void OnSetupFinish(GameObject chomper);
    
    public AttachmentLocation GetAttachmentLocation();
    
    public AttachmentConfig GetAttachmentConfig();
}