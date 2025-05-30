using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Versão Adaptada: Usa os parâmetros do Animator fornecido pelo usuário.
public class MovimentoPlayer_Adaptado : MonoBehaviour
{
    private CharacterController controller;
    private Transform myCamera;
    private Animator animator;
    // Referência ao script de combate adaptado
    private PlayerCombat_Adaptado playerCombat;

    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidadeMovimento = 6f;
    [SerializeField] private float aceleracaoMovimento = 90.0f;
    [SerializeField] private float rotacaoSuave = 15f;
    private Vector3 velocidadeAtual;

    [Header("Configurações de Pulo e Gravidade")]
    [SerializeField] private Transform peDoPersonagem;
    [SerializeField] private LayerMask colisaoLayer;
    [SerializeField] private float forcaPulo = 8f;
    [SerializeField] private float forcaPuloDuplo = 8f;
    [SerializeField] private float gravidade = -25f;
    [SerializeField] private float multiplicadorGravidadeQueda = 2f;
    private float velocidadeVertical;
    private bool estaNoChao;
    private bool podePuloDuplo = false;

    [Header("Estados")]
    private bool jogadorMorto = false;
    private bool podeMover = true;
    private bool podePular = true;

    // Constantes de Animação - Adaptadas ao Animator do usuário
    private const string ANIM_SPRINT = "Sprint"; // Bool para indicar corrida
    private const string ANIM_SALTAR = "Saltar"; // Trigger para Pulo e Pulo Duplo
    private const string ANIM_ESTA_NO_CHAO = "EstaNoChao"; // Bool para indicar se está no chão

    void Start()
    {
        controller = GetComponent<CharacterController>();
        myCamera = Camera.main.transform;
        animator = GetComponent<Animator>();
        // Pega a referência ao script de combate adaptado
        playerCombat = GetComponent<PlayerCombat_Adaptado>();

        if (controller == null) Debug.LogError("CharacterController não encontrado!");
        if (myCamera == null) Debug.LogError("Câmera principal não encontrada!");
        if (animator == null) Debug.LogError("Animator não encontrado!");
        if (playerCombat == null) Debug.LogWarning("PlayerCombat_Simplificado não encontrado! Funcionalidade de morte pode ser limitada.");
    }

    void Update()
    {
        jogadorMorto = (playerCombat != null) ? playerCombat.IsDead() : false;
        podeMover = !jogadorMorto;
        podePular = !jogadorMorto;

        if (jogadorMorto || controller == null || !controller.enabled) return;

        VerificarChao();
        ProcessarMovimentoHorizontal();
        ProcessarPuloEGravidade();
        AtualizarAnimator();
    }

    private void VerificarChao()
    {
        bool estavaNoChao = estaNoChao;
        if (peDoPersonagem == null) {
            Debug.LogError("Referência 'peDoPersonagem' não definida!");
            estaNoChao = false;
        } else {
            estaNoChao = Physics.CheckSphere(peDoPersonagem.position, 0.3f, colisaoLayer);
        }

        if (!estavaNoChao && estaNoChao) // Acabou de aterrissar
        {
            podePuloDuplo = false;
        }
    }

    private void ProcessarMovimentoHorizontal()
    {
        if (!podeMover) {
            if (controller.enabled)
            {
                velocidadeAtual = Vector3.MoveTowards(velocidadeAtual, Vector3.zero, aceleracaoMovimento * Time.deltaTime);
                controller.Move(velocidadeAtual * Time.deltaTime);
            }
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirecao = new Vector3(horizontal, 0, vertical).normalized;

        float velocidadeAlvo = velocidadeMovimento;
        Vector3 direcaoMovimento = Vector3.zero;

        if (inputDirecao.magnitude >= 0.1f && myCamera != null)
        {
            float anguloAlvo = Mathf.Atan2(inputDirecao.x, inputDirecao.z) * Mathf.Rad2Deg + myCamera.eulerAngles.y;
            direcaoMovimento = Quaternion.Euler(0f, anguloAlvo, 0f) * Vector3.forward;
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcaoMovimento.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * rotacaoSuave);
        }

        Vector3 velocidadeDesejada = direcaoMovimento.normalized * velocidadeAlvo * inputDirecao.magnitude;
        velocidadeAtual = Vector3.MoveTowards(velocidadeAtual, velocidadeDesejada, aceleracaoMovimento * Time.deltaTime);

        if (controller.enabled)
        {
            controller.Move(velocidadeAtual * Time.deltaTime);
        }
    }

    private void ProcessarPuloEGravidade()
    {
        if (estaNoChao)
        {
            podePuloDuplo = false;
            if (velocidadeVertical < 0.0f)
            {
                velocidadeVertical = -2f;
            }

            if (Input.GetButtonDown("Jump") && podePular)
            {
                Pular(forcaPulo);
                podePuloDuplo = true;
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && podePular && podePuloDuplo)
            {
                Pular(forcaPuloDuplo);
                podePuloDuplo = false;
                // Usa o trigger 'Saltar' para o pulo duplo também
                animator?.SetTrigger(ANIM_SALTAR);
            }

            float multiplicadorGravidade = velocidadeVertical < 0 ? multiplicadorGravidadeQueda : 1f;
            velocidadeVertical += gravidade * multiplicadorGravidade * Time.deltaTime;
        }

        if (controller.enabled)
        {
            controller.Move(new Vector3(0.0f, velocidadeVertical, 0.0f) * Time.deltaTime);
        }
    }

    private void Pular(float forca)
    {
        if (forca <= 0) return;
        velocidadeVertical = Mathf.Sqrt(forca * -2f * gravidade);
        estaNoChao = false;
        // Aciona a animação de pulo usando o trigger 'Saltar'
        animator?.SetTrigger(ANIM_SALTAR);
    }

    private void AtualizarAnimator()
    {
        if (animator == null) return;

        // Verifica se está se movendo (baseado no input ou velocidade)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = new Vector3(horizontal, 0, vertical).magnitude >= 0.1f;

        // Atualiza parâmetros de movimento usando os nomes do Animator do usuário
        animator.SetBool(ANIM_SPRINT, isMoving);
        animator.SetBool(ANIM_ESTA_NO_CHAO, estaNoChao);
    }

    public void PlayerDeathSequence()
    {
        if (jogadorMorto) return;
        jogadorMorto = true;
        podeMover = false;
        podePular = false;
        Debug.Log("Iniciando sequência de morte do jogador (Movimento)...");
        if (controller != null)
            controller.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameplayMusicPlayer.Instance?.PararMusica();
        if (SceneManager.GetActiveScene() != null)
        {
             PlayerPrefs.SetString("LastGameplayScene", SceneManager.GetActiveScene().name);
             PlayerPrefs.Save();
             StartCoroutine(LoadGameOverScreen(1.5f));
        }
        else
        {
            Debug.LogError("Não foi possível obter a cena ativa para salvar ou carregar Game Over.");
        }
    }

    private IEnumerator LoadGameOverScreen(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Carregando tela de Game Over...");
        // SceneManager.LoadScene("GameOverScreen"); // Verifique se a cena existe
        Debug.LogWarning("Carregamento da cena 'GameOverScreen' desativado. Verifique se a cena existe.");
    }

    public bool IsGrounded() {
        return estaNoChao;
    }
}

