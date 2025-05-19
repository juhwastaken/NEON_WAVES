using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private bool jogadorMorto = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        myCamera = Camera.main.transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Bloqueia o controle se jogador morreu ou controller não está disponível
        if (jogadorMorto || controller == null || !controller.enabled) return;

        // Impede movimentação se o tempo do jogo estiver pausado (ex: durante o tutorial)
        if (Time.timeScale == 0f) return;

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
            if (estaNoChao || (podePularNovamente && !estaNoChao))
            {
                velocidadeVertical = forcaPulo;
                podePularNovamente = estaNoChao ? true : false;
                animator.SetTrigger("Saltar");
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

        // Desativa o movimento
        if (controller != null)
            controller.enabled = false;

        // Exibe cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Para a música
        GameplayMusicPlayer.Instance?.PararMusica();

        // Salva a cena atual para poder dar Retry depois
        PlayerPrefs.SetString("LastGameplayScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();

        Debug.Log("Jogador morreu - carregando tela de Game Over");

        // Carrega a cena de Game Over
        SceneManager.LoadScene("GameOverScreen");
    }
}
