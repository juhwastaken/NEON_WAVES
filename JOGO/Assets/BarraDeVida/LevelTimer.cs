using UnityEngine;
using UnityEngine.Video;

public class LevelTimer : MonoBehaviour
{
    public float totalTime = 120f;
    private float currentTime;

    public VideoPlayer videoPlayer;

    private bool isPaused = false;

    void Start()
    {
        currentTime = totalTime;

        if (videoPlayer != null)
            videoPlayer.Play();
    }

    void Update()
    {
        if (isPaused)
            return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
        }
        else
        {
            currentTime = 0;
            if (videoPlayer != null && videoPlayer.isPlaying)
                videoPlayer.Stop();

            Debug.Log("Tempo esgotado!");
            // Aqui você pode adicionar lógica de fim de fase, como carregar uma nova cena
        }
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
