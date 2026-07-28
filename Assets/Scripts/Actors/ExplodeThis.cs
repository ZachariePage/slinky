using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.VFX;

public class ExplodeThis : MonoBehaviour
{
    [Header("Spline")]
    [SerializeField] private SplineContainer spline;
    private float t;
    
    [Header("Picture")]
    [SerializeField] private bool TakePicture;
    private int screenshotID = 0;
    [Header("explosion")] 
    [SerializeField] private int delayBeforeExplosion = 5;
    [SerializeField] private float TimeBeforeCleanUp = 5f;
    [SerializeField] private float breakForce = 5f;

    [Header("Retroaction")]
    [SerializeField] protected GameCue[] OnExplosionCues;
    [SerializeField] private GameCue DelayOnomatopia;
    [SerializeField] protected Sprite[] countDown;
    [SerializeField] private Transform vfxLocation;
    [SerializeField] private GameObject igniteVFX;
    private GameObject ignite;
    private bool exploded = false;
    private float startingTime;

    private float coldown = 1f;
    private float igniteTimer;
    
    [SerializeField]
    float fadeSpeed = 1f;
    
    public static event Action OnExplosion;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingTime = TimeBeforeCleanUp;
        igniteTimer = Time.time;
        if (vfxLocation == null)
        {
            vfxLocation = transform;
        }

        //StartCoroutine(Explode());
        
        CountdownTimer.StartCountdown(delayBeforeExplosion, OnTimerFinished);

        if (igniteVFX != null)
        {
            ignite = Instantiate(igniteVFX, transform.position, Quaternion.identity); 
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (TimeBeforeCleanUp >= 0f)
        {
            if (exploded)
            {
                TimeBeforeCleanUp -= Time.deltaTime;

                if (TimeBeforeCleanUp <= startingTime / 2)
                {
                    float fadeStart = startingTime / 2f;

                    float t = TimeBeforeCleanUp / fadeStart;
                    t = Mathf.Clamp01(t);

                    foreach (Transform child in transform)
                    {
                        Renderer rend = child.GetComponent<Renderer>();
                        if (rend != null)
                        {
                            Color color = rend.material.color;
                            color.a = t;
                            rend.material.color = color;
                        }
                    }
                }
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        if (t < 1f)
        {
            t += Time.fixedDeltaTime / delayBeforeExplosion;
            t = Mathf.Clamp01(t);
        }

        if (ignite != null)
        {
            float newTime = igniteTimer - Time.time;
            if (math.abs(newTime) > coldown)
            {
                ignite.GetComponent<VisualEffect>().Play();
                igniteTimer = Time.time;
            }
            UpdatePosition();
        }
    }

    void UpdatePosition()
    {
        if(ignite == null || spline == null) return;
        ignite.transform.position = spline.EvaluatePosition(t);
        ignite.transform.rotation = Quaternion.LookRotation(spline.EvaluateTangent(t));
    }

    public void OnTimerFinished()
    {
        foreach (GameCue cue in OnExplosionCues)
        {
            cue?.Execute(vfxLocation.position);
        }
        OnExplosion?.Invoke();
        
        exploded = true;
        foreach (Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            int slinkyLayer = LayerMask.NameToLayer("SlinkySegment");

            rb.excludeLayers = (1 << playerLayer) | (1 << slinkyLayer);
            rb.isKinematic = false;
            Vector3 force = (rb.transform.position - transform.position).normalized;
            rb.AddForce(force * breakForce, ForceMode.Impulse);
        }
        Destroy(ignite);
        StartCoroutine(TakePictureCoroutine());
    }

    IEnumerator TakePictureCoroutine()
    {
        if (TakePicture)
        {
            yield return new WaitForSeconds(0.2f);
            
            if(screenshotID == 0)
                screenshotID = ScreenshotManager.NextScreenshotID;
            ScreenshotManager.TakeScreenshot(screenshotID);
        }
        
        yield return null;
    }
    IEnumerator Explode()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject obj = DelayOnomatopia.Execute(vfxLocation.position);
            VisualEffect effect = obj.GetComponent<VisualEffect>();

            if (effect != null)
            {
                effect.SetTexture("Onomatopia", countDown[i].texture);
            }

            yield return new WaitForSeconds(1f);
        }

        foreach (GameCue cue in OnExplosionCues)
        {
            cue?.Execute(vfxLocation.position);
        }

        exploded = true;
        foreach (Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            int slinkyLayer = LayerMask.NameToLayer("SlinkySegment");

            rb.excludeLayers = (1 << playerLayer) | (1 << slinkyLayer);
            rb.isKinematic = false;
            Vector3 force = (rb.transform.position - transform.position).normalized;
            rb.AddForce(force * breakForce, ForceMode.Impulse);
        }

        if (TakePicture)
        {
            yield return new WaitForSeconds(0.2f);
            
            if(screenshotID == 0)
                screenshotID = ScreenshotManager.NextScreenshotID;
            ScreenshotManager.TakeScreenshot(screenshotID);
        }
        yield return null;
    }
}