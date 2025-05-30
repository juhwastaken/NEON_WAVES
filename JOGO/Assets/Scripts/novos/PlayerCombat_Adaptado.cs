using UnityEngine;
using System.Collections;

// Versão Adaptada: Usa os parâmetros do Animator fornecido pelo usuário.
public class PlayerCombat_Adaptado : MonoBehaviour
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
    // Removido: Combo logic (attackChainCount, attackResetCoroutine)

    [Header("Dash Attack Settings")]
    public float dashRange = 10f;
    public float dashSpeed = 20f;
    public float dashCooldown = 3f;
    public float dashAttackDamageMultiplier = 1.5f;
    private float lastDashTime = -Mathf.Infinity;
    private bool isDashing = false;
    private Coroutine dashCoroutine;

    [Header("Components")]
    private Animator animator;
    private CharacterController cc;
    // Referência ao script de movimento adaptado
    private MovimentoPlayer_Adaptado movimentoPlayer;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxAtaque; // Um SFX para todos os ataques
    [SerializeField] private AudioClip sfxDash;
    [SerializeField] private AudioClip sfxMorte;

    // State
    private bool isDead = false;
    private bool canAttack = true;
    private bool canDash = true;

    // Constantes de Animação - Adaptadas ao Animator do usuário
    private const string ANIM_ATACAR = "Atacar"; // Trigger para Ataque Básico e Dash Attack
    private const string ANIM_DIE = "Die";       // Trigger para a morte
    // Removido: Action Int, AttackTrigger, AttackMoveTrigger

    void Start()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        // Garante que pega o nome correto do script de movimento adaptado
        movimentoPlayer = GetComponent<MovimentoPlayer_Adaptado>();

        playerHealth = maxHealth;

        if (animator == null) Debug.LogError("Animator não encontrado!");
        if (cc == null) Debug.LogWarning("CharacterController não encontrado!");
        if (movimentoPlayer == null) Debug.LogError("MovimentoPlayer_Adaptado não encontrado! Algumas funcionalidades podem ser limitadas.");
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

        // Dash Attack (Mouse 1)
        if (Input.GetMouseButtonDown(1) && canDash && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            AttemptDashAttack();
        }

        // Teste de Morte (Exemplo)
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(maxHealth + 1);
        }
    }

    void AttemptAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        // Removido: Lógica de combo

        Attack();
    }

    void Attack()
    {
        Debug.Log("[DEBUG] Iniciando Ataque.");
        // Aciona a animação de ataque usando o trigger 'Atacar'
        animator?.SetTrigger(ANIM_ATACAR);

        // Toca SFX de ataque
        if (sfxAtaque != null && audioSource != null) audioSource.PlayOneShot(sfxAtaque);

        // Aplica o dano após um pequeno delay
        StartCoroutine(ApplyAttackDamage(0.2f, baseDamage));
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
            Debug.Log("[DEBUG] Nenhum inimigo próximo. Dash para frente.");
            if (dashCoroutine != null) StopCoroutine(dashCoroutine);
            Vector3 forwardTargetPos = transform.position + transform.forward * dashRange;
            Transform forwardTarget = new GameObject("DashTargetForward").transform;
            forwardTarget.position = forwardTargetPos;
            dashCoroutine = StartCoroutine(ExecuteDashAttack(forwardTarget, true));
        }
    }

    IEnumerator ExecuteDashAttack(Transform target, bool destroyTarget = false)
    {
        isDashing = true;
        canAttack = false;
        canDash = false;
        lastDashTime = Time.time;

        if (sfxDash != null && audioSource != null) audioSource.PlayOneShot(sfxDash);

        // Aciona a animação de Dash Attack usando o mesmo trigger 'Atacar'
        animator?.SetTrigger(ANIM_ATACAR);

        Vector3 start = transform.position;
        Vector3 directionToTarget = (target.position - start).normalized;
        if (directionToTarget == Vector3.zero) directionToTarget = transform.forward;

        Vector3 end = target.position - directionToTarget * 1.0f;
        if (destroyTarget) end = target.position;

        float distance = Vector3.Distance(start, end);
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

        if (destroyTarget) Destroy(target.gameObject);

        Debug.Log("[DEBUG] Dash finalizado. Aplicando dano...");
        ApplyAreaDamage(transform.position, attackRadius, baseDamage * dashAttackDamageMultiplier);

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
            var damageable = enemyCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage((int)damageAmount);
                Debug.Log($"[DEBUG] Dano aplicado a {enemyCollider.gameObject.name}: {damageAmount}");
            }
        }
    }

    // Removido: ResetAttackChain

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        playerHealth -= damage;
        Debug.Log($"[DEBUG] Jogador tomou {damage} de dano! Vida: {playerHealth}/{maxHealth}");

        if (playerHealth <= 0)
        {
            playerHealth = 0;
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("[DEBUG] Player morreu!");

        // Aciona a animação de morte usando o trigger 'Die'
        animator?.SetTrigger(ANIM_DIE);
        if (sfxMorte != null && audioSource != null) audioSource.PlayOneShot(sfxMorte);

        if (cc != null) cc.enabled = false;
        movimentoPlayer?.PlayerDeathSequence();

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

    public bool IsDead()
    {
        return isDead;
    }

    public void ResetState()
    {
        playerHealth = maxHealth;
        isDead = false;
        isDashing = false;
        lastDashTime = -Mathf.Infinity;
        lastAttackTime = -Mathf.Infinity;
        // Removido: attackChainCount

        canAttack = true;
        canDash = true;

        if (cc != null) cc.enabled = true;
        if (animator) {
             animator.ResetTrigger(ANIM_DIE);
             animator.ResetTrigger(ANIM_ATACAR);
             // Removido: Reset de Action Int e AttackMoveTrigger
        }
        this.enabled = true;
        Debug.Log("[DEBUG] Estado do PlayerCombat_Simplificado resetado.");
    }
}

