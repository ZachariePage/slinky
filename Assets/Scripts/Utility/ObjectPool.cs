using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class PooledObject
{
    [Tooltip("Object to store in the pool")]
    public GameObject obj; 

    [Tooltip("Original Prefab")]
    public GameObject prefabType;

    [Tooltip("Default quantity pooled of object")]
    public int quantity; 
}


public class ObjectPool : MonoBehaviour, ISingleton
{

    private List<PooledObject> pool = new List<PooledObject>();

    [Tooltip("Objects to store in the pool by default")]
    [SerializeField] List<PooledObject> DefaultPooledObjects = new List<PooledObject>();


    // Init singleton
    public static ObjectPool Instance;

    void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else 
            Destroy(gameObject);
    }


    void Start()
    {
        // Loop through default objects and spawn them in pool

        foreach (PooledObject pooledObject in DefaultPooledObjects)
        {
            for (int i = 0; i < pooledObject.quantity; i++)
            {
                CreatePooledObject(pooledObject.prefabType);
            }
        }
    }

    

    // Add new objects and quanities to pool
    public void AddPooledObject(GameObject typeObject, float objectQty)
    {

        for (int o = 0; o < objectQty; o++)
        {
            CreatePooledObject(typeObject);
        }
    }

    public GameObject CreatePooledObject(GameObject prefab, bool addToPool = true)
    {
        GameObject instance = Instantiate(prefab, transform);
        instance.SetActive(false);
        instance.transform.SetParent(null); // Put it in scene root
        instance.name = prefab.name;

        PooledObject pooledObject = new PooledObject
        {
            obj = instance,
            prefabType = prefab,
            quantity = 1
        };

        if (addToPool)
            pool.Add(pooledObject);

        return instance;
    }

    public GameObject GetPooledObject(GameObject objType)
    {
        PooledObject pooledObj = pool.Find(p => 
            p.obj != null && 
            p.prefabType == objType && 
            !p.obj.activeInHierarchy
        );

        if (pooledObj != null)
        {
            ActivateObject(pooledObj.obj);
            return pooledObj.obj;
        }
        else
        {
            GameObject newObj = CreatePooledObject(objType, true);
            ActivateObject(newObj);
            return newObj;
        }
    }

    public GameObject GetObjectWithName(string objName)
    {
        PooledObject pooledObj = pool.Find(p => 
            p.obj != null && 
            p.obj.name == objName && 
            !p.obj.activeInHierarchy
        );
        //PooledObject pooledObj = pool.Find(p => p.obj.name == objName && !p.obj.activeInHierarchy);

        if (pooledObj != null)
        {
            ActivateObject(pooledObj.obj);
            return pooledObj.obj;
        }

        GameObject prefab = DefaultPooledObjects
            .Find(p => p.prefabType.name == objName)?.prefabType;

        if (prefab != null)
        {
            GameObject newObj = CreatePooledObject(prefab);
            ActivateObject(newObj);
            return newObj;
        }

        Debug.LogWarning($"Object with name '{objName}' not found in pool or prefabs.");
        return null;
    }


    public static void ReturnToPool(GameObject objectToReturn)
    {
        // Reset rigidbody if needed
        Rigidbody rb = objectToReturn.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        objectToReturn.SetActive(false);
        Instance.pool.Add(new PooledObject
        {
            obj = objectToReturn,
            prefabType = objectToReturn, 
            quantity = 1
        });
    }

    public static IEnumerator ReturnToPoolAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(obj);
    }

    private void ActivateObject(GameObject obj)
    {
        obj.SetActive(true);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var pooled in pool)
        {
            if (pooled.obj != null)
            {
                pooled.obj.SetActive(false);
            }
        }
    }
}
