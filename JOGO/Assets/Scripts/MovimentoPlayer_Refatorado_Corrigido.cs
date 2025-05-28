using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MovimentoPlayer_Refatorado_Corrigido : MonoBehaviour
{
    private CharacterController controller;
    private Transform myCamera;
    private Animator animator;
    private PlayerCombat_Refatorado_Corrigido playerCombat; // Referência ao script de combate

    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidadeMovimento = 6f;
    [SerializeField] private float velocidadeCorrida = 8f; // Adicionado para diferenciar corrida
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
    [SerializeField] private float multiplicadorGravidadeQueda = 2f; // Para cair mais rápido
    private float velocidadeVertical;
    private bool estaNoChao;
    private bool podePuloDuplo = false;
    private bool pulando = false;
    private bool caindo = false;

    [Header("Estados")]
    private bool jogadorMorto = false;
    private bool podeMover = true;
    private bool podePular = true;

    // Constantes de Animação (baseadas no Warrior)
    private const string ANIM_MOVING = "Moving";
    private const string ANIM_VELOCITY_Z = "Velocity Z";
    private const string ANIM_JUMPING_STATE = "Jumping"; // 0: Idle/Move, 1: Jump, 2: Fall, 3: DoubleJump, 4: Land (trigger?)
    private const string ANIM_JUMP_TRIGGER = "JumpTrigger";
    private const string ANIM_LAND_TRIGGER = "LandTrigger"; // Assumindo um trigger para aterrissagem
    private const string ANIM_DIE_TRIGGER = "Die"; // Trigger de morte (acionado pelo PlayerCombat)

    void Start()
    {
        controller = GetComponent<CharacterController>();
        myCamera = Camera.main.transform;
        animator = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat_Refatorado_Corrigido>(); // Pega a referência

        if (controller == null) Debug.LogError("CharacterController não encontrado!");
        if (myCamera == null) Debug.LogError("Câmera principal não encontrada!");
        if (animator == null) Debug.LogError("Animator não encontrado!");
        if (playerCombat == null) Debug.LogWarning("PlayerCombat não encontrado! Algumas funcionalidades podem ser limitadas.");
    }

    void Update()
    {
        // Atualiza estado de morte vindo do PlayerCombat
        // Verifica se playerCombat não é nulo antes de acessar IsDead e IsBlocking
        jogadorMorto = (playerCombat != null) ? playerCombat.IsDead() : false; // Assume não morto se não houver script de combate
        bool blocking = (playerCombat != null) ? playerCombat.IsBlocking() : false;

        podeMover = !jogadorMorto && !blocking;
        podePular = !jogadorMorto && !blocking;

        if (jogadorMorto || controller == null || !controller.enabled) return;

        VerificarChao();
        ProcessarMovimentoHorizontal();
        ProcessarPuloEGravidade();
        AtualizarAnimator();

        // Verificações de morte (mantidas por segurança, mas PlayerCombat deve cuidar da lógica de dano)
        VerificarMortePorTecla();
        VerificarMortePorQueda();
    }

    private void VerificarChao()
    {
        bool estavaNoChao = estaNoChao;
        // Garante que peDoPersonagem não é nulo
        if (peDoPersonagem == null) {
            Debug.LogError("Referência 'peDoPersonagem' não definida no Inspector!");
            estaNoChao = false; // Assume que não está no chão se não puder verificar
        } else {
            estaNoChao = Physics.CheckSphere(peDoPersonagem.position, 0.3f, colisaoLayer);
        }

        if (!estavaNoChao && estaNoChao) // Acabou de aterrissar
        {
            Aterrissar();
            // Chama OnLand no PlayerCombat se existir
            playerCombat?.OnLand();
        }
        else if (estavaNoChao && !estaNoChao) // Acabou de sair do chão
        {
            // Começou a cair ou pulou
            if (!pulando)
            {
                caindo = true;
                animator?.SetInteger(ANIM_JUMPING_STATE, 2); // Estado de Queda
                animator?.SetTrigger(ANIM_JUMP_TRIGGER); // Usar o mesmo trigger pode funcionar dependendo do Animator
            }
        }
    }

    private void ProcessarMovimentoHorizontal()
    {
        if (!podeMover) {
            // Garante que está desacelerando apenas se o controller estiver ativo
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

        velocidadeAnimacao = transform.InverseTransformDirection(velocidadeAtual).z;
    }

    private void ProcessarPuloEGravidade()
    {
        if (estaNoChao)
        {
            pulando = false;
            caindo = false;
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
                animator?.SetInteger(ANIM_JUMPING_STATE, 3);
                animator?.SetTrigger(ANIM_JUMP_TRIGGER);
            }

            float multiplicadorGravidade = velocidadeVertical < 0 ? multiplicadorGravidadeQueda : 1f;
            velocidadeVertical += gravidade * multiplicadorGravidade * Time.deltaTime;

            if (!pulando && velocidadeVertical < -0.1f && !caindo)
            {
                 caindo = true;
                 animator?.SetInteger(ANIM_JUMPING_STATE, 2);
                 animator?.SetTrigger(ANIM_JUMP_TRIGGER);
            }
        }

        if (controller.enabled)
        {
            controller.Move(new Vector3(0.0f, velocidadeVertical, 0.0f) * Time.deltaTime);
        }
    }

    private void Pular(float forca)
    {
        // Evita pulo se a força for zero ou negativa
        if (forca <= 0) return;
        // Cálculo da velocidade inicial para atingir a altura desejada (forca = altura do pulo)
        velocidadeVertical = Mathf.Sqrt(forca * -2f * gravidade);
        pulando = true;
        caindo = false;
        estaNoChao = false;
        animator?.SetInteger(ANIM_JUMPING_STATE, 1);
        animator?.SetTrigger(ANIM_JUMP_TRIGGER);
    }

    private void Aterrissar()
    {
        Debug.Log("Aterrissou");
        pulando = false;
        caindo = false;
        podePuloDuplo = false;
        animator?.SetInteger(ANIM_JUMPING_STATE, 0);
        // animator?.SetTrigger(ANIM_LAND_TRIGGER); // Acionar trigger de aterrissagem se houver
        StartCoroutine(ResetJumpCooldown());
    }

    private IEnumerator ResetJumpCooldown()
    {
        yield return new WaitForSeconds(0.1f);
    }

    private void AtualizarAnimator()
    {
        if (animator == null) return;
        animator.SetBool(ANIM_MOVING, Mathf.Abs(velocidadeAnimacao) > 0.1f);
        animator.SetFloat(ANIM_VELOCITY_Z, velocidadeAnimacao);
    }

    private void VerificarMortePorTecla()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Jogador morreu via tecla K (Debug)");
            if (playerCombat != null) playerCombat.TakeDamage(playerCombat.maxHealth + 1);
            else PlayerDeathSequence();
        }
    }

    private void VerificarMortePorQueda()
    {
        // Verifica se transform não é nulo
        if (transform == null) return;
        if (transform.position.y < -20f)
        {
            Debug.Log("Jogador caiu do mapa");
            if (playerCombat != null) playerCombat.TakeDamage(playerCombat.maxHealth + 1);
            else PlayerDeathSequence();
        }
    }

    public void PlayerDeathSequence()
    {
        if (jogadorMorto) return;
        jogadorMorto = true;

        Debug.Log("Iniciando sequência de morte do jogador...");

        if (controller != null)
            controller.enabled = false;

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
        // Verifica se a cena existe antes de carregar
        // SceneManager.LoadScene("GameOverScreen"); // Comentado para evitar erro se a cena não existir
        Debug.LogWarning("Carregamento da cena 'GameOverScreen' desativado temporariamente. Verifique se a cena existe e descomente a linha acima em LoadGameOverScreen.");
    }

    public bool IsGrounded() {
        return estaNoChao;
    }
}

