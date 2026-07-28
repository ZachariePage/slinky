using System.Collections.Generic;
using UnityEngine;

public class TouchPlaySFX : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip sfx;

    [Header("Cooldown")]
    [Tooltip("Time before a sound can play again")]
    [SerializeField] private float cooldown = 0.5f;

    [Header("Random Volume")]
    [SerializeField] private float minVolume = 1f;
    [SerializeField] private float maxVolume = 1f;

    [Header("Random Pitch")]
    [SerializeField] private float minPitch = 1f;
    [SerializeField] private float maxPitch = 1f;

    [Header("Allowed Tags")]
    [Tooltip("Tags that can trigger the sound")]
    [SerializeField] private List<string> triggeringTags = new List<string>();

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

        float volume = Random.Range(minVolume, maxVolume);
        float pitch = Random.Range(minPitch, maxPitch);

        SoundManager.Instance.PlaySFX(
            sfx,
            transform.position,
            volume,
            pitch
        );
    }
}