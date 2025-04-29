using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

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
    public AudioSource musicAudioSource;

    [Header("Vídeos da Barra de Vida")]
    public VideoPlayer healthBarVideoPlayer1;
    public VideoPlayer healthBarVideoPlayer2;

    [Header("Fade da HUD (opcional)")]
    public CanvasGroup hudCanvasGroup; // Adicione um CanvasGroup no gameUI!

    [Header("Timer")]
    public LevelTimer levelTimer; // <<< NOVO: Referência para o LevelTimer

    private bool isPaused = false;
    private MainMenuManager mainMenuManager;

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        else
            Debug.LogWarning("pausePanel não está atribuído no Inspetor!");

        Time.timeScale = 1f;

        mainMenuManager = FindObjectOfType<MainMenuManager>();
        if (mainMenuManager == null)
            Debug.LogWarning("MainMenuManager não encontrado na cena!");

        if (musicAudioSource == null)
            Debug.LogWarning("musicAudioSource não está atribuído!");

        if (hudCanvasGroup == null && gameUI != null)
        {
            hudCanvasGroup = gameUI.GetComponent<CanvasGroup>();
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

        if (hudCanvasGroup != null)
            StartCoroutine(FadeHUD(1f, 0f, 0.2f)); // Fade-out da HUD

        if (musicAudioSource != null)
            musicAudioSource.Pause();

        if (healthBarVideoPlayer1 != null)
            healthBarVideoPlayer1.Pause();

        if (healthBarVideoPlayer2 != null)
            healthBarVideoPlayer2.Pause();

        if (levelTimer != null) // <<< NOVO
            levelTimer.PauseTimer();

        Debug.Log("Jogo pausado");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (hudCanvasGroup != null)
            StartCoroutine(FadeHUD(0f, 1f, 0.2f)); // Fade-in da HUD

        if (musicAudioSource != null)
            musicAudioSource.UnPause();

        if (healthBarVideoPlayer1 != null)
            healthBarVideoPlayer1.Play();

        if (healthBarVideoPlayer2 != null)
            healthBarVideoPlayer2.Play();

        if (levelTimer != null) // <<< NOVO
            levelTimer.ResumeTimer();

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

    private System.Collections.IEnumerator FadeHUD(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        if (hudCanvasGroup == null)
            yield break;

        hudCanvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            hudCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        hudCanvasGroup.alpha = endAlpha;
    }
}