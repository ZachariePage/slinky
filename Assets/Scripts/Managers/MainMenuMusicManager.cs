// using UnityEngine;
//
// public class MainMenuMusicManager : MonoBehaviour
// {
//     [SerializeField] private TwoPartMusic mainMenuMusic;
//
//     private void Awake()
//     {
//         if (mainMenuMusic.Intro != null) mainMenuMusic.Intro.LoadAudioData();
//         if (mainMenuMusic.Loop != null) mainMenuMusic.Loop.LoadAudioData();
//     }
//     
//     private void Start()
//     {
//         InGameMusicManager inGameMusicManager = FindObjectOfType<InGameMusicManager>();
//         if(inGameMusicManager)
//             inGameMusicManager.SetMode(InGameMusicManager.InGameMode.Menu);
//         else
//             MusicManager.Instance.PlayTwoPart(mainMenuMusic);
//     }
// }