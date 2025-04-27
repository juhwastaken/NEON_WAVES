using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("Referências")]
    public GameObject optionsPanel; // painel de opções (dentro da mesma cena)

    [Header("Cena do menu principal")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Músicas")]
    public AudioSource gameplayMusic;    // Música que tá tocando durante o jogo
    public AudioSource gameOverMusic;    // Música da tela de morte

    public void ShowGameOver()
    {
        // Pausa o jogo
        Time.timeScale = 0f;
        gameObject.SetActive(true);

        // Troca a música
        if (gameplayMusic != null)
        {
            gameplayMusic.Stop();
        }
        else
        {
            Debug.LogWarning("GameplayMusic não está atribuído!");
        }

        if (gameOverMusic != null)
        {
            gameOverMusic.Play();
        }
        else
        {
            Debug.LogWarning("GameOverMusic não está atribuído!");
        }
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
