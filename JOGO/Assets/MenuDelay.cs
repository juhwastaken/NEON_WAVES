using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuDelay : MonoBehaviour
{
    public GameObject menuCanvas; // Arraste seu Canvas aqui
    public float delay = 3f; // Tempo em segundos antes de mostrar o menu

    void Start()
    {
        menuCanvas.SetActive(false); // Esconde o menu no início
        Invoke("ShowMenu", delay);   // Chama a função após o tempo definido
    }

    void ShowMenu()
    {
        menuCanvas.SetActive(true);
    }
    void Update()
    {
        if (Input.anyKeyDown) // ou algum evento
        {
            menuCanvas.SetActive(true);
            this.enabled = false;
        }
    }
}
