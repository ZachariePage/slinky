using UnityEngine;

public class Collectible : MonoBehaviour, ICollectible
{
    [SerializeField] private int ScoreGiven;
    public bool collected = false;
    
    [Header("Retroaction")]
    [SerializeField] protected GameCue[] OnCollectionCue;
    
    private void OnEnable()
    {
        collected = false;
    }
    
    public virtual int Collect(GameObject actor)
    {
        if (collected) return 0;
        collected = true;

        foreach (GameCue gameCue in OnCollectionCue)
        {
            gameCue?.Execute(transform.position);
        }

        PlayerState ps = actor.GetComponent<PlayerState>();
        if (ps != null)
        {
            ps.AddScore(ScoreGiven);
        }
        ObjectPool.ReturnToPool(gameObject);
        //Destroy(gameObject);
        return ScoreGiven;
    }
}
