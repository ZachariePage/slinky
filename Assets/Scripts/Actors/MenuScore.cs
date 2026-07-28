using TMPro;
using UnityEngine;

public class MenuScore : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text player1ScoreText;
    [SerializeField] private TMP_Text player2ScoreText;
    [SerializeField] private TMP_Text totalCoinsText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player1ScoreText.text = WorldManager.Instance.GetPlayerScore(0).ToString();
        player2ScoreText.text = WorldManager.Instance.GetPlayerScore(1).ToString();
        totalCoinsText.text = WorldManager.Instance.GetTotalCoins().ToString();
        WorldManager.Instance.ResetAllScores();
        Debug.Log(WorldManager.Instance.GetPlayerScore(0));
        Debug.Log(WorldManager.Instance.GetPlayerScore(1));
        Debug.Log(WorldManager.Instance.GetTotalCoins());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
