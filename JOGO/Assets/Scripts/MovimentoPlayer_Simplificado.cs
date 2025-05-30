using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Versão Simplificada: Foco em Idle, Corrida, Pulo/Pulo Duplo (mesma animação) e Morte.
public class MovimentoPlayer_Simplificado : MonoBehaviour
{
    private CharacterController controller;
    private Transform myCamera;
    private Animator animator;
    // Referência ao script de combate simplificado
    private PlayerCombat_Simplificado playerCombat;

    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidadeMovimento = 6f;
    // [SerializeField] private float velocidadeCorrida = 8f; // Removido - Usar apenas velocidadeMovimento por simplicidade
    [SerializeField] private float aceleracaoMovimento = 90.0f;
    [SerializeField] private float rotacaoSuave = 15f;
    private Vector3 velocidadeAtual;
    private float velocidadeAnimacao;

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

    // Constantes de Animação Essenciais
    private const string ANIM_MOVING = "Moving"; // Bool para indicar movimento
    private const string ANIM_VELOCITY_Z = "Velocity Z"; // Float para blend de Idle/Run
    private const string ANIM_JUMP_TRIGGER = "JumpTrigger"; // Trigger para Pulo e Pulo Duplo
    // Removido: Jumping State Int, Land Trigger, Die Trigger (Die é acionado pelo Combat)

    void Start()
    {
        controller = GetComponent<CharacterController>();
        myCamera = Camera.main.transform;
        animator = GetComponent<Animator>();
        // Pega a referência ao script de combate simplificado
        playerCombat = GetComponent<PlayerCombat_Simplificado>();

        if (controller == null) Debug.LogError("CharacterController não encontrado!");
        if (myCamera == null) Debug.LogError("Câmera principal não encontrada!");
        if (animator == null) Debug.LogError("Animator não encontrado!");
        if (playerCombat == null) Debug.LogWarning("PlayerCombat_Simplificado não encontrado! Funcionalidade de morte pode ser limitada.");
    }

    void Update()
    {
        // Atualiza estado de morte vindo do PlayerCombat
        jogadorMorto = (playerCombat != null) ? playerCombat.IsDead() : false;

        // Simplificado: só não pode mover/pular se estiver morto
        podeMover = !jogadorMorto;
        podePular = !jogadorMorto;

        if (jogadorMorto || controller == null || !controller.enabled) return;

        VerificarChao();
        ProcessarMovimentoHorizontal();
        ProcessarPuloEGravidade();
        AtualizarAnimator();

        // Removido: Verificações de morte por tecla/queda (Combat cuida disso)
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

        // Simplificado: Apenas reseta pulo duplo ao tocar o chão
        if (!estavaNoChao && estaNoChao) // Acabou de aterrissar
        {
            podePuloDuplo = false;
            // Removido: Chamada a Aterrissar() e playerCombat.OnLand()
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
            velocidadeAnimacao = 0f;
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

        // Calcula a velocidade local Z para o Animator
        velocidadeAnimacao = transform.InverseTransformDirection(velocidadeAtual).z;
    }

    private void ProcessarPuloEGravidade()
    {
        if (estaNoChao)
        {
            // Reseta pulo duplo e velocidade vertical ao tocar o chão
            podePuloDuplo = false;
            if (velocidadeVertical < 0.0f)
            {
                velocidadeVertical = -2f; // Pequena força para baixo para manter no chão
            }

            // Pulo Simples
            if (Input.GetButtonDown("Jump") && podePular)
            {
                Pular(forcaPulo);
                podePuloDuplo = true; // Permite o pulo duplo após sair do chão
            }
        }
        else // Está no ar
        {
            // Pulo Duplo
            if (Input.GetButtonDown("Jump") && podePular && podePuloDuplo)
            {
                Pular(forcaPuloDuplo);
                podePuloDuplo = false; // Só pode um pulo duplo
                // Usa o mesmo trigger de pulo para o pulo duplo
                animator?.SetTrigger(ANIM_JUMP_TRIGGER);
            }

            // Aplica gravidade (mais forte na queda)
            float multiplicadorGravidade = velocidadeVertical < 0 ? multiplicadorGravidadeQueda : 1f;
            velocidadeVertical += gravidade * multiplicadorGravidade * Time.deltaTime;
        }

        // Aplica movimento vertical (gravidade/pulo)
        if (controller.enabled)
        {
            controller.Move(new Vector3(0.0f, velocidadeVertical, 0.0f) * Time.deltaTime);
        }
    }

    private void Pular(float forca)
    {
        if (forca <= 0) return;
        // Calcula velocidade vertical necessária para atingir a altura (forca)
        velocidadeVertical = Mathf.Sqrt(forca * -2f * gravidade);
        estaNoChao = false;
        // Aciona a animação de pulo (serve para pulo simples e duplo)
        animator?.SetTrigger(ANIM_JUMP_TRIGGER);
    }

    // Removido: Aterrissar(), ResetJumpCooldown()

    private void AtualizarAnimator()
    {
        if (animator == null) return;
        // Atualiza parâmetros de movimento
        animator.SetBool(ANIM_MOVING, Mathf.Abs(velocidadeAnimacao) > 0.1f);
        animator.SetFloat(ANIM_VELOCITY_Z, velocidadeAnimacao);
        // Removido: Atualização do Jumping State Int
    }

    // Chamado pelo PlayerCombat quando o jogador morre
    public void PlayerDeathSequence()
    {
        if (jogadorMorto) return;
        jogadorMorto = true;
        podeMover = false;
        podePular = false;

        Debug.Log("Iniciando sequência de morte do jogador (Movimento)...");

        // Desabilita o controle físico
        if (controller != null)
            controller.enabled = false;

        // Lógica de UI e carregamento de cena (mantida)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Verifica se GameplayMusicPlayer existe antes de chamar
        GameplayMusicPlayer.Instance?.PararMusica();
        // Verifica se SceneManager existe antes de chamar
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

    // Função pública para o PlayerCombat saber se está no chão
    public bool IsGrounded() {
        return estaNoChao;
    }
}


