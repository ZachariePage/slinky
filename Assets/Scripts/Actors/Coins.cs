using UnityEngine;

public class Coins : MonoBehaviour,ICollectible
{
    public bool collected = false;
    
    [Header("Retroaction")]
    [SerializeField] protected GameCue[] OnCollectionCue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int Collect(GameObject actor)
    {
        if (collected) return 0;
        Debug.Log(gameObject.name + " collected");
        collected = true;
        
        foreach (GameCue gameCue in OnCollectionCue)
        {
            gameCue?.Execute(transform.position);
        }

        WorldManager.Instance.AddCoins(1);
        Destroy(gameObject);
        return 0;
    }
}
