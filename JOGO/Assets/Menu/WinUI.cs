using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WinUI : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject winPanel;
    public GameObject optionsPanel;

    [Header("Sliders de Opções")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;

    private const string SensKey = "Sensitivity";
    private const string VolKey = "Volume";

    void Start()
    {
        Time.timeScale = 1f;

        winPanel.SetActive(true);
        optionsPanel.SetActive(false);

        // Carrega valores salvos
        float savedSens = PlayerPrefs.GetFloat(SensKey, 5f);
        int savedVol = PlayerPrefs.GetInt(VolKey, 100);

        sensitivitySlider.value = savedSens;
        volumeSlider.value = savedVol;

        UpdateSensitivityText(savedSens);
        UpdateVolumeText(savedVol);

        AudioListener.volume = savedVol / 100f;

        // Listeners
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    public void OnRetryPressed()
    {
        string lastScene = PlayerPrefs.GetString("LastGameplayScene", "Gameplay");
        SceneManager.LoadScene(lastScene);
    }

    public void OnQuitPressed()
    {
        SceneManager.LoadScene("MainMenu"); // Substitua pelo nome exato da sua cena de menu
    }

    public void OnOptionsPressed()
    {
        winPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnBackFromOptions()
    {
        winPanel.SetActive(true);
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
            sensitivityValueText.text = value.ToString("F1");
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
            volumeValueText.text = $"{value}%";
    }
}
