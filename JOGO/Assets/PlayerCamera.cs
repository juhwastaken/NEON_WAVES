ve se tem algo de errado nisso ai

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
public Transform player; // Referência ao Player
public float mouseSensitivity = 100f; // Sensibilidade do mouse
public Vector3 offset = new Vector3(0f, 2f, -4f); // Posição da câmera em relação ao Player

```
private float rotationX = 0f;

void Start()
{
    Cursor.lockState = CursorLockMode.Locked; // Trava o cursor no centro da tela
    Cursor.visible = false;
}

void Update()
{
    // Captura o movimento do mouse
    float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
    float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

    // Rotação vertical (limitada para evitar giros exagerados)
    rotationX -= mouseY;
    rotationX = Mathf.Clamp(rotationX, -80f, 80f);

    // Aplica rotação na câmera e no Player
    transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    player.Rotate(Vector3.up * mouseX);

    // Mantém a câmera atrás do Player com suavidade
    transform.position = Vector3.Lerp(transform.position, player.position + offset, Time.deltaTime * 5f);
    transform.LookAt(player.position + Vector3.up * 2f);
}
```

}
