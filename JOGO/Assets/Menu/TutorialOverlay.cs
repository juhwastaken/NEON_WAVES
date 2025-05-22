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
    public GameObject gameplayUI;

    private const string TutorialKey = "TutorialVisto";

    void Start()
    {
        // Garante que o painel está ativo no editor
        if (tutorialPanel == null)
        {
            Debug.LogError("Tutorial Panel não está atribuído.");
            return;
        }

        bool jaVisto = PlayerPrefs.GetInt(TutorialKey, 0) == 1;

        if (jaVisto)
        {
            tutorialPanel.SetActive(false);
            if (gameplayUI != null) gameplayUI.SetActive(true);
            TutorialAtivo = false;
            return;
        }

        // Mostrar tutorial
        tutorialPanel.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameplayMusicPlayer.Instance?.PausarMusica();

        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        if (fecharBotao != null)
            fecharBotao.onClick.AddListener(FecharTutorial);

        TutorialAtivo = true;
    }

    public void FecharTutorial()
    {
        if (naoMostrarToggle != null && naoMostrarToggle.isOn)
        {
            PlayerPrefs.SetInt(TutorialKey, 1);
            PlayerPrefs.Save();
        }

        tutorialPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameplayMusicPlayer.Instance?.ContinuarMusica();

        TutorialAtivo = false;
    }
}
