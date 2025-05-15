using UnityEngine;
using UnityEngine.UI;

public class TutorialOverlay : MonoBehaviour
{
    public static bool TutorialAtivo { get; private set; }

    [Header("UI")]
    public GameObject tutorialPanel;
    public Toggle naoMostrarToggle;
    public Button fecharBotao;

    [Header("Gameplay UI")]
    public GameObject gameplayUI; // <== Referência à UI principal do jogo

    private const string TutorialKey = "TutorialVisto";

    void Start()
    {
        bool jaVisto = PlayerPrefs.GetInt(TutorialKey, 0) == 1;

        if (jaVisto)
        {
            tutorialPanel.SetActive(false);
            if (gameplayUI != null) gameplayUI.SetActive(true); // Garante que a UI esteja visível
            return;
        }

        // Mostrar tutorial
        tutorialPanel.SetActive(true);
        Time.timeScale = 0f;
        GameplayMusicPlayer.Instance?.PausarMusica();

        if (gameplayUI != null)
            gameplayUI.SetActive(false); // Oculta a UI do jogo enquanto o tutorial está ativo

        fecharBotao.onClick.AddListener(FecharTutorial);

        TutorialAtivo = !jaVisto;

    }

    public void FecharTutorial()
    {
        if (naoMostrarToggle.isOn)
        {
            PlayerPrefs.SetInt(TutorialKey, 1);
            PlayerPrefs.Save();
        }

        tutorialPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true); // Reativa UI

        Time.timeScale = 1f;
        GameplayMusicPlayer.Instance?.ContinuarMusica();
        
        TutorialAtivo = false;

    }
}
