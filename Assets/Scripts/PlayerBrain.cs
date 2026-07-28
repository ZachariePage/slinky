using System;
using UnityEngine;

public class PlayerBrain : MonoBehaviour
{
    private PlayerState playerState;
    [SerializeField]private int PlayerId;
    [SerializeField]private PlayerSoundBank playerSoundBank;
    [SerializeField]private PlayerVFXBank playerVFXBank;
    [SerializeField] private GameObject crown;

    private PlayerVFXPlayer VFXPlayer;

    private void Awake()
    {
        if(playerSoundBank == null || playerVFXBank == null)
        {
            Debug.LogError("No player sound/vfx bank assigned");
        }
        
        //NullCheckUtility.CheckForNullFields(playerSoundBank);
        //NullCheckUtility.CheckForNullFields(playerVFXBank);
        
        VFXPlayer =  GetComponent<PlayerVFXPlayer>();
    }

    void Start()
    {
        playerState = GetComponent<PlayerState>();
        Init(PlayerId);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Init(int id)
    {
        PlayerId = id;
        
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.RegisterPlayer(PlayerId);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherObj = other.gameObject;
        if (other.CompareTag("Collectible"))
        {
            ICollectible collectible = other.GetComponent<ICollectible>();
            if (otherObj.GetComponent<Collectible>() != null)
            {
                SpawnMunchVFX();
            }
            if (collectible != null)
            {
                int score = collectible.Collect(gameObject);
            }
            
            
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        GameObject otherObj = other.gameObject;
        if (otherObj.CompareTag("Collectible"))
        {
            ICollectible collectible = otherObj.GetComponent<ICollectible>();

            if (otherObj.GetComponent<Collectible>() != null)
            {
                SpawnMunchVFX();
            }
            if (collectible != null)
            {
                int score = collectible.Collect(gameObject);
            }

            
        }
    }

    private void SpawnMunchVFX()
    {
        VFXPlayer.PlayOnomatopeia(GetPlayerVFXBank().EatCandy, transform);
    }
    public int GetPlayerId()
    {
        return PlayerId;
    }

    public PlayerSoundBank GetPlayerSoundBank()
    {
        return playerSoundBank;
    }

    public PlayerVFXBank GetPlayerVFXBank()
    {
        return playerVFXBank;
    }

    public GameObject SetCrown(bool value)
    {
        crown.SetActive(value);
        return crown;
    }
}
