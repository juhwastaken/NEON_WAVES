using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	[SerializeField] private string nomeDoLevelDeJogo;
	[SerializeField] private GameObject painelMenuInicial;
	[SerializeField] private GameObject painelOpcoes;

public void Play()
{
	SceneManager.LoadScene(nomeDoLevelDeJogo);
}

public void OpenOptions()
{
	painelMenuInicial.SetActive(false);
	painelOpcoes.SetActive(true);
}

public void CloseOptions()
{
	painelOpcoes.SetActive(false);
	painelMenuInicial.SetActive(true);
}

public void CloseGame()
{
	Debug.Log("Sair do Jogo");
	Application.Quit();
}

}
