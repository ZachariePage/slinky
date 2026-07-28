using UnityEngine;

public abstract class AttachmentConfig : ScriptableObject
{
    public bool allowRotation = false;
    public abstract AttachmentType Type { get; }
}







