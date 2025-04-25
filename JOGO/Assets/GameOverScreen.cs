using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("Referências")]
    public GameObject optionsPanel; // painel de opções (dentro da mesma cena)

    [Header("Cena do menu principal")]
    public string mainMenuSceneName = "MainMenu";

    public void ShowGameOver()
    {
        Time.timeScale = 0f; // pausa o jogo
        gameObject.SetActive(true);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
