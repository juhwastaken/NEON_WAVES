using UnityEngine;

public class GameplayMusicPlayer : MonoBehaviour
{
    public static GameplayMusicPlayer Instance { get; private set; }

    [Header("Música de Gameplay")]
    public AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantém entre cenas, pode remover se não quiser isso
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("GameplayMusicPlayer: Nenhum AudioSource encontrado.");
            }
        }
    }

    void Start()
    {
        TocarMusica();
    }

    public void TocarMusica()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void PararMusica()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PausarMusica()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void ContinuarMusica()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }
}
