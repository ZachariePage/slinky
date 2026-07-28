using UnityEngine;
using UnityEngine.UI;


public class PauseUIBinding : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Fade")]
        [SerializeField] private Image fadeImage;

        [Header("Tabs")]
        [SerializeField] private GameObject graphicsTab;
        [SerializeField] private GameObject audioTab;
        [SerializeField] private GameObject controlsTab;

        [Header("In-Game UI")]
        [SerializeField] private GameObject inGameUI;

        [Header("Player Reactions")]
        [SerializeField] private GameObject player1IdleGO;
        [SerializeField] private GameObject player1OpenGO;
        [SerializeField] private GameObject player2IdleGO;
        [SerializeField] private GameObject player2OpenGO;

        private void OnEnable()
        {
            PauseMenuManager pm = FindAnyObjectByType<PauseMenuManager>();
            if (pm == null) return;

            pm.RegisterUI(pauseMenuPanel, settingsPanel, fadeImage,
                graphicsTab, audioTab, controlsTab,
                inGameUI,
                player1IdleGO, player1OpenGO,
                player2IdleGO, player2OpenGO);
        }
    }
