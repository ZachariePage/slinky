using System;
using UnityEngine;

public class MusicStarter : MonoBehaviour
{
    [SerializeField] MusicManager.MusicType musicType = MusicManager.MusicType.None;
    [SerializeField] bool playOnStart = true;
    
    private void Start()
    {
        if(playOnStart)
            SelectMusic();
    }
    
    public void SelectMusic()
    {
        MusicManager.Instance.SetMusic(musicType);
    }
}