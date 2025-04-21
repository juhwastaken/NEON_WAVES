using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public string mainMenuSceneName = "MainMenu";  // Nome da cena do menu principal

    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Verifica se o jogo já começou, baseado no GameState
        if (!GameState.hasStarted) return;

        // Verifica se o jogador pressionou ESC para pausar ou abrir as opções
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)
                BackFromOptions();
            else
                TogglePause();
        }
    }

    // Função de alternar pausa
    public void TogglePause()
    {
        isPaused = !isPaused;

        // Ativa ou desativa o painel de pausa e ajusta a velocidade do tempo
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        // Controle do cursor
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // Função para retomar o jogo
    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Função para reiniciar o nível
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Função para abrir o painel de opções
    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    // Função para voltar do painel de opções
    public void BackFromOptions()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    // Função para voltar ao menu principal
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
