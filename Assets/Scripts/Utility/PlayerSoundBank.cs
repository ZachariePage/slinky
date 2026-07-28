using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerSoundBank", order = 1)]
public class PlayerSoundBank : ScriptableObject
{
    public AudioClip Bite;
    public AudioClip BiteEmpty;
    public AudioClip Death;
    public AudioClip FootStep;
}
