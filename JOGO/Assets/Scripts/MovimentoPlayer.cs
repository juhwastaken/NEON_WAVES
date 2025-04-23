using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimentoJogador : MonoBehaviour
{
    private CharacterController controller;
    private Transform myCamera;
    private Animator animator;

    private bool estaNoChao;
    private bool podePularNovamente; // Para controlar o pulo duplo
    [SerializeField] private Transform peDoPersonagem;
    [SerializeField] private LayerMask colisaoLayer;

    private float forcaY;
    private float forcaPulo = 6f; // Intensidade do pulo
    private float gravidade = -15f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        myCamera = Camera.main.transform;
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movimento = new Vector3(horizontal, 0, vertical);
        movimento = myCamera.TransformDirection(movimento);
        movimento.y = 0;

        controller.Move(movimento * Time.deltaTime * 5);

        if (movimento != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movimento), Time.deltaTime * 10);
        }

        animator.SetBool("Sprint", movimento != Vector3.zero);

        estaNoChao = Physics.CheckSphere(peDoPersonagem.position, 0.3f, colisaoLayer);

        if (estaNoChao)
        {
            podePularNovamente = true; // Reseta a capacidade de pular novamente
        }

        animator.SetBool("EstaNoChao", estaNoChao);

        // L�gica do Pulo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (estaNoChao)
            {
                forcaY = forcaPulo;
                animator.SetTrigger("Saltar");
            }
            else if (podePularNovamente) // Pulo duplo
            {
                forcaY = forcaPulo;
                podePularNovamente = false; // Impede outro pulo duplo at� tocar o ch�o
                animator.SetTrigger("Saltar");
            }
        }

        // Aplica��o da gravidade
        forcaY += gravidade * Time.deltaTime;

        controller.Move(new Vector3(0, forcaY, 0) * Time.deltaTime);
    }

    public void AplicarImpulsoVertical(float impulso)
    {
        forcaY = impulso;
        animator.SetTrigger("Saltar");
    }



}
