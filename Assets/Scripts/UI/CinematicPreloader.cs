// using System.Collections;
// using UnityEngine;
// using UnityEngine.Video;
//
// public class CinematicPreloader : MonoBehaviour
// {
//     [Header("Clip + Output")]
//     [SerializeField] private VideoClip cinematicToPreload;
//     [SerializeField] private RenderTexture targetTexture;
//
//     [Header("Playback (enabled object)")]
//     [SerializeField] private VideoPlayer playbackPlayer;
//     private bool _prepared;
//
//     private void Awake()
//     {
//         if (cinematicToPreload == null)
//             return;
//
//         playbackPlayer.playOnAwake = false;
//         playbackPlayer.waitForFirstFrame = true;
//         playbackPlayer.isLooping = false;
//
//         playbackPlayer.source = VideoSource.VideoClip;
//         playbackPlayer.clip = cinematicToPreload;
//
//         playbackPlayer.renderMode = VideoRenderMode.RenderTexture;
//         playbackPlayer.targetTexture = targetTexture;
//
//         playbackPlayer.audioOutputMode = VideoAudioOutputMode.None;
//
//         playbackPlayer.prepareCompleted += OnPrepared;
//         playbackPlayer.Prepare();
//     }
//
//     private void OnEnable()
//     {
//         if (cinematicToPreload == null || playbackPlayer == null)
//             return;
//
//         StartCoroutine(PlayWhenReady());
//     }
//
//     private IEnumerator PlayWhenReady()
//     {
//         while (!_prepared)
//             yield return null;
//
//         playbackPlayer.playOnAwake = true;
//         playbackPlayer.waitForFirstFrame = true;
//         playbackPlayer.isLooping = false;
//
//         playbackPlayer.source = VideoSource.VideoClip;
//         playbackPlayer.clip = cinematicToPreload;
//
//         playbackPlayer.renderMode = VideoRenderMode.RenderTexture;
//         playbackPlayer.targetTexture = targetTexture;
//
//         playbackPlayer.Play();
//     }
//
//     private void OnPrepared(VideoPlayer vp)
//     {
//         _prepared = true;
//         vp.prepareCompleted -= OnPrepared;
//     }
//
//     private void OnDestroy()
//     {
//         if (playbackPlayer != null)
//             playbackPlayer.prepareCompleted -= OnPrepared;
//     }
// }