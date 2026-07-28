using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CollectibleSpawnSO", order = 1)]
public class CollectibleSpawnSO : ScriptableObject
{
    [SerializeField] public float forceMin = 4;
    [SerializeField] public float forceMax = 4;
    [SerializeField] public float radius = 1.5f;
    [SerializeField] public float upward = 0.4f;
    [SerializeField] public float coneAngle = 45f;
    
    [SerializeField] public float torque = 3f;

    [SerializeField] public float TimeMin;
    [SerializeField] public float TimeMax;
}
