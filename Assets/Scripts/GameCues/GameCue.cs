using UnityEngine;

public abstract class GameCue : ScriptableObject
{
    public abstract GameObject Execute(Vector3 position);
    public abstract GameObject Execute(Vector3 position, Sprite png);
}
