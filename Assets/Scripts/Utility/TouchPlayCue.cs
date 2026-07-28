using System.Collections.Generic;
using UnityEngine;

public class TouchPlayCue : MonoBehaviour
{
    [Header("Cooldown")]
    [Tooltip("Time before a sound can play again")]
    [SerializeField] private float cooldown = 0.5f;

    [Header("Allowed Tags")]
    [Tooltip("Tags that can trigger the cues")]
    [SerializeField] private List<string> triggeringTags = new List<string>();

    [Header("Cues")]

    [SerializeField] private GameCue[] cues;


    private float lastPlayTime = -999f;
    private Transform cam;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryPlay(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {

        TryPlay(other);
    }

    private void TryPlay(Collider other)
    {
        // Check if collision is in the allowed tags 
        if (!triggeringTags.Contains(other.tag))
            return;

        if (Time.time < lastPlayTime + cooldown)
            return;

        lastPlayTime = Time.time;

        PlayCue(cues, other.transform); 
    }

    void PlayCue(GameCue[] cuesList, Transform location)
    {
        foreach (GameCue cue in cuesList)
        {
            cue?.Execute(location.position);
        }
    }
}