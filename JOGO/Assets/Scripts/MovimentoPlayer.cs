using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimentoJogador : MonoBehaviour
{
    private CharacterController controller;
    private Transform myCamera;
    private Animator animator;

    private bool estaNoChao;
    private bool podePularNovamente;
    [SerializeField] private Transform peDoPersonagem;
    [SerializeField] private LayerMask colisaoLayer;

    private float velocidadeVertical;
    [SerializeField] private float forcaPulo = 12f;
    [SerializeField] private float gravidade = -30f;
    [SerializeField] private float velocidadeMovimento = 7f;

    [Header("Referência à tela de Game Over")]
    public GameOverScreen gameOverScreen;

    private bool jogadorMorto = false;

    // 🎵 SFX de pulo
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxPulo;
    [SerializeField] private AudioClip sfxPuloDuplo;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        myCamera = Camera.main.transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (jogadorMorto || controller == null || !controller.enabled) return;

        ProcessarMovimentoHorizontal();
        ProcessarPuloEGravidade();
        VerificarMortePorTecla();
        VerificarMortePorQueda();
    }

    private void ProcessarMovimentoHorizontal()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movimento = new Vector3(horizontal, 0, vertical);
        movimento = myCamera.TransformDirection(movimento);
        movimento.y = 0;

        if (movimento.magnitude > 0.1f)
        {
            controller.Move(movimento.normalized * velocidadeMovimento * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movimento), Time.deltaTime * 10f);
        }

        animator.SetBool("Sprint", movimento.magnitude > 0.1f);
    }

    private void ProcessarPuloEGravidade()
    {
        estaNoChao = Physics.CheckSphere(peDoPersonagem.position, 0.3f, colisaoLayer);

        if (estaNoChao && velocidadeVertical < 0)
        {
            velocidadeVertical = -2f;
            podePularNovamente = true;
        }

        animator.SetBool("EstaNoChao", estaNoChao);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (estaNoChao)
            {
                velocidadeVertical = forcaPulo;
                animator.SetTrigger("Saltar");

                // 🔊 Toca som de pulo
                if (sfxPulo != null && audioSource != null)
                    audioSource.PlayOneShot(sfxPulo);
            }
            else if (podePularNovamente)
            {
                velocidadeVertical = forcaPulo;
                podePularNovamente = false;
                animator.SetTrigger("Saltar");

                // 🔊 Toca som de pulo duplo
                if (sfxPuloDuplo != null && audioSource != null)
                    audioSource.PlayOneShot(sfxPuloDuplo);
            }
        }

        velocidadeVertical += gravidade * Time.deltaTime;
        controller.Move(new Vector3(0, velocidadeVertical, 0) * Time.deltaTime);
    }

    private void VerificarMortePorTecla()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Jogador morreu via tecla K");
            PlayerDeath();
        }
    }

    private void VerificarMortePorQueda()
    {
        if (transform.position.y < -10f)
        {
            Debug.Log("Jogador caiu do mapa");
            PlayerDeath();
        }
    }

    public void AplicarImpulsoVertical(float impulso)
    {
        if (jogadorMorto || controller == null || !controller.enabled) return;

        velocidadeVertical = impulso;
        animator.SetTrigger("Saltar");
    }

    public void PlayerDeath()
    {
        if (jogadorMorto) return;

        jogadorMorto = true;

        if (controller != null)
            controller.enabled = false;

        if (gameOverScreen != null)
        {
            gameOverScreen.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverScreen não foi atribuído no MovimentoJogador!");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
