using UnityEngine;

public class subwooferjumper : MonoBehaviour
{
    private MovimentoJogador movimentoJogador;

    void Start()
    {
        movimentoJogador = GetComponent<MovimentoJogador>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("subwoofer"))
        {
            Debug.Log("Colidiu com: " + hit.gameObject.name);
            movimentoJogador.AplicarImpulsoVertical(10f); // Impulso vertical desejado
        }
    }
}
