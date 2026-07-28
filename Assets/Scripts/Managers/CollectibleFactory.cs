using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleFactory : MonoBehaviour
{
    public static CollectibleFactory Instance { get; private set; }

    [System.Serializable]
    public class CollectibleEntry
    {
        public string name;
        public GameObject prefab;
    }

    [Header("Register your collectibles here")]
    public CollectibleEntry[] collectibles;

    [Header("Spawn Settings")]
    public CollectibleSpawnSO spawnSettings;

    private Dictionary<string, GameObject> prefabLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        prefabLookup = new Dictionary<string, GameObject>();
        foreach (var entry in collectibles)
        {
            if (!prefabLookup.ContainsKey(entry.name))
                prefabLookup.Add(entry.name, entry.prefab);
        }
    }
    
    public void SpawnCollectiblesInBurst(GameObject prefab, Vector3 position, int amount)
    {
        StartCoroutine(SpawnOverTime(prefab, position, amount));
    }
    
    public void SpawnCollectiblesInBurst(string prefabName, Vector3 position, int amount)
    {
        if (!prefabLookup.TryGetValue(prefabName, out GameObject prefab))
        {
            Debug.LogWarning($"CollectibleFactory: No prefab registered with name '{prefabName}'");
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"CollectibleFactory: No prefab registered with name '{prefabName}'");
        }

        if (position == Vector3.zero)
        {
            Debug.LogWarning($"position null");
        }
        
        StartCoroutine(SpawnOverTime(prefab, position, amount));
    }
    
    private IEnumerator SpawnOverTime(GameObject prefab, Vector3 position, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnSingle(prefab, position);

            float delay = Random.Range(spawnSettings.TimeMin, spawnSettings.TimeMax);
            yield return new WaitForSeconds(delay);
        }
    }

    private void SpawnSingle(GameObject prefab, Vector3 position)
    {
        GameObject obj = ObjectPool.Instance.GetPooledObject(prefab);

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float force = Random.Range(spawnSettings.forceMin, spawnSettings.forceMax);
            
            Vector3 direction = (Random.insideUnitSphere).normalized;

            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }
    
    public void SpawnCollectiblesInCone(string prefabName, Vector3 position, Vector3 forward, int amount)
    {
        if (!prefabLookup.TryGetValue(prefabName, out GameObject prefab))
        {
            Debug.LogWarning($"CollectibleFactory: No prefab registered with name '{prefabName}'");
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"CollectibleFactory: No prefab registered with name '{prefabName}'");
        }

        if (position == Vector3.zero)
        {
            Debug.LogWarning($"position null");
        }
        
        StartCoroutine(SpawnOverTimeCone(prefab, position,forward, amount));
    }
    
    private IEnumerator SpawnOverTimeCone(GameObject prefab, Vector3 position, Vector3 inForward, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnSingleInCone(prefab, position, inForward);

            float delay = Random.Range(spawnSettings.TimeMin, spawnSettings.TimeMax);
            yield return new WaitForSeconds(delay);
        }
    }

    
    
    private void SpawnSingleInCone(GameObject prefab, Vector3 position, Vector3 inForward)
    {
        GameObject obj = ObjectPool.Instance.GetPooledObject(prefab);

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (inForward == Vector3.zero)
            {
                return;
            }

            float force = Random.Range(spawnSettings.forceMin, spawnSettings.forceMax);
            float upward = spawnSettings.upward;

            rb.AddTorque(Random.insideUnitSphere * spawnSettings.torque, ForceMode.Impulse);

            Vector3 forward = inForward.normalized;
            float coneAngle = spawnSettings.coneAngle;

            Vector3 randomDirection = GetRandomDirectionInCone(forward, coneAngle);
            randomDirection += Vector3.up * upward;

            rb.AddForce(randomDirection.normalized * force, ForceMode.Impulse);
        }
    }
    
    Vector3 GetRandomDirectionInCone(Vector3 forward, float angle)
    {
        Quaternion forwardRotation = Quaternion.LookRotation(forward);

        float randomAngle = Random.Range(0f, angle);
        float randomYaw = Random.Range(0f, 360f);

        Quaternion randomRotation = Quaternion.AngleAxis(randomYaw, Vector3.forward) * Quaternion.AngleAxis(randomAngle, Vector3.right);

        return forwardRotation * randomRotation * Vector3.forward;
    }
}
