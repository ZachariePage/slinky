using UnityEngine;

[System.Serializable]
public struct ChompRule
{
    public LayerMask layerMask;
    public bool useFOV;
    public bool requiresChompable;

    public AttachmentDistance[] attachmentDistances;

    public float nonChompableDistance;

    public float GetMaxDistance(IChompable chompable)
    {
        if (chompable == null)
            return nonChompableDistance;

        AttachmentType type = chompable.GetAttachmentType();

        foreach (var entry in attachmentDistances)
        {
            if (entry.type == type)
            {
                return entry.maxDistance;
            }
        }

        return nonChompableDistance;
    }
}
[System.Serializable]
public struct AttachmentDistance
{
    public AttachmentType type;
    public float maxDistance;
}

[CreateAssetMenu(menuName = "Chomp/Chomp Rules")]
public class ChompRuleSet : ScriptableObject
{
    public ChompRule[] rules;
}
