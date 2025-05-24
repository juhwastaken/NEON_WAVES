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

    [Header("Configuração de Teste")]
    public bool resetarPrefsNoComeco = false;

    private const string TutorialKey = "TutorialVisto";

    void Start()
    {
        Debug.Log("TutorialOverlay Start chamado");

        // Resetar PlayerPrefs se ativado para testes
        if (resetarPrefsNoComeco)
        {
            PlayerPrefs.DeleteKey(TutorialKey);
            PlayerPrefs.Save();
            Debug.LogWarning("TutorialKey deletado para testes.");
        }

        // Garante que o painel está ativo no editor
        if (tutorialPanel == null)
        {
            Debug.LogError("Tutorial Panel não está atribuído.");
            return;
        }

        bool jaVisto = PlayerPrefs.GetInt(TutorialKey, 0) == 1;
        Debug.Log("Tutorial já visto? " + jaVisto);

        if (jaVisto)
        {
            tutorialPanel.SetActive(false);
            if (gameplayUI != null) gameplayUI.SetActive(true);
            TutorialAtivo = false;
            return;
        }

        // Mostrar tutorial
        Debug.Log("Mostrando tutorial...");
        tutorialPanel.SetActive(true);
        Time.timeScale = 0f;

        CursorManager.LiberarCursor();

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
            Debug.Log("Tutorial marcado como visto.");
        }

        tutorialPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        Time.timeScale = 1f;

        CursorManager.TravarCursor();

        GameplayMusicPlayer.Instance?.ContinuarMusica();

        TutorialAtivo = false;
    }
}
