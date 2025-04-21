using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Sliders de Opções")]
    public Slider sensitivitySlider;
    public Slider volumeSlider;
    public TMP_Text sensitivityValueText;
    public TMP_Text volumeValueText;

    [Header("Objetos do jogo")]
    public GameObject player;
    public GameObject gameplayUI;
    public GameObject menuCamera;

    private float defaultSensitivity = 1f;
    private float defaultVolume = 1f;

    void Start()
    {
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        if (player != null) player.SetActive(false);
        if (gameplayUI != null) gameplayUI.SetActive(false);

        Time.timeScale = 1f;

        // Carregar os valores salvos do PlayerPrefs
        if (sensitivitySlider != null)
        {
            float sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
            sensitivitySlider.value = sensitivity;
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
            UpdateSensitivityText(sensitivity);
        }

        if (volumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("Volume", defaultVolume);
            volumeSlider.value = volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
            UpdateVolumeText(volume);
        }
    }

    public void OnStartButton()
    {
        Debug.Log("Botão Start pressionado.");

        // Certifique-se de que o menu principal está sendo desativado
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        // Desative a câmera do menu, pois o jogador precisa da câmera do jogo
        if (menuCamera != null) menuCamera.SetActive(false);

        // Ative o jogador e a UI do gameplay
        if (player != null) player.SetActive(true);
        if (gameplayUI != null) gameplayUI.SetActive(true);

        // Altere o tempo do jogo para o valor normal
        Time.timeScale = 1f;

        // Controle do cursor: desative o cursor no início do jogo
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Atualize o estado do jogo para indicar que começou
        GameState.hasStarted = true;

        Debug.Log("Jogo iniciado com sucesso!");
    }

    // Função para abrir o painel de opções
    public void OnOptionsButton()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    // Função para abrir o painel de créditos
    public void OnCreditsButton()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    // Função de voltar para o menu principal
    public void OnBackToMainMenu()
    {
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Função para sair do jogo
    public void OnExitButton()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }

    // Funções de controle de opções
    public void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("Sensitivity", value);
        PlayerPrefs.Save(); // Garantir que a alteração seja salva imediatamente
        Debug.Log("Sensibilidade ajustada para: " + value);
        UpdateSensitivityText(value);
    }

    public void SetVolume(float value)
    {
        PlayerPrefs.SetFloat("Volume", value);
        AudioListener.volume = value;
        PlayerPrefs.Save(); // Garantir que a alteração seja salva imediatamente
        Debug.Log("Volume ajustado para: " + value);
        UpdateVolumeText(value);
    }

    private void UpdateSensitivityText(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = value.ToString("F1");
    }

    private void UpdateVolumeText(float value)
    {
        if (volumeValueText != null)
            volumeValueText.text = value.ToString("F1");
    }

    // Novo método para botão de voltar no painel de créditos
    public void OnBackFromCredits()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
