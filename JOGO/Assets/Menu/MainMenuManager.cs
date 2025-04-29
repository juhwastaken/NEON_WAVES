using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // Para usar IEnumerator

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

    [Header("Músicas")]
    public AudioSource menuMusic;      // Música do menu
    public AudioSource gameplayMusic;  // Música do jogo

    private void Start()
    {
        // Sensitivity pode ter valor quebrado, volume não
        sensitivitySlider.wholeNumbers = false;
        volumeSlider.wholeNumbers = true;

        // Ativa apenas o menu principal ao iniciar
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        gameUI.SetActive(false);
        gameplayElements.SetActive(false);
        player.SetActive(false);

        // Configurações carregadas
        float savedSensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        int savedVolume = PlayerPrefs.GetInt("Volume", 100);

        sensitivitySlider.value = savedSensitivity;
        volumeSlider.value = savedVolume;

        ApplySensitivity(savedSensitivity);
        ApplyVolume(savedVolume);

        UpdateSensitivityUI(savedSensitivity);
        UpdateVolumeUI(savedVolume);

        // Listeners para sliders
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

        // Toca a música do menu
        if (menuMusic != null)
        {
            menuMusic.Play();
        }
        else
        {
            Debug.LogWarning("menuMusic não está atribuído no inspector!");
        }

        // No menu, libera o cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // -------------------- Menu Principal --------------------

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gameUI.SetActive(true);
        gameplayElements.SetActive(true);
        player.SetActive(true);

        // Trava e esconde o cursor ao iniciar o jogo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Fade-out da música do menu
        if (menuMusic != null)
        {
            StartCoroutine(FadeOutAndStop(menuMusic, 1f));
        }
        else
        {
            Debug.LogWarning("menuMusic não está atribuído no inspector!");
        }

        if (gameplayMusic != null)
        {
            gameplayMusic.Play();
        }
        else
        {
            Debug.LogWarning("gameplayMusic não está atribuído no inspector!");
        }

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
            sensitivityValueText.text = value.ToString("F2");
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
            volumeValueText.text = $"{value}%";
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
        var camera = Camera.main;
        if (camera != null)
        {
            var playerCameraScript = camera.GetComponent<PlayerCamera>();
            if (playerCameraScript != null)
            {
                playerCameraScript.mouseSensitivity = value * 100f;
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

    private void OnApplicationQuit()
    {
        SaveSensitivity(sensitivitySlider.value);
        SaveVolume((int)volumeSlider.value);
    }

    // -------------------- Fade-Out da Música --------------------

    private IEnumerator FadeOutAndStop(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
