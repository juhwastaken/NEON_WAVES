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
        SaveSettings(); // Salva ao fechar as opções
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
        SaveSettings(); // Salva ao sair do jogo
        Debug.Log("Saindo do jogo...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // -------------------- Configurações --------------------

    public void OnSensitivityChanged(float value)
    {
        UpdateSensitivityUI(value);
    }

    public void OnVolumeChanged(float value)
    {
        UpdateVolumeUI(value);
        AudioListener.volume = value;
    }

    private void UpdateSensitivityUI(float value)
    {
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = value.ToString("F2");
            sensitivityValueText.ForceMeshUpdate();
        }
        else
        {
            Debug.LogWarning("Campo 'sensitivityValueText' não está atribuído no Inspector.");
        }
    }

    private void UpdateVolumeUI(float value)
    {
        if (volumeValueText != null)
        {
            volumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";
            volumeValueText.ForceMeshUpdate();
        }
        else
        {
            Debug.LogWarning("Campo 'volumeValueText' não está atribuído no Inspector.");
        }
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.Save();
        Debug.Log("Configurações salvas automaticamente.");
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }
}
