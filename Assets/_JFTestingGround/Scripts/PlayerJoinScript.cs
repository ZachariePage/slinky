using System;
using UnityEngine;

public class PlayerJoinScript : MonoBehaviour
{
    [SerializeField] private Transform SpawnPoint;
    [SerializeField] private GameObject Players;
    private void Awake()
    {
        Instantiate(Players, SpawnPoint.position, SpawnPoint.rotation);
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
