using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Video;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject mainMenuPanel;
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;
    public GameObject gameOverPanel;
    public GameObject gameUI;

    [Header("Gameplay")]
    public GameObject gameplayElements;
    public GameObject player;

    [Header("Músicas")]
    public AudioSource menuMusic;
    public AudioSource gameplayMusic;
    public AudioSource gameOverMusic;

    [Header("Options (Sliders e Textos)")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;

    [Header("Extras")]
    public CanvasGroup hudCanvasGroup;
    public VideoPlayer healthBarVideoPlayer1;
    public VideoPlayer healthBarVideoPlayer2;
    public LevelTimer levelTimer;

    [Header("Cena do menu principal")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Start()
    {
        // Inicializar sliders
        sensitivitySlider.wholeNumbers = false;
        volumeSlider.wholeNumbers = true;

        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        int savedVolume = PlayerPrefs.GetInt("Volume", 100);
        sensitivitySlider.value = savedSensitivity;
        volumeSlider.value = savedVolume;

        ApplySensitivity(savedSensitivity);
        ApplyVolume(savedVolume);
        UpdateSensitivityUI(savedSensitivity);
        UpdateVolumeUI(savedVolume);

        sensitivitySlider.onValueChanged.AddListener((value) =>
        {
            UpdateSensitivityUI(value);
            SaveSensitivity(value);
            ApplySensitivity(value);
        });

        volumeSlider.onValueChanged.AddListener((value) =>
        {
            int intValue = (int)value;
            UpdateVolumeUI(intValue);
            SaveVolume(intValue);
            ApplyVolume(intValue);
        });

        // Estado inicial dos painéis
        mainMenuPanel.SetActive(true);
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameUI.SetActive(false);
        gameplayElements.SetActive(false);
        player.SetActive(false);

        menuMusic?.Play();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && player.activeSelf)
        {
            if (optionsPanel.activeSelf)
            {
                CloseOptions();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // -------------------- MENU PRINCIPAL --------------------

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gameUI.SetActive(true);
        gameplayElements.SetActive(true);
        player.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (menuMusic != null)
            StartCoroutine(FadeOutAndStop(menuMusic, 1f));

        gameplayMusic?.Play();

        ApplySensitivity(sensitivitySlider.value);
        ApplyVolume((int)volumeSlider.value);
    }

    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);

        if (isPaused)
            pausePanel.SetActive(true);
        else if (gameOverPanel.activeSelf)
            gameOverPanel.SetActive(true);
        else
            mainMenuPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // -------------------- PAUSE MENU --------------------

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);

        if (hudCanvasGroup != null)
            StartCoroutine(FadeHUD(1f, 0f, 0.2f));

        gameplayMusic?.Pause();
        healthBarVideoPlayer1?.Pause();
        healthBarVideoPlayer2?.Pause();
        levelTimer?.PauseTimer();

        Debug.Log("Jogo pausado");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);

        if (hudCanvasGroup != null)
            StartCoroutine(FadeHUD(0f, 1f, 0.2f));

        gameplayMusic?.UnPause();
        healthBarVideoPlayer1?.Play();
        healthBarVideoPlayer2?.Play();
        levelTimer?.ResumeTimer();

        Debug.Log("Jogo retomado");
    }

    public void ExitToMainMenu() // Botão da tela de PAUSE
    {
        Time.timeScale = 1f;
        isPaused = false;

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        gameUI.SetActive(false);
        gameplayElements.SetActive(false);
        player.SetActive(false);
        mainMenuPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameplayMusic?.Stop();
        menuMusic?.Play();
    }

    // -------------------- GAME OVER --------------------

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);

        gameplayMusic?.Stop();
        gameOverMusic?.Play();
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMainMenu() // Botão da tela de GAME OVER
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // -------------------- CONFIGURAÇÕES --------------------

    private void UpdateSensitivityUI(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = value.ToString("F2");
    }

    private void UpdateVolumeUI(int value)
    {
        if (volumeValueText != null)
            volumeValueText.text = $"{value}%";
    }

    private void SaveSensitivity(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
        PlayerPrefs.Save();
    }

    private void ApplySensitivity(float value)
    {
        var camera = Camera.main;
        if (camera != null)
        {
            var playerCameraScript = camera.GetComponent<PlayerCamera>();
            if (playerCameraScript != null)
                playerCameraScript.mouseSensitivity = value * 100f;
        }
    }

    private void SaveVolume(int value)
    {
        PlayerPrefs.SetInt("Volume", value);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(int value)
    {
        AudioListener.volume = value / 100f;
    }

    private IEnumerator FadeOutAndStop(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.unscaledDeltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    private IEnumerator FadeHUD(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        hudCanvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            hudCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        hudCanvasGroup.alpha = endAlpha;
    }

    private void OnApplicationQuit()
    {
        SaveSensitivity(sensitivitySlider.value);
        SaveVolume((int)volumeSlider.value);
    }
}
