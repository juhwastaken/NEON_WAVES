using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Painéis do Menu")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Cena de Jogo")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Sliders de Opções")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;

    [Header("Música do Menu")]
    public AudioSource menuMusic;

    private void Start()
    {
        // Sensibilidade
        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        sensitivitySlider.value = savedSensitivity;
        UpdateSensitivityText(savedSensitivity);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        // Volume
        int savedVolumeInt = PlayerPrefs.GetInt("VolumePercent", 100);
        float volume = savedVolumeInt / 100f;
        volumeSlider.value = savedVolumeInt;
        AudioListener.volume = volume;
        UpdateVolumeText(savedVolumeInt);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    public void OnStartButton()
    {
        if (menuMusic != null)
            menuMusic.Stop();

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnOptionsButton()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnCreditsButton()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void OnBackButton()
    {
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
        UpdateSensitivityText(value);
    }

    private void OnVolumeChanged(float value)
    {
        int intVolume = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt("VolumePercent", intVolume);
        AudioListener.volume = intVolume / 100f;
        UpdateVolumeText(intVolume);
    }

    private void UpdateSensitivityText(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = value.ToString("F2"); // Apenas número, 2 casas
    }

    private void UpdateVolumeText(int value)
    {
        if (volumeValueText != null)
            volumeValueText.text = value + "%"; // Ex: 75%
    }
}
