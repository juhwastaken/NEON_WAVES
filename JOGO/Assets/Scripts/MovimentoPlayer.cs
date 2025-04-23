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

    private float velocidadeVertical;
    private float forcaPulo = 12f;     // Intensidade do pulo
    private float gravidade = -30f;    // Gravidade aplicada no Player

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

        controller.Move(movimento * Time.deltaTime * 7f);

        if (movimento != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movimento), Time.deltaTime * 10f);
        }

        animator.SetBool("Sprint", movimento != Vector3.zero);

        estaNoChao = Physics.CheckSphere(peDoPersonagem.position, 0.3f, colisaoLayer);

        if (estaNoChao && velocidadeVertical < 0)
        {
            velocidadeVertical = -2f; // Evita teleportar para o chão
            podePularNovamente = true;
        }

        animator.SetBool("EstaNoChao", estaNoChao);

        // Pulo simples e duplo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (estaNoChao)
            {
                velocidadeVertical = forcaPulo;
                animator.SetTrigger("Saltar");
            }
            else if (podePularNovamente)
            {
                velocidadeVertical = forcaPulo;
                podePularNovamente = false;
                animator.SetTrigger("Saltar");
            }
        }

        // Aplica gravidade
        velocidadeVertical += gravidade * Time.deltaTime;

        // Aplica movimento vertical
        controller.Move(new Vector3(0, velocidadeVertical, 0) * Time.deltaTime);
    }

    public void AplicarImpulsoVertical(float impulso)
    {
        velocidadeVertical = impulso;
        animator.SetTrigger("Saltar");
    }
}
