using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuUI;     // Painel do menu principal
    public GameObject optionsMenuUI;  // Painel das opções

    public void PlayGame()
    {
        SceneManager.LoadScene("NomeDaCenaDoJogo"); // Substitua pelo nome da sua cena
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Saindo do jogo...");
    }

    public void OpenOptions()
    {
        mainMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void BackToMenu()
    {
        optionsMenuUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }
}