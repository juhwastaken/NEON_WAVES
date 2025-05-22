using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VictoryMusicPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource.clip == null)
        {
            Debug.LogWarning("VictoryMusicPlayer: Nenhum AudioClip atribuído!");
            return;
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.Play();
    }
}
