using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CastShadowData", order = 5)]
public class CastShadowData : ScriptableObject
{
    public LayerMask shadowLayerMask;
    [Tooltip("Maximum distance at which the shadow will be cast")]
    public float shadowDistance = 10f;
    [Tooltip("Base size of the shadow when the object is directly above the surface")]
    public float shadowSize = 5f;
    [Range(0f, 1f)]
    [Tooltip("Minimum size multiplier for the shadow when the object is at the maximum distance")]
    public float minSizeMultiplier= 0.3f;
    [Range(0f, 1f)]
    [Tooltip("Minimum opacity of the shadow at maximum distance")]
    public float minFadeMultiplier = 0.1f;
}
