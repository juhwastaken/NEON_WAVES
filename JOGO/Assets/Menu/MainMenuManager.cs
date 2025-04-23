using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;
    public GameObject gameUI;

    [Header("Gameplay")]
    public GameObject gameplayElements;
    public GameObject player;

    [Header("Options (Sliders e Textos)")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI SensitivityValueText;
    public Slider volumeSlider;
    public TextMeshProUGUI VolumeValueText;

    void Start()
    {
        // Ativa só o menu principal ao iniciar
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        gameUI.SetActive(false);
        gameplayElements.SetActive(false);
        player.SetActive(false);

        // Carrega configurações salvas
        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1.0f);

        sensitivitySlider.value = savedSensitivity;
        volumeSlider.value = savedVolume;

        UpdateSensitivityUI(savedSensitivity);
        UpdateVolumeUI(savedVolume);

        AudioListener.volume = savedVolume;
    }

    // -------------------- Menu Principal --------------------

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gameUI.SetActive(true);
        gameplayElements.SetActive(true);
        player.SetActive(true);
    }

    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
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

    // -------------------- Configurações --------------------

    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
        UpdateSensitivityUI(value);
    }

    public void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("Volume", value);
        UpdateVolumeUI(value);
        AudioListener.volume = value;
    }

    private void UpdateSensitivityUI(float value)
    {
        if (SensitivityValueText != null)
            SensitivityValueText.text = value.ToString("F2");
    }

    private void UpdateVolumeUI(float value)
    {
        if (VolumeValueText != null)
            VolumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
