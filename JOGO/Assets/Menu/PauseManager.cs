using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;       // Painel principal do menu de pausa
    public GameObject optionsMenuUI;     // Menu de opções já existente (em outro canvas)
    public static PauseManager Instance;

    private bool isPaused = false;
    public bool ready = false;

    void Awake()
    {
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }



    void Update()
    {
        if (!ready) return; // Aguarda o UI ser carregado

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressionado");
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }


    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false); // Garante que o menu de opções seja fechado
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        if (pauseMenuUI == null)
        {
            Debug.LogError("pauseMenuUI está nulo ao tentar pausar!");
            return;
        }

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Retry()
    {
        Time.timeScale = 1f; // Despausa antes de recarregar
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void Start()
    {
        Debug.Log("PauseManager ativo na cena!");
        if (pauseMenuUI == null) Debug.LogWarning("pauseMenuUI está vazio!");
        if (optionsMenuUI == null) Debug.LogWarning("optionsMenuUI está vazio!");
    }

}
