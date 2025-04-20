using UnityEngine;

public class PauseUILoader : MonoBehaviour
{
    public GameObject pauseUIPrefab;

    void Start()
    {
        if (pauseUIPrefab != null)
        {
            // Instancia o prefab
            GameObject pauseUI = Instantiate(pauseUIPrefab);
            pauseUI.SetActive(true); // Garante que está visível

            // Faz o pause UI persistir entre cenas
            DontDestroyOnLoad(pauseUI);

            Debug.Log("CanvasPause instanciado!");

            // Aqui, garantimos que estamos configurando o PauseManager corretamente
            if (PauseManager.Instance != null)
            {
                PauseManager.Instance.pauseMenuUI = pauseUI.transform.Find("PauseMenuUI").gameObject;
                PauseManager.Instance.optionsMenuUI = pauseUI.transform.Find("MenuOptions").gameObject;

                // Agora podemos definir o ready
                PauseManager.Instance.ready = true;
                Debug.Log("PauseUILoader terminou. pauseMenuUI e optionsMenuUI setados!");
            }
            else
            {
                Debug.LogError("PauseManager não encontrado na cena!");
            }
        }
        else
        {
            Debug.LogWarning("Pause UI Prefab não atribuído no Inspector!");
        }
    }
}
