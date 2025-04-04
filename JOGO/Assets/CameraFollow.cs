using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Referência ao jogador
    public Vector3 offset = new Vector3(0f, 5f, -5f); // Ajuste a posição da câmera em relação ao player
    public float smoothSpeed = 5f; // Velocidade de suavização do movimento

    void LateUpdate()
    {
        if (player == null) return;

        // Posição desejada da câmera
        Vector3 desiredPosition = player.position + offset;
        // Suaviza a transição para evitar movimentação abrupta
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        // Mantém a câmera olhando para o player
        transform.LookAt(player);
    }
}
