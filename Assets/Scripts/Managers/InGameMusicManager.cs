// using System;
// using UnityEngine;
//
// public class InGameMusicManager : MonoBehaviour
// {
//     public enum InGameMode
//     {
//         Menu,
//         Gameplay
//     }
//
//     [Header("Tracks")]
//     [SerializeField] private TwoPartMusic menuMusic;
//     [SerializeField] private TwoPartMusic gameplayMusic;
//
//     private InGameMode currentMode;
//
//     private void Awake()
//     {
//         if (gameplayMusic.Intro != null) gameplayMusic.Intro.LoadAudioData();
//         if (gameplayMusic.Loop != null) gameplayMusic.Loop.LoadAudioData();
//         
//         if (menuMusic.Intro != null) menuMusic.Intro.LoadAudioData();
//         if (menuMusic.Loop != null) menuMusic.Loop.LoadAudioData();
//     }
//
//     private void Start()
//     {
//         SetMode(InGameMode.Gameplay);
//     }
//
//     public void SetMode(InGameMode newMode)
//     {
//         if (currentMode == newMode) return;
//
//         currentMode = newMode;
//         
//         switch (currentMode)
//         {
//             case InGameMode.Menu:
//                 MusicManager.Instance.PlayTwoPart(menuMusic);
//                 break;
//
//             case InGameMode.Gameplay:
//                 MusicManager.Instance.PlayTwoPart(gameplayMusic);
//                 break;
//         }
//     }
//
//     public void PauseGameplayMusic()
//     {
//         MusicManager.Instance.Pause();
//     }
//
//     public void ResumeGameplayMusic()
//     {
//         MusicManager.Instance.Resume();
//     }
// }