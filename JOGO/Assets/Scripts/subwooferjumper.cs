using UnityEngine;

public class subwooferjumper : MonoBehaviour
{
    private MovimentoJogador movimentoJogador;
    private AudioSource audioSource;
    public AudioClip somSubwoofer; // arraste esse no Inspector

    void Start()
    {
        movimentoJogador = GetComponent<MovimentoJogador>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("subwoofer"))
        {
            Debug.Log("Colidiu com: " + hit.gameObject.name);
            movimentoJogador.AplicarImpulsoVertical(20f);

            // Toca o som se estiver tudo certo
            if (somSubwoofer != null && audioSource != null)
            {
                audioSource.PlayOneShot(somSubwoofer);
            }
            else
            {
                Debug.LogWarning("Som ou AudioSource ausente!");
            }
        }
    }
}
