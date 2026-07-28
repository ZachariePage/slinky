using System;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField]private int PlayerId;
    [SerializeField]private int Score;

    private void Start()
    {
        Init(PlayerId);
    }

    public void Init(int id)
    {
        PlayerId = id;
        WorldManager.Instance.RegisterPlayer(PlayerId);
        Score = WorldManager.Instance.GetPlayerScore(PlayerId);
    }
    
    public void AddScore(int amount)
    {
        Score += amount;
        WorldManager.Instance.NotifyScoreChanged(PlayerId, amount);
    }

    public int GetScore()
    {
        return Score;
    }

    public int GetPlayerId()
    {
        return PlayerId;
    }
}