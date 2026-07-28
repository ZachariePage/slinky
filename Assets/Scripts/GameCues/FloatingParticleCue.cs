using UnityEngine;

[CreateAssetMenu(menuName = "Game/Cues/floating particle Cue")]
public class FloatingParticleCue : GameCue
{
    public GameObject prefab;
    public Sprite defaultPng;
    public bool autoDestroy;
    public float destroyDelay;
    public override GameObject Execute(Vector3 position)
    {
        if (prefab != null)
        {
            ParticleSystem ps = VFXFactory.Instance.SpawnParticleSystem("FloatingParticle", position, Quaternion.identity, defaultPng, true, 2f);
            return ps.gameObject;
        }
        return null;
    }

    public override GameObject Execute(Vector3 position, Sprite png)
    {
        if (prefab != null)
        {
            ParticleSystem ps = VFXFactory.Instance.SpawnParticleSystem("FloatingParticle", position, Quaternion.identity, png, true, 2f);
            return ps.gameObject;
        }
        return null;
    }
}
