using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;      // Painel do menu de pausa
    public GameObject optionsPanel;    // Painel do menu de opções
    public GameObject gameplayUI;      // Elementos de UI que somem ao pausar

    [Header("Options UI")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;
    public Button backButton;

    private bool isPaused = false;

    private const string SensKey = "Sensitivity";
    private const string VolKey = "Volume";

    void Start()
    {
        // Carregar valores salvos
        float savedSens = PlayerPrefs.GetFloat(SensKey, 5f);
        int savedVol = PlayerPrefs.GetInt(VolKey, 100);

        sensitivitySlider.value = savedSens;
        volumeSlider.value = savedVol;

        UpdateSensitivityText(savedSens);
        UpdateVolumeText(savedVol);

        AudioListener.volume = savedVol / 100f;

        if (backButton != null)
            backButton.onClick.AddListener(CloseOptions);

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(true);
    }

    void Update()
    {
        // Bloqueia o ESC se o tutorial ainda estiver ativo
        if (TutorialOverlay.TutorialAtivo)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Fecha o tutorial manualmente
                var tutorial = FindObjectOfType<TutorialOverlay>();
                if (tutorial != null)
                    tutorial.FecharTutorial();
            }

            return; // Não processa o pause se o tutorial está ativo
        }

        // ESC normal para pausar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)
            {
                CloseOptions();
                return;
            }

            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;

        pausePanel.SetActive(true);
        if (gameplayUI != null) gameplayUI.SetActive(false);
        optionsPanel.SetActive(false);

        FindObjectOfType<LevelTimer>()?.PauseTimer();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        pausePanel.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(true);
        optionsPanel.SetActive(false);

        FindObjectOfType<LevelTimer>()?.ResumeTimer();
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Substitua pelo nome correto da cena de menu
    }

    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    // === Sensibilidade ===
    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(SensKey, value);
        UpdateSensitivityText(value);
    }

    private void UpdateSensitivityText(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = $"Sensibilidade: {value:F1}";
    }

    // === Volume ===
    public void OnVolumeChanged(float value)
    {
        int intVal = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt(VolKey, intVal);
        AudioListener.volume = intVal / 100f;
        UpdateVolumeText(intVal);
    }

    private void UpdateVolumeText(int value)
    {
        if (volumeValueText != null)
            volumeValueText.text = $"Volume: {value}%";
    }
}
