using UnityEngine;

[CreateAssetMenu(fileName = "SFXSettings", menuName = "Audio/SFX Settings")]
public class SFXSettings : ScriptableObject
{
    [Header("Volume")]
    public float baseVolume = 1f;
    public float volumeMin = 0.95f;
    public float volumeMax = 1.05f;

    [Header("Pitch")]
    public float basePitch = 1f;
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;
}
