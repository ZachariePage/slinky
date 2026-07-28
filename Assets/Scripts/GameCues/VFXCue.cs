using UnityEngine;

[CreateAssetMenu(menuName = "Game/Cues/VFX Cue")]
public class VFXCue : GameCue
{
    public GameObject prefab;
    public bool autoDestroy;
    public float destroyDelay;
    public override GameObject Execute(Vector3 position)
    {
        if (prefab != null)
        {
            GameObject obj = VFXFactory.Instance.SpawnVFX(prefab, position, Quaternion.identity, autoDestroy, destroyDelay);
            return obj;
        }
        return null;
    }

    public override GameObject Execute(Vector3 position, Sprite png)
    {
        return Execute(position);
    }
}