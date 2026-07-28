using UnityEngine;
using System.Collections.Generic;

public class VFXFactory : MonoBehaviour
{
    public static VFXFactory Instance;

    public enum VFXBehavior
    {
        Default,
        AttachScript1,
        CustomLogic
    }

    [System.Serializable]
    public class VFXEntry
    {
        public string name;
        public GameObject prefab;

        [Header("Auto Destroy Settings")]
        public bool autoDestroy = true;
        public float destroyDelay = 2f;
    }

    [Header("VFX List")]
    public List<VFXEntry> vfxList = new List<VFXEntry>();

    private Dictionary<string, VFXEntry> vfxDict;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        vfxDict = new Dictionary<string, VFXEntry>();
        foreach (var entry in vfxList)
        {
            if (!vfxDict.ContainsKey(entry.name))
            {
                vfxDict.Add(entry.name, entry);
            }
        }
    }

    public GameObject SpawnVFX(string name, Vector3 position, Quaternion rotation)
    {
        if (vfxDict.TryGetValue(name, out VFXEntry entry))
        {
            GameObject instance = Instantiate(entry.prefab, position, rotation);

            if (entry.autoDestroy)
            {
                Destroy(instance, entry.destroyDelay);
            }

            return instance;
        }
        else
        {
            Debug.LogWarning($"VFX '{name}' not found.");
            return null;
        }
    }
    public GameObject SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, bool autoDestroy = false, float destroyDelay = 2f)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab is null, cannot spawn VFX.");
            return null;
        }

        GameObject instance = Instantiate(prefab, position, rotation);

        if (autoDestroy)
        {
            Destroy(instance, destroyDelay);
        }

        return instance;
    }

    public ParticleSystem SpawnParticleSystem(string name, Vector3 position, Quaternion rotation,  bool autoDestroy = false, float destroyDelay = 2f )
    {
        if (vfxDict.TryGetValue(name, out VFXEntry entry))
        {
            GameObject instance = Instantiate(entry.prefab, position, rotation);

            ParticleSystem ps = instance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();

                if (entry.autoDestroy || autoDestroy == true)
                {
                    Destroy(instance, ps.main.duration + ps.main.startLifetime.constantMax);
                }
            }
            else
            {
                Debug.LogWarning($"Prefab '{name}' does not contain a ParticleSystem component.");
            }

            return ps;
        }
        else
        {
            Debug.LogWarning($"Particle system '{name}' not found.");
            return null;
        }
    }
    public ParticleSystem SpawnParticleSystem(string name, Vector3 position, Quaternion rotation, Sprite customSprite, bool autoDestroy = false, float destroyDelay = 2f)
    {
        if (vfxDict.TryGetValue(name, out VFXEntry entry))
        {
            GameObject instance = Instantiate(entry.prefab, position, rotation);

            ParticleSystem ps = instance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && customSprite != null)
                {
                    Material mat = renderer.material;
                    if (mat != null)
                    {
                        ps.textureSheetAnimation.RemoveSprite(0);
                        ps.textureSheetAnimation.AddSprite(customSprite);
                    }
                }

                ps.Play();

                if (entry.autoDestroy || autoDestroy == true)
                {
                    Destroy(instance, ps.main.duration + ps.main.startLifetime.constantMax);
                }
            }
            else
            {
                Debug.LogWarning($"Prefab '{name}' does not contain a ParticleSystem component.");
            }

            return ps;
        }
        else
        {
            Debug.LogWarning($"Particle system '{name}' not found.");
            return null;
        }
    }

}
