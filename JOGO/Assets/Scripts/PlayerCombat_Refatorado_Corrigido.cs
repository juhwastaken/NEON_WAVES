using UnityEngine;
using System.Collections;

public class PlayerCombat_Refatorado_Corrigido : MonoBehaviour
{
    [Header("Stats")]
    public int playerHealth = 100;
    public int maxHealth = 100;
    public int baseDamage = 20;

    [Header("Attack Settings")]
    public float attackRange = 5f; // Usado para encontrar inimigo para dash?
    public LayerMask enemyLayer;
    public Transform attackPoint;
    public float attackRadius = 1.5f;
    public float attackCooldown = 0.5f; // Cooldown básico entre ataques
    private float lastAttackTime = -Mathf.Infinity;
    private int attackChainCount = 0;
    private Coroutine attackResetCoroutine;

    [Header("Dash Attack (MoveAttack1) Settings")]
    public float dashRange = 10f;
    public float dashSpeed = 20f;
    public float dashCooldown = 3f; // Cooldown específico do dash
    public float dashAttackDamageMultiplier = 1.5f;
    private float lastDashTime = -Mathf.Infinity;
    private bool isDashing = false;
    private Coroutine dashCoroutine;

    [Header("Jump Attack Settings")]
    public float jumpAttackDamageMultiplier = 1.2f;
    public float jumpAttackLandingLag = 0.5f; // Tempo de "lock" após jump attack
    private bool isJumpAttacking = false;

    [Header("Blocking Settings")]
    private bool isBlocking = false;

    [Header("Rhythm Mechanics")]
    public AudioSource musicSource;
    public float perfectAttackTime = 1.0833333334f; // Tempo da batida
    public float timeMargin = 0.1f; // Margem para acerto perfeito
    private int currentDamageMultiplier = 1;
    private int maxMultiplier = 32;
    private bool musicStarted = false;
    private float musicTime;

    [Header("Components")]
    private Animator animator;
    private CharacterController cc;
    private MovimentoPlayer_Refatorado_Corrigido movimentoPlayer; // Referência ao script de movimento

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxAtaque1;
    [SerializeField] private AudioClip sfxAtaque2;
    [SerializeField] private AudioClip sfxAtaque3;
    [SerializeField] private AudioClip sfxRecebeDano;
    [SerializeField] private AudioClip sfxDash;
    [SerializeField] private AudioClip sfxMorte;
    [SerializeField] private AudioClip sfxBloqueio;
    [SerializeField] private AudioClip sfxQuebraBloqueio; // Renomeado para clareza

    // State
    private bool isDead = false;
    private bool canAttack = true;
    private bool canDash = true;
    private bool canBlock = true;

    // Constantes de Animação
    private const string ANIM_ATTACK_TRIGGER = "AttackTrigger";
    private const string ANIM_ACTION_INT = "Action";
    private const string ANIM_MOVE_ATTACK_TRIGGER = "AttackMoveTrigger";
    private const string ANIM_JUMP_ATTACK_TRIGGER = "JumpAttackTrigger"; // Considerar se realmente existe/é necessário
    private const string ANIM_BLOCKING_BOOL = "Blocking";
    private const string ANIM_GET_HIT_TRIGGER = "GetHitTrigger";
    private const string ANIM_DIE_TRIGGER = "Die";
    private const string ANIM_BLOCK_IMPACT_TRIGGER = "BlockImpactTrigger"; // Adicionado para feedback de bloqueio

    void Start()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        movimentoPlayer = GetComponent<MovimentoPlayer_Refatorado_Corrigido>();

        playerHealth = maxHealth;

        if (musicSource != null && musicSource.isPlaying) musicStarted = true;
        if (animator == null) Debug.LogError("Animator não encontrado!");
        if (cc == null) Debug.LogWarning("CharacterController não encontrado!");
        if (movimentoPlayer == null) Debug.LogError("MovimentoPlayer não encontrado! Combate não funcionará corretamente.");
    }

    void Update()
    {
        if (isDead) return;

        if (!musicStarted && musicSource != null && musicSource.isPlaying) musicStarted = true;

        // Verifica se movimentoPlayer existe antes de chamar IsGrounded
        bool grounded = (movimentoPlayer != null) ? movimentoPlayer.IsGrounded() : true; // Assume grounded se não houver script de movimento

        HandleInput(grounded);
        UpdateBlockingState(grounded);
    }

    private void HandleInput(bool isGrounded)
    {
        // Ataque Básico (Mouse 0)
        if (Input.GetMouseButtonDown(0) && canAttack && !isDashing && !isBlocking && isGrounded)
        {
            AttemptAttack();
        }

        // Ataque Aéreo (Mouse 0 no Ar)
        if (Input.GetMouseButtonDown(0) && canAttack && !isDashing && !isBlocking && !isGrounded && !isJumpAttacking)
        {
            AttemptJumpAttack();
        }

        // Dash Attack (Mouse 1)
        if (Input.GetMouseButtonDown(1) && canDash && !isDashing && !isBlocking && Time.time >= lastDashTime + dashCooldown)
        {
            AttemptDashAttack();
        }

        // Bloqueio (Manter pressionado - Ex: Left Shift)
        // Só pode bloquear no chão
        isBlocking = Input.GetKey(KeyCode.LeftShift) && canBlock && !isDashing && isGrounded;

        // Teste de Dano
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(10);
        }
    }

    private void UpdateBlockingState(bool isGrounded)
    {
        // Garante que só bloqueia no chão
        if (!isGrounded) isBlocking = false;

        // Atualiza Animator apenas se o estado mudou
        if (animator != null && animator.GetBool(ANIM_BLOCKING_BOOL) != isBlocking)
        {
            animator.SetBool(ANIM_BLOCKING_BOOL, isBlocking);
            // Tocar som de início/fim de bloqueio?
        }

        // Bloqueio impede ataque e dash
        canAttack = !isBlocking && !isDashing; // Não pode atacar bloqueando ou durante dash
        canDash = !isBlocking && !isDashing; // Não pode dar dash bloqueando ou durante outro dash
    }

    void AttemptAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        attackChainCount++;

        if (attackChainCount > 3) attackChainCount = 1;

        if (attackResetCoroutine != null) StopCoroutine(attackResetCoroutine);
        attackResetCoroutine = StartCoroutine(ResetAttackChain(0.8f));

        Attack(attackChainCount);
    }

    void Attack(int chainStep)
    {
        Debug.Log($"[DEBUG] Iniciando Ataque {chainStep}.");
        animator?.SetInteger(ANIM_ACTION_INT, chainStep);
        animator?.SetTrigger(ANIM_ATTACK_TRIGGER);

        AudioClip sfx = null;
        switch (chainStep)
        {
            case 1: sfx = sfxAtaque1; break;
            case 2: sfx = sfxAtaque2; break;
            case 3: sfx = sfxAtaque3; break;
        }
        if (sfx != null && audioSource != null) audioSource.PlayOneShot(sfx);

        StartCoroutine(ApplyAttackDamage(0.2f, baseDamage));
        CheckRhythm();
    }

    void AttemptJumpAttack()
    {
        Debug.Log("Attempt Jump Attack");
        isJumpAttacking = true;
        animator?.SetInteger(ANIM_ACTION_INT, 5); // Usar um número de ação diferente para jump attack?
        animator?.SetTrigger(ANIM_ATTACK_TRIGGER); // Ou usar o trigger geral?
        // A chamada OnLand() no MovimentoPlayer cuidará do dano ao aterrissar.
    }

    // Chamado por MovimentoPlayer quando aterrissa
    public void OnLand()
    {
        if (isJumpAttacking)
        {
            Debug.Log("Jump Attack Landed!");
            ApplyAreaDamage(transform.position, attackRadius * 1.5f, baseDamage * jumpAttackDamageMultiplier);
            StartCoroutine(ActionCooldown(jumpAttackLandingLag));
            isJumpAttacking = false;
        }
    }

    void AttemptDashAttack()
    {
        GameObject nearestEnemy = FindNearestEnemyInRange(dashRange);
        if (nearestEnemy != null)
        {
            Debug.Log($"[DEBUG] Iniciando Dash Attack em direção a {nearestEnemy.name}.");
            if (dashCoroutine != null) StopCoroutine(dashCoroutine);
            dashCoroutine = StartCoroutine(ExecuteDashAttack(nearestEnemy.transform));
        }
        else
        {
            Debug.Log("[DEBUG] Nenhum inimigo próximo para dash attack.");
        }
    }

    IEnumerator ExecuteDashAttack(Transform target)
    {
        isDashing = true;
        canAttack = false;
        canDash = false;
        lastDashTime = Time.time;

        if (sfxDash != null && audioSource != null) audioSource.PlayOneShot(sfxDash);

        animator?.SetInteger(ANIM_ACTION_INT, 1); // Assumindo que MoveAttack1 é Action 1 no trigger AttackMove
        animator?.SetTrigger(ANIM_MOVE_ATTACK_TRIGGER);

        Vector3 start = transform.position;
        Vector3 directionToTarget = (target.position - start).normalized;
        // Evita direção zero se o alvo estiver na mesma posição
        if (directionToTarget == Vector3.zero) directionToTarget = transform.forward;
        Vector3 end = target.position - directionToTarget * 1.0f;

        float distance = Vector3.Distance(start, end);
        // Evita divisão por zero e dash muito curto
        float dashDuration = (dashSpeed > 0 && distance > 0.1f) ? distance / dashSpeed : 0.1f;
        float elapsed = 0f;

        if (cc != null) cc.enabled = false;

        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / dashDuration);
            transform.rotation = Quaternion.LookRotation(directionToTarget);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        if (cc != null) cc.enabled = true;

        Debug.Log("[DEBUG] Dash finalizado. Aplicando dano...");
        ApplyAreaDamage(transform.position, attackRadius, baseDamage * dashAttackDamageMultiplier);
        CheckRhythm();

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

        bool enemyHit = false;
        foreach (Collider enemyCollider in hitEnemies)
        {
            // Tenta obter EnemyAI, verifica se não é nulo antes de chamar TakeDamage
            EnemyAI enemyAI = enemyCollider.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyHit = true;
                Vector3 knockbackDirection = (enemyCollider.transform.position - transform.position).normalized;
                // Evita direção zero
                if (knockbackDirection == Vector3.zero) knockbackDirection = transform.forward;

                int finalDamage = Mathf.RoundToInt(damageAmount * currentDamageMultiplier);
                // Assume que EnemyAI tem knockbackForce, idealmente verificar se existe
                float knockbackForce = enemyAI.knockbackForce; // Exemplo, pegue o valor real
                enemyAI.TakeDamage(finalDamage, knockbackDirection, knockbackForce);
                Debug.Log($"[DEBUG] Dano aplicado a {enemyCollider.gameObject.name}: {finalDamage} (Multiplicador: {currentDamageMultiplier}x)");
            }
        }
    }

    void CheckRhythm()
    {
        if (!musicStarted || musicSource == null) return;

        musicTime = musicSource.time;
        // Evita divisão por zero se perfectAttackTime for 0
        if (perfectAttackTime <= 0) return;

        float beatProgress = (musicTime / perfectAttackTime) % 1f;
        float beatDistance = Mathf.Min(beatProgress, 1f - beatProgress);

        if (beatDistance <= timeMargin)
        {
            currentDamageMultiplier = Mathf.Min(currentDamageMultiplier * 2, maxMultiplier);
            Debug.Log($"[DEBUG] Ataque Perfeito! Multiplicador: {currentDamageMultiplier}x");
        }
    }

    IEnumerator ResetAttackChain(float delay)
    {
        yield return new WaitForSeconds(delay);
        attackChainCount = 0;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (isBlocking)
        {
            Debug.Log("Dano bloqueado!");
            animator?.SetTrigger(ANIM_BLOCK_IMPACT_TRIGGER);
            if (sfxBloqueio != null && audioSource != null) audioSource.PlayOneShot(sfxBloqueio);
            currentDamageMultiplier = 1;
            return;
        }

        playerHealth -= damage;
        Debug.Log($"[DEBUG] Jogador tomou {damage} de dano! Vida: {playerHealth}/{maxHealth}");

        if (sfxRecebeDano != null && audioSource != null) audioSource.PlayOneShot(sfxRecebeDano);
        animator?.SetTrigger(ANIM_GET_HIT_TRIGGER);

        currentDamageMultiplier = 1;
        Debug.Log("[DEBUG] Multiplicador resetado para 1x.");

        if (playerHealth <= 0)
        {
            playerHealth = 0;
            Die();
        }
        else
        {
            StartCoroutine(ActionCooldown(0.5f));
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("[DEBUG] Player morreu!");

        animator?.SetTrigger(ANIM_DIE_TRIGGER);
        if (sfxMorte != null && audioSource != null) audioSource.PlayOneShot(sfxMorte);

        if (cc != null) cc.enabled = false;
        // Não desabilitar o script Combat inteiro, pois pode ser necessário para resetar
        // this.enabled = false;

        // Chama a sequência de morte no MovimentoPlayer
        movimentoPlayer?.PlayerDeathSequence();
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

    IEnumerator ActionCooldown(float duration)
    {
        canAttack = false;
        canDash = false;
        canBlock = false;
        yield return new WaitForSeconds(duration);
        canAttack = true;
        canDash = true;
        canBlock = true;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsBlocking()
    {
        return isBlocking;
    }

    // ResetState pode ser chamado externamente para reviver o jogador
    public void ResetState()
    {
        playerHealth = maxHealth;
        isDead = false;
        isDashing = false;
        isBlocking = false;
        isJumpAttacking = false;
        currentDamageMultiplier = 1;
        lastDashTime = -Mathf.Infinity;
        lastAttackTime = -Mathf.Infinity;
        attackChainCount = 0;

        canAttack = true;
        canDash = true;
        canBlock = true;

        if (cc != null) cc.enabled = true;
        if (animator) {
             animator.ResetTrigger(ANIM_DIE_TRIGGER);
             animator.ResetTrigger(ANIM_GET_HIT_TRIGGER);
             animator.ResetTrigger(ANIM_ATTACK_TRIGGER);
             animator.ResetTrigger(ANIM_MOVE_ATTACK_TRIGGER);
             animator.ResetTrigger(ANIM_BLOCK_IMPACT_TRIGGER);
             animator.SetBool(ANIM_BLOCKING_BOOL, false);
             animator.SetInteger(ANIM_ACTION_INT, 0);
        }
        this.enabled = true; // Garante que o script está ativo
        Debug.Log("[DEBUG] Estado do PlayerCombat resetado.");
    }
}

