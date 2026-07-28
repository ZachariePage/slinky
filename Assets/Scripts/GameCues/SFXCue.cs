using UnityEngine;

[CreateAssetMenu(menuName = "Game/Cues/SFX Cue")]
public class SFXCue : GameCue
{
    public AudioClip clip;

    public float volumeMin = 1.0f;
    public float volumeMax = 1.0f;

    public float pitchMin = 1.0f;
    public float pitchMax = 1.0f;


    public override GameObject Execute(Vector3 position)
    {
        if (clip != null)
        {
            float randomPitch = Random.Range(pitchMin, pitchMax);
            float randomVolume = Random.Range(volumeMin, volumeMax);

            SoundManager.Instance.PlaySFX(clip, position,randomVolume, randomPitch);
        }
        return null;
    }

    public override GameObject Execute(Vector3 position, Sprite png)
    {
        Execute(position);
        return null;
    }
}
