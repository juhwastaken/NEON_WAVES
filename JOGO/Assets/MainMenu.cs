using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("NomeDaCenaDoJogo"); // Troca pelo nome real da cena do jogo
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Saindo do jogo...");
    }
}
