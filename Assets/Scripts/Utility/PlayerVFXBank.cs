using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerVFXBank", order = 1)]
public class PlayerVFXBank : ScriptableObject
{
    public GameCue Bite;
    public GameCue BiteEmpty;
    public GameCue Death;
    public GameCue Footstep;
    
    public Sprite BiteSprite;
    public Sprite Bark;
    public Sprite FootstepSprite;
    public Sprite DeathSprite;
    public Sprite EatCandy;
}
