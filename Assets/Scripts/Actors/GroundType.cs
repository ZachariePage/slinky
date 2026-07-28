using UnityEngine;

public enum TerrainType
{
    grass,
    ground,
    water,
    street
}
public class GroundType : MonoBehaviour
{
    public TerrainType type;
}
