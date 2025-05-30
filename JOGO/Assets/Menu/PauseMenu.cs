using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject gameplayUI;

    [Header("Options UI")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;
    public Button backButton;

    [Header("Audio")]
    public AudioSource musicSource; // Arraste o AudioSource da música aqui via Inspector

    private bool isPaused = false;

    private const string SensKey = "Sensitivity";
    private const string VolKey = "Volume";

    void Start()
    {
        float savedSens = PlayerPrefs.GetFloat(SensKey, 5f);
        int savedVol = PlayerPrefs.GetInt(VolKey, 100);

        sensitivitySlider.value = savedSens;
        volumeSlider.value = savedVol;

        UpdateSensitivityText(savedSens);
        UpdateVolumeText(savedVol);

        AudioListener.volume = savedVol / 100f;

        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (backButton != null)
            backButton.onClick.AddListener(CloseOptions);

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(true);

        // Se não for atribuído manualmente, tenta encontrar por tag
        if (musicSource == null)
            musicSource = GameObject.FindGameObjectWithTag("Music")?.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (TutorialOverlay.TutorialAtivo)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var tutorial = FindObjectOfType<TutorialOverlay>();
                if (tutorial != null)
                    tutorial.FecharTutorial();
            }

            return;
        }

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

        if (musicSource != null && musicSource.isPlaying)
            musicSource.Pause();

            CursorManager.LiberarCursor();

    }

    public void ResumeGame()
{
    Time.timeScale = 1f;
    isPaused = false;

    pausePanel.SetActive(false);
    if (gameplayUI != null) gameplayUI.SetActive(true);
    optionsPanel.SetActive(false);

    FindObjectOfType<LevelTimer>()?.ResumeTimer();

    // Só despausa a música se o tutorial já estiver fechado
    if (!TutorialOverlay.TutorialAtivo && musicSource != null && !musicSource.isPlaying)
    {
        musicSource.UnPause();
    }

    CursorManager.TravarCursor();
}


    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
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

    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(SensKey, value);
        UpdateSensitivityText(value);
    }

    private void UpdateSensitivityText(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = value.ToString("F1");
    }

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
            volumeValueText.text = $"{value}%";
    }
}
