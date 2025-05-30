using UnityEngine;

public class PosicionadorInicial : MonoBehaviour
{
    // Defina a posição inicial desejada aqui no Inspector ou diretamente no código
    public Vector3 posicaoInicial = new Vector3(22.8f, 53.51f, -15.685f);

    void Start()
    {
        // Define a posição do objeto assim que ele é inicializado na cena
        transform.position = posicaoInicial;
        Debug.Log("Posição inicial definida por script para: " + transform.position);
    }
}

