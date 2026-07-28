using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StickyEnemies : Unit
{
    [Header("Sticky Enemies")]
    [Header("Hit")]
    [SerializeField] private float hitForce;
    [SerializeField] private float ColdownBetweenHit;
    private Dictionary<GameObject, float> cooldowns = new Dictionary<GameObject, float>();



    protected override void Initialize()
    {
        stateMachine.Init(chaseState);
    }

    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (cooldowns.Count > 0)
        {
            var keys = new List<GameObject>(cooldowns.Keys);
            foreach (GameObject go in keys)
            {
                cooldowns[go] -= Time.deltaTime;
                if (cooldowns[go] <= 0f)
                {
                    cooldowns.Remove(go);
                }
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);
    }

    public void OnOtherCollision(GameObject other)
    {
        if (cooldowns.ContainsKey(other))
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            OnPlayerHit(other);
        }
        cooldowns[other] = ColdownBetweenHit;
    }

    public void OnPlayerHit(GameObject player)
    {
        if (cooldowns.ContainsKey(player))
        {
            return;
        }
        Vector3 direction = (player.transform.position - transform.position).normalized;
        player.GetComponent<Rigidbody>().AddForce(direction * hitForce, ForceMode.Impulse);
        cooldowns[player] = ColdownBetweenHit;
    }
}
