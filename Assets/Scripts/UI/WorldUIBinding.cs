using TMPro;
using UnityEngine;


    public class WorldUIBinding : MonoBehaviour
    {
        public enum UIRole { Player1Score, Player2Score, TotalCoins }

        [SerializeField] private UIRole role;

        private void OnEnable()
        {
            if (WorldManager.Instance == null) return;

            TMP_Text text = GetComponent<TMP_Text>();
            if (text == null) return;

            WorldManager.Instance.RegisterUI(role, text);
        }
    }