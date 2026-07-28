using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ChompSpringValue", order = 1)]
public class ChompableSpringValue : ScriptableObject
{
    [SerializeField] public float spring = 200f;
    [SerializeField] public float damper = 80f;
    [SerializeField] public float maxDistance = 0.3f;
    [SerializeField] public float minDistance = 0f;
    [SerializeField] public float breakForce = 500f;
}
