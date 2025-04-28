using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LevelTimer : MonoBehaviour
{
    public float totalTime = 120f;
    private float currentTime;
    public Text timerText;
    public VideoPlayer videoPlayer;

    private bool isPaused = false; // NOVO: controle de pausa interno

    void Start()
    {
        currentTime = totalTime;
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }

    void Update()
    {
        if (isPaused)
            return; // Se pausado, não atualiza timer

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            currentTime = 0;
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            Debug.Log("Tempo esgotado!");
            // Aqui você pode fazer algo, tipo carregar outra cena
        }
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
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
