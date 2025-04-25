using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Referências UI e Gameplay")]
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject mainMenuPanel;
    public GameObject gameUI;
    public GameObject gameplayElements;
    public GameObject player;

    [Header("Música")]
    public AudioSource musicAudioSource; // Arraste aqui o AudioSource da música no Inspetor

    private bool isPaused = false;
    private MainMenuManager mainMenuManager;

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        else
            Debug.LogWarning("pausePanel não está atribuído no Inspetor!");

        Time.timeScale = 1f;

        // Encontra automaticamente o MainMenuManager na cena
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        if (mainMenuManager == null)
        {
            Debug.LogWarning("MainMenuManager não encontrado na cena!");
        }

        if (musicAudioSource == null)
        {
            Debug.LogWarning("musicAudioSource não está atribuído! A música não será pausada.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && player != null && player.activeSelf)
        {
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                CloseOptions();
                return;
            }

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (gameUI != null)
            gameUI.SetActive(false);

        if (musicAudioSource != null)
            musicAudioSource.Pause();

        Debug.Log("Jogo pausado");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(true);

        if (musicAudioSource != null)
            musicAudioSource.UnPause();

        Debug.Log("Jogo retomado");
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenOptions()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (mainMenuManager != null)
        {
            float sensValue = mainMenuManager.sensitivitySlider.value;
            int volumeValue = (int)mainMenuManager.volumeSlider.value;

            mainMenuManager.SaveSensitivity(sensValue);
            mainMenuManager.ApplySensitivity(sensValue);

            mainMenuManager.SaveVolume(volumeValue);
            mainMenuManager.ApplyVolume(volumeValue);
        }

        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        if (gameplayElements != null) gameplayElements.SetActive(false);
        if (player != null) player.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
}
