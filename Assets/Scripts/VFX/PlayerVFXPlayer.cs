using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFXPlayer : MonoBehaviour
{
    [SerializeField] public Transform playerFeet;
    [SerializeField] public Transform aboveHead;
    [Header("Speed VFX")]
    [SerializeField] private VisualEffectAsset speedVfx;
    [SerializeField] private float speedNeededToAppear = 3;
    private VisualEffect speedObj;

    [SerializeField] private float spawnParticledDelay = 0.2f;

    private float timerSpeed;
    
    [SerializeField] 
    private GameCue[] playerJumpCues;
    
    [Header("Land VFX")]
    public LayerMask layerMask;
    [SerializeField] private GameCue[] onLandGameCues;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color groundedColor;
    [SerializeField] private Color waterColor;
    [SerializeField] private Color streetColor;
    [SerializeField] private Color grassColor;
    
    [Header("Slingshot VFX")]
    [SerializeField] private GameObject slingshotVfx;
    private ParticleSystem slingshotParticles;
    private GameObject slingshotGO;
    private float lastTimeSlingshot;
    [SerializeField] private GameCue[] onSlingshotGameCues;

    private float timerSlingshot;

    [Header("Onomatopeia VFX")]
    [SerializeField] private Onomatopeia onomatopeia;
    [SerializeField] private Onomatopeia onomatopeiaStayUp;
    private SlinAndKyControllerBase controller;
    private GameObject onoGo;

    private Rigidbody rb;
    
    
    void Start()
    {
        controller = GetComponent<SlinAndKyControllerBase>();
        controller.OnLanded += PlayOnLandVFX;
        controller.OnSlingshot += OnSlingshot;
        controller.OnEndSlingshot += OnEndSlingshot;
        controller.OnPlayerJump += OnPlayerJump;
        

        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnPlayerDie += OnPlayerDie;
        }

        PlayerChomp pc = GetComponent<PlayerChomp>();
        pc.onReleaseChompedEvent += OnBiteRelease;
        
        rb = GetComponent<Rigidbody>();
        
        //run
        timerSpeed = spawnParticledDelay;
        //slingshot
        slingshotGO = Instantiate(slingshotVfx, playerFeet.transform.position, Quaternion.identity); 
        slingshotGO.transform.SetParent(gameObject.transform);
        slingshotGO.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        slingshotParticles =  slingshotGO.GetComponent<ParticleSystem>();
        slingshotGO.SetActive(false);
    }

    void Update()
    {
        if(rb == null || controller == null) return;
        bool IsGrounded = controller.GetIsGrounded();
        float speed = rb.linearVelocity.magnitude;
        //smoke
        if (timerSpeed > 0)
        {
            if (IsGrounded)
            {
                timerSpeed -= Time.deltaTime;
            }
        }
        else
        {
            timerSpeed = spawnParticledDelay;
            SpawnSmokeParticle();
        }
        
        //slingshot

        SpawnSlingshotParticle();
        
    }

    private void SpawnSlingshotParticle()
    {
        SlinkyManager slinky = FindFirstObjectByType<SlinkyManager>(); 
        if(slingshotParticles == null || slingshotGO == null || slinky == null) return;
        
        if (SlinkyManager.CurrentZone == SlinkyManager.SlinkyZone.Hard)
        {
            slingshotGO.SetActive(true);
            if (!slingshotParticles.isPlaying)
            {
                slingshotParticles.Play();
            }
        }
        else
        {
            slingshotGO.SetActive(false);
        }
    }

    void SpawnSmokeParticle()
    {
        Vector3 vspeed = controller.GetComponent<Rigidbody>().linearVelocity;
        vspeed.y = 0;
        float speed = vspeed.magnitude;
        bool IsGrounded = controller.GetIsGrounded();
        
        if(!((speed > speedNeededToAppear) && IsGrounded)) return;
        
        GameObject obj = ObjectPool.Instance.GetObjectWithName("Smoke Trail");
        obj.transform.position = playerFeet.position;
        obj.GetComponent<VisualEffect>().Play();
        obj.transform.rotation = transform.rotation * Quaternion.Euler(0f, 270f, 0f);
        Color color = defaultColor;
        
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 5f, layerMask))
        {
            GroundType groundType = hit.collider.GetComponent<GroundType>();
            if (groundType != null)
            {
                switch (groundType.type)
                {
                    case TerrainType.grass:
                        color = grassColor;
                        break;
                    case TerrainType.ground:
                        color = groundedColor;
                        break;
                    case TerrainType.street:
                        color = streetColor;
                        break;
                    case TerrainType.water:
                        color = waterColor;
                        break;
                }
            }
        }
        obj.GetComponent<VisualEffect>().SetVector4("TrailColor", color);
        ObjectPool.Instance.StartCoroutine(ObjectPool.ReturnToPoolAfterDelay(obj, 1f));
    }

    void PlayOnLandVFX()
    {
        foreach (GameCue gc in onLandGameCues)
        {
            gc?.Execute(playerFeet.position);
        }
    }

    void OnSlingshot()
    {
        foreach (GameCue gc in onSlingshotGameCues)
        {
            gc?.Execute(transform.position);
        }
    }

    void OnEndSlingshot()
    {
        
    }

    void OnPlayerJump(SlinAndKyControllerBase.PlayerNumber playerNum)
    {
        foreach (GameCue gc in playerJumpCues)
        {
            gc?.Execute(transform.position);
        }
    }

    public GameObject PlayOnomatopeia(Sprite sprite, Transform transform)
    {
        onoGo = onomatopeia?.Execute(transform.position, sprite);
        return onoGo;
    }
    
    public GameObject PlayStayOnomatopeia(Sprite sprite, Transform transform)
    {
        onoGo = onomatopeiaStayUp?.Execute(transform.position, sprite);
        return onoGo;
    }

    public void OnBiteRelease()
    {
        if (onoGo != null)
        {
            
        }
    }

    private void OnPlayerDie()
    {
        
    }
}