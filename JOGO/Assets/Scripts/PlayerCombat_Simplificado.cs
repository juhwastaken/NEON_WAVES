using UnityEngine;
using System.Collections;

// Versão Simplificada: Foco em Idle, Corrida, Pulo, Ataque Básico (Combo), Dash Attack (MoveAttack1) e Morte.
public class PlayerCombat_Simplificado : MonoBehaviour
{
    [Header("Stats")]
    public int playerHealth = 100;
    public int maxHealth = 100;
    public int baseDamage = 20;

    [Header("Attack Settings")]
    public LayerMask enemyLayer;
    public Transform attackPoint;
    public float attackRadius = 1.5f;
    public float attackCooldown = 0.5f;
    private float lastAttackTime = -Mathf.Infinity;
    private int attackChainCount = 0;
    private Coroutine attackResetCoroutine;

    [Header("Dash Attack (MoveAttack1) Settings")]
    public float dashRange = 10f;
    public float dashSpeed = 20f;
    public float dashCooldown = 3f;
    public float dashAttackDamageMultiplier = 1.5f;
    private float lastDashTime = -Mathf.Infinity;
    private bool isDashing = false;
    private Coroutine dashCoroutine;

    // Removido: Jump Attack, Blocking, Rhythm Mechanics

    [Header("Components")]
    private Animator animator;
    private CharacterController cc;
    // Mantém referência ao MovimentoPlayer para saber se está no chão e para chamar morte
    private MovimentoPlayer_Refatorado_Corrigido movimentoPlayer;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxAtaque1;
    [SerializeField] private AudioClip sfxAtaque2;
    [SerializeField] private AudioClip sfxAtaque3;
    [SerializeField] private AudioClip sfxDash;
    [SerializeField] private AudioClip sfxMorte;
    // Removido: sfxRecebeDano, sfxBloqueio, sfxQuebraBloqueio

    // State
    private bool isDead = false;
    private bool canAttack = true;
    private bool canDash = true;

    // Constantes de Animação Essenciais
    private const string ANIM_ATTACK_TRIGGER = "AttackTrigger"; // Para ataques básicos
    private const string ANIM_ACTION_INT = "Action"; // Para diferenciar ataques do combo (1, 2, 3)
    private const string ANIM_MOVE_ATTACK_TRIGGER = "AttackMoveTrigger"; // Para o Dash Attack (MoveAttack1)
    private const string ANIM_DIE_TRIGGER = "Die"; // Para a morte
    // Removido: Blocking, GetHit, BlockImpact, JumpAttack triggers/bools

    void Start()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        // Garante que pega o nome correto do script de movimento
        movimentoPlayer = GetComponent<MovimentoPlayer_Refatorado_Corrigido>();

        playerHealth = maxHealth;

        if (animator == null) Debug.LogError("Animator não encontrado!");
        if (cc == null) Debug.LogWarning("CharacterController não encontrado!");
        if (movimentoPlayer == null) Debug.LogError("MovimentoPlayer_Refatorado_Corrigido não encontrado! Algumas funcionalidades podem ser limitadas.");
    }

    void Update()
    {
        if (isDead) return;

        // Verifica se movimentoPlayer existe antes de chamar IsGrounded
        bool grounded = (movimentoPlayer != null) ? movimentoPlayer.IsGrounded() : true;

        HandleInput(grounded);
    }

    private void HandleInput(bool isGrounded)
    {
        // Ataque Básico (Mouse 0) - Apenas no chão
        if (Input.GetMouseButtonDown(0) && canAttack && !isDashing && isGrounded)
        {
            AttemptAttack();
        }

        // Dash Attack (Mouse 1) - Pode ser no chão ou ar?
        // Vamos manter como estava: pode dar dash a qualquer momento se não estiver em cooldown ou atacando
        if (Input.GetMouseButtonDown(1) && canDash && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            AttemptDashAttack();
        }

        // Teste de Morte (Exemplo)
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(maxHealth + 1); // Causa morte instantânea para teste
        }
    }

    void AttemptAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        attackChainCount++;

        // Limita o combo a 3 ataques
        if (attackChainCount > 3) attackChainCount = 1;

        // Reseta o combo se o jogador demorar para clicar novamente
        if (attackResetCoroutine != null) StopCoroutine(attackResetCoroutine);
        attackResetCoroutine = StartCoroutine(ResetAttackChain(0.8f)); // Tempo para resetar o combo

        Attack(attackChainCount);
    }

    void Attack(int chainStep)
    {
        Debug.Log($"[DEBUG] Iniciando Ataque {chainStep}.");
        // Define qual ataque do combo usar
        animator?.SetInteger(ANIM_ACTION_INT, chainStep);
        // Aciona a animação de ataque
        animator?.SetTrigger(ANIM_ATTACK_TRIGGER);

        // Toca SFX correspondente
        AudioClip sfx = null;
        switch (chainStep)
        {
            case 1: sfx = sfxAtaque1; break;
            case 2: sfx = sfxAtaque2; break;
            case 3: sfx = sfxAtaque3; break;
        }
        if (sfx != null && audioSource != null) audioSource.PlayOneShot(sfx);

        // Aplica o dano após um pequeno delay (simula o tempo da animação)
        StartCoroutine(ApplyAttackDamage(0.2f, baseDamage));
    }

    void AttemptDashAttack()
    {
        // Tenta encontrar um inimigo próximo para mirar o dash
        GameObject nearestEnemy = FindNearestEnemyInRange(dashRange);
        if (nearestEnemy != null)
        {
            Debug.Log($"[DEBUG] Iniciando Dash Attack em direção a {nearestEnemy.name}.");
            if (dashCoroutine != null) StopCoroutine(dashCoroutine);
            dashCoroutine = StartCoroutine(ExecuteDashAttack(nearestEnemy.transform));
        }
        else
        {
            // Se não houver inimigo, faz um dash para frente
            Debug.Log("[DEBUG] Nenhum inimigo próximo. Dash para frente.");
            if (dashCoroutine != null) StopCoroutine(dashCoroutine);
            // Usa a direção atual do personagem como alvo
            Vector3 forwardTargetPos = transform.position + transform.forward * dashRange;
            Transform forwardTarget = new GameObject("DashTargetForward").transform;
            forwardTarget.position = forwardTargetPos;
            dashCoroutine = StartCoroutine(ExecuteDashAttack(forwardTarget, true)); // true para destruir o alvo temporário
        }
    }

    IEnumerator ExecuteDashAttack(Transform target, bool destroyTarget = false)
    {
        isDashing = true;
        canAttack = false;
        canDash = false;
        lastDashTime = Time.time;

        if (sfxDash != null && audioSource != null) audioSource.PlayOneShot(sfxDash);

        // Aciona a animação de Dash Attack (MoveAttack1)
        // Não precisa mais do Action Int aqui, apenas o trigger específico
        animator?.SetTrigger(ANIM_MOVE_ATTACK_TRIGGER);

        Vector3 start = transform.position;
        Vector3 directionToTarget = (target.position - start).normalized;
        if (directionToTarget == Vector3.zero) directionToTarget = transform.forward;

        // Calcula o ponto final um pouco antes do alvo para não entrar nele
        Vector3 end = target.position - directionToTarget * 1.0f;
        // Se for dash pra frente sem alvo, vai até o ponto calculado
        if (destroyTarget) end = target.position;

        float distance = Vector3.Distance(start, end);
        float dashDuration = (dashSpeed > 0 && distance > 0.1f) ? distance / dashSpeed : 0.1f;
        float elapsed = 0f;

        // Desabilita o CharacterController durante o dash para evitar conflitos
        if (cc != null) cc.enabled = false;

        // Move o personagem
        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / dashDuration);
            // Mantém o personagem virado para a direção do dash
            transform.rotation = Quaternion.LookRotation(directionToTarget);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        if (cc != null) cc.enabled = true; // Reabilita o CharacterController

        // Destroi o alvo temporário se foi criado
        if (destroyTarget) Destroy(target.gameObject);

        Debug.Log("[DEBUG] Dash finalizado. Aplicando dano...");
        // Aplica dano em área no final do dash
        ApplyAreaDamage(transform.position, attackRadius, baseDamage * dashAttackDamageMultiplier);

        // Pequeno cooldown após o dash antes de poder agir novamente
        yield return new WaitForSeconds(0.2f);
        isDashing = false;
        canAttack = true;
        canDash = true;

        Debug.Log("[DEBUG] Jogador não está mais dashing.");
    }

    IEnumerator ApplyAttackDamage(float delay, float damageAmount)
    {
        yield return new WaitForSeconds(delay);

        if (attackPoint == null) {
            Debug.LogError("[DEBUG] AttackPoint não configurado!");
            yield break;
        }
        ApplyAreaDamage(attackPoint.position, attackRadius, damageAmount);
    }

    void ApplyAreaDamage(Vector3 center, float radius, float damageAmount)
    {
        Collider[] hitEnemies = Physics.OverlapSphere(center, radius, enemyLayer);
        Debug.Log($"[DEBUG] Verificando inimigos no raio {radius} a partir de {center}. Encontrados: {hitEnemies.Length}");

        foreach (Collider enemyCollider in hitEnemies)
        {
            // Simplificado: Apenas aplica dano se encontrar um componente que possa receber
            // Idealmente, o inimigo teria um script com um método como "TakeDamage"
            var damageable = enemyCollider.GetComponent<IDamageable>(); // Exemplo de interface
            if (damageable != null)
            {
                damageable.TakeDamage((int)damageAmount);
                Debug.Log($"[DEBUG] Dano aplicado a {enemyCollider.gameObject.name}: {damageAmount}");
            }
            // Removido: Knockback e referência a EnemyAI específico
        }
    }

    IEnumerator ResetAttackChain(float delay)
    {
        yield return new WaitForSeconds(delay);
        attackChainCount = 0;
    }

    // Função pública para receber dano (pode ser chamada por inimigos, armadilhas, etc.)
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // Removido: Lógica de bloqueio

        playerHealth -= damage;
        Debug.Log($"[DEBUG] Jogador tomou {damage} de dano! Vida: {playerHealth}/{maxHealth}");

        // Removido: Animação de GetHit e SFX de dano

        if (playerHealth <= 0)
        {
            playerHealth = 0;
            Die();
        }
        // Removido: Cooldown após tomar dano
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("[DEBUG] Player morreu!");

        // Aciona a animação de morte
        animator?.SetTrigger(ANIM_DIE_TRIGGER);
        if (sfxMorte != null && audioSource != null) audioSource.PlayOneShot(sfxMorte);

        // Desabilita controle de movimento
        if (cc != null) cc.enabled = false;
        // Chama a sequência de morte no MovimentoPlayer (se existir)
        movimentoPlayer?.PlayerDeathSequence();

        // Desabilita este script para impedir mais ações
        // Pode ser reativado externamente por um GameManager ao reiniciar
        this.enabled = false;
    }

    GameObject FindNearestEnemyInRange(float range)
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, range, enemyLayer);
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestEnemy = enemy.gameObject;
            }
        }
        return closestEnemy;
    }

    // Removido: ActionCooldown

    public bool IsDead()
    {
        return isDead;
    }

    // ResetState simplificado para reativar o script ao reiniciar o jogo/nível
    public void ResetState()
    {
        playerHealth = maxHealth;
        isDead = false;
        isDashing = false;
        lastDashTime = -Mathf.Infinity;
        lastAttackTime = -Mathf.Infinity;
        attackChainCount = 0;

        canAttack = true;
        canDash = true;

        if (cc != null) cc.enabled = true;
        if (animator) {
             // Reseta os triggers que podem ter ficado ativos
             animator.ResetTrigger(ANIM_DIE_TRIGGER);
             animator.ResetTrigger(ANIM_ATTACK_TRIGGER);
             animator.ResetTrigger(ANIM_MOVE_ATTACK_TRIGGER);
             animator.SetInteger(ANIM_ACTION_INT, 0);
        }
        this.enabled = true; // Garante que o script está ativo
        Debug.Log("[DEBUG] Estado do PlayerCombat_Simplificado resetado.");
    }
}

// Exemplo de interface para dano (coloque em um arquivo separado: IDamageable.cs)
public interface IDamageable
{
    void TakeDamage(int amount);
}

