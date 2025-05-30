using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class LevelTimer : MonoBehaviour
{
    [Header("Tempo Total (segundos)")]
    public float totalTime = 120f;
    private float currentTime;

    [Header("Vídeo opcional (ex: fundo ou música)")]
    public VideoPlayer videoPlayer;

    private bool isPaused = false;
    private bool gameEnded = false;

    void Start()
    {
        currentTime = totalTime;

        if (videoPlayer != null)
            videoPlayer.Play();
    }

    void Update()
    {
        if (isPaused || gameEnded)
            return;

        if (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;
        }
        else
        {
            currentTime = 0f;
            EndLevel();
        }
    }

    private void EndLevel()
    {
        gameEnded = true;

        // Parar vídeo se estiver tocando
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        // Destruir todos os inimigos com a tag "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        Debug.Log("Tempo esgotado - Fim da fase!");

        // Esperar e carregar a cena de vitória
        StartCoroutine(LoadVictorySceneAfterDelay(1.5f)); // 1.5 segundos
    }

    private IEnumerator LoadVictorySceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("WinScreen"); // <<-- Altere se sua cena tiver outro nome
    }

    public void PauseTimer()
    {
        isPaused = true;

        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Pause();
    }

    public void ResumeTimer()
    {
        isPaused = false;

        if (videoPlayer != null && !videoPlayer.isPlaying)
            videoPlayer.Play();
    }
}
