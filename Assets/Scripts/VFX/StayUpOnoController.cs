using UnityEngine;
using UnityEngine.VFX;

public class StayUpOnoController : MonoBehaviour
{
    [Header("Bite VFX")]
    public VisualEffect onomatopievfx;
    public AnimationCurve scaleCurve;
    public float speed = 2f;
    public bool isHolding = true;
    public PlayerChomp owner;

    float progress = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        onomatopievfx = GetComponent<VisualEffect>();
        owner.onReleaseChompedEvent += OnRelease;
        isHolding = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (onomatopievfx != null)
        {
            float target = 0;
            if (isHolding)
            {
                target = 1;
            }
            progress = Mathf.MoveTowards(progress, target, Time.deltaTime * speed);

            float value = scaleCurve.Evaluate(progress);

            onomatopievfx.SetFloat("SpawnProgress", value);
        }

        if (!isHolding && progress <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    void OnRelease()
    {
        isHolding = false;
    }
}
