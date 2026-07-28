using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float health;
    [SerializeField] private float startingHealth;
    
    [SerializeField] private float invincibilityColdown = 3;

    public bool SpawnCandies;
    public int ammount;
    [Header("Retroaction")]
    [SerializeField] protected GameCue[] OnDamageTakenCues;
    [SerializeField] protected GameCue[] OnDeathCues;
    private float timer;
    void Start()
    {
        health = startingHealth;
        timer = invincibilityColdown;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }

    public void TakeDamage(DamageInfo info)
    {
        if(!CanBeDamaged()) return;
        
        if(info.InstantKill)
        {
            Death();
        }
        
        health -= info.Amount;

        if (health <= 0)
        {
            Death();
        }
        timer =  invincibilityColdown;
        foreach (GameCue c in OnDamageTakenCues)
        {
            c?.Execute(transform.position);
        }
        //implement stun here
    }

    bool CanBeDamaged()
    {
        return timer <= 0;
    }

    void Death()
    {
        if (SpawnCandies)
        {
            CollectibleFactory.Instance.SpawnCollectiblesInBurst("BonBonWithRB", transform.position, ammount);
        }
        Destroy(gameObject);
        foreach (GameCue c in OnDeathCues)
        {
            c?.Execute(transform.position);
        }
    }
}
