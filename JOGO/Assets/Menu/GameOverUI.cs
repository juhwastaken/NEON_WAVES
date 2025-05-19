using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject gameOverPanel;
    public GameObject optionsPanel;

    [Header("Sliders de Opções")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueText;

    private const string SensKey = "Sensitivity";
    private const string VolKey = "Volume";
    private const string ShowGameOverKey = "ShowGameOver";

    void Start()
    {
        Time.timeScale = 1f;

        // Verifica se deve mostrar o painel de Game Over
        bool showGameOver = PlayerPrefs.GetInt(ShowGameOverKey, 0) == 1;

        gameOverPanel.SetActive(showGameOver);
        optionsPanel.SetActive(false);

        if (showGameOver)
        {
            // Limpa a flag para não exibir automaticamente na próxima vez
            PlayerPrefs.SetInt(ShowGameOverKey, 0);
            PlayerPrefs.Save();
        }

        // Carrega preferências salvas
        float savedSens = PlayerPrefs.GetFloat(SensKey, 5f);
        int savedVol = PlayerPrefs.GetInt(VolKey, 100);

        sensitivitySlider.value = savedSens;
        volumeSlider.value = savedVol;

        UpdateSensitivityText(savedSens);
        UpdateVolumeText(savedVol);

        AudioListener.volume = savedVol / 100f;

        // Atualiza textos em tempo real
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
        SceneManager.LoadScene("MainMenu");
    }

    public void OnOptionsPressed()
    {
        gameOverPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnBackFromOptions()
    {
        gameOverPanel.SetActive(true);
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
            sensitivityValueText.text = value.ToString("F1"); // Formato 0.0
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
