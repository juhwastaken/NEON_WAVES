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

        // Aplica valores
        ApplySensitivity(savedSensitivity);
        ApplyVolume(savedVolume);

        // Atualiza textos
        UpdateSensitivityUI(savedSensitivity);
        UpdateVolumeUI(savedVolume);

        // Listeners com salvamento e aplicação
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
    }

    // -------------------- Menu Principal --------------------

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gameUI.SetActive(true);
        gameplayElements.SetActive(true);
        player.SetActive(true);

        // Garante aplicação dos valores ao iniciar o gameplay
        ApplySensitivity(sensitivitySlider.value);
        ApplyVolume((int)volumeSlider.value);
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

    public void UpdateSensitivityUI(float value)
    {
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = value.ToString("F2"); // Ex: 1.25
        }
        else
        {
            Debug.LogWarning("Campo 'sensitivityValueText' não está atribuído.");
        }
    }

    public void UpdateVolumeUI(int value)
    {
        if (volumeValueText != null)
        {
            volumeValueText.text = $"{value}%"; // Ex: 75%
        }
        else
        {
            Debug.LogWarning("Campo 'volumeValueText' não está atribuído.");
        }
    }

    public void SaveSensitivity(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
        PlayerPrefs.Save();
    }

    public void ApplySensitivity(float value)
    {
        // Aplica a sensibilidade no script PlayerCamera
        var camera = Camera.main;
        if (camera != null)
        {
            var playerCameraScript = camera.GetComponent<PlayerCamera>();
            if (playerCameraScript != null)
            {
                playerCameraScript.mouseSensitivity = value * 100f; // Escala para valores esperados
            }
        }
    }

    public void SaveVolume(int value)
    {
        PlayerPrefs.SetInt("Volume", value);
        PlayerPrefs.Save();
    }

    public void ApplyVolume(int value)
    {
        AudioListener.volume = value / 100f;
    }

    public void OnApplicationQuit()
    {
        SaveSensitivity(sensitivitySlider.value);
        SaveVolume((int)volumeSlider.value);
    }
}
