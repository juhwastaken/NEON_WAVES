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
    public TextMeshProUGUI sensitivityValueText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;

    void Start()
    {
        // Sensitivity pode ter valor quebrado, volume não
        sensitivitySlider.wholeNumbers = false;
        volumeSlider.wholeNumbers = true;

        // Ativa só o menu principal ao iniciar
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        gameUI.SetActive(false);
        gameplayElements.SetActive(false);
        player.SetActive(false);

        // Carrega configurações
        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        int savedVolume = PlayerPrefs.GetInt("Volume", 100);

        sensitivitySlider.value = savedSensitivity;
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume / 100f;

        // Atualiza textos
        UpdateSensitivityUI(savedSensitivity);
        UpdateVolumeUI(savedVolume);

        // Adiciona listeners
        sensitivitySlider.onValueChanged.AddListener(UpdateSensitivityUI);
        volumeSlider.onValueChanged.AddListener((value) => UpdateVolumeUI((int)value));
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
        SaveSettings();
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
        SaveSettings();
        Debug.Log("Saindo do jogo...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // -------------------- Configurações --------------------

    private void UpdateSensitivityUI(float value)
    {
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = value.ToString("F2"); // Ex: 2.45
        }
        else
        {
            Debug.LogWarning("Campo 'sensitivityValueText' não está atribuído.");
        }
    }

    private void UpdateVolumeUI(int value)
    {
        if (volumeValueText != null)
        {
            volumeValueText.text = $"{value}%"; // Ex: 75%
        }
        else
        {
            Debug.LogWarning("Campo 'volumeValueText' não está atribuído.");
        }

        AudioListener.volume = value / 100f;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
        PlayerPrefs.SetInt("Volume", (int)volumeSlider.value);
        PlayerPrefs.Save();
        Debug.Log("Configurações salvas.");
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }
}
