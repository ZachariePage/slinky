using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "Game/Cues/onomatopiea particle Cue")]
public class Onomatopeia : GameCue
{
    public GameObject prefab;
    public Sprite defaultPng;
    public bool autoDestroy;
    public float destroyDelay;
    public override GameObject Execute(Vector3 position)
    {
        if (prefab != null)
        {
            GameObject obj = VFXFactory.Instance.SpawnVFX(prefab, position, Quaternion.identity, autoDestroy, destroyDelay);

            VisualEffect effect = obj.GetComponent<VisualEffect>();

            if (effect != null && defaultPng != null)
            {
                effect.SetTexture("Onomatopia", defaultPng.texture);
            }
            
            return obj;
        }
        return null;
    }

    public override GameObject Execute(Vector3 position, Sprite png)
    {
        if (prefab != null)
        {
            GameObject obj = VFXFactory.Instance.SpawnVFX(prefab, position, Quaternion.identity, autoDestroy, destroyDelay);

            VisualEffect effect = obj.GetComponent<VisualEffect>();

            if (effect != null && png != null)
            {
                effect.SetTexture("Onomatopia", png.texture);
            }
            
            return obj;
        }
        return null;
    }
}
