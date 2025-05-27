using UnityEngine;
using System.Collections;

// Renomeado para refletir as novas correções
public class PlayerCombat : MonoBehaviour 
{
    [Header("Stats")]
    public int playerHealth = 100;
    public int maxHealth = 100; 
    public int baseDamage = 20; 

    [Header("Attack Settings")]
    public float attackRange = 5f; 
    public LayerMask enemyLayer; 
    public Transform attackPoint; 
    public float attackRadius = 1.5f; 

    [Header("Dash Settings")]
    public float dashRange = 10f; 
    public float dashSpeed = 20f;
    public float dashCooldown = 5f;
    private float lastDashTime = -Mathf.Infinity;
    private bool isDashing = false;
    private bool isInvulnerable = false; 
    private Coroutine dashCoroutine;

    [Header("Rhythm Mechanics")]
    public AudioSource musicSource; 
    public float perfectAttackTime = 1.0833333334f; 
    public float timeMargin = 0.1f; 
    private int currentDamageMultiplier = 1; 
    private int maxMultiplier = 32; 
    private float lastAttackTime = -Mathf.Infinity;
    private bool musicStarted = false;
    private float musicTime;

    [Header("Components")]
    private Animator animator;
    private CharacterController cc; // Adicionado para referência

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private AudioClip sfxAtaque;
    [SerializeField] private AudioClip sfxRecebeDano;
    [SerializeField] private AudioClip sfxDash;
    [SerializeField] private AudioClip sfxMorte;

    // State
    private bool isDead = false;

    // Constantes para animação MoveAttack1 (baseado na imagem do Animator)
    private const int MOVE_ATTACK_ACTION_NUMBER = 1;
    private const int MOVE_ATTACK_TRIGGER_NUMBER = 11; 

    void Start()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>(); // Pega referência ao CharacterController

        playerHealth = maxHealth; 

        if (musicSource != null && musicSource.isPlaying)
        {
            musicStarted = true;
        }

        if (animator == null) Debug.LogError("Animator não encontrado no Player!");
        if (cc == null) Debug.LogWarning("CharacterController não encontrado no Player, algumas funcionalidades podem ser afetadas.");
    }

    void Update()
    {
        if (isDead) return; 

        if (!musicStarted && musicSource != null && musicSource.isPlaying)
        {
            musicStarted = true;
        }

        if (!musicStarted) return; 

        if (Input.GetMouseButtonDown(0) && !isDashing) 
        {
            AttemptAttack();
        }

        if (Input.GetMouseButtonDown(1) && !isDashing) 
        {
            AttemptDash();
        }

        if (Input.GetKeyDown(KeyCode.T)) 
        {
            Debug.Log("[DEBUG] Teste de Dano Manual!");
            TakeDamage(10);
        }
    }

    void AttemptAttack()
    {
        musicTime = musicSource != null ? musicSource.time : 0;
        lastAttackTime = Time.time;
        Attack();
    }

    void Attack()
    {
        Debug.Log("[DEBUG] Iniciando Ataque Normal.");
        if (animator) animator.SetTrigger("Atacar"); 

        if (sfxAtaque != null && audioSource != null)
            audioSource.PlayOneShot(sfxAtaque);

        if (attackPoint == null) {
            Debug.LogError("[DEBUG] AttackPoint não está configurado no Inspector!");
            return;
        }

        Debug.Log($"[DEBUG] Verificando inimigos no raio {attackRadius} a partir de {attackPoint.position} na layer {LayerMask.LayerToName(enemyLayer.value)}.");
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRadius, enemyLayer);
        bool enemyHit = false;
        Debug.Log($"[DEBUG] Inimigos encontrados na área de ataque: {hitEnemies.Length}");

        foreach (Collider enemyCollider in hitEnemies)
        {
            Debug.Log($"[DEBUG] Tentando obter EnemyAI do objeto: {enemyCollider.gameObject.name}");
            EnemyAI enemyAI = enemyCollider.GetComponent<EnemyAI>(); 
            if (enemyAI != null)
            {
                Debug.Log($"[DEBUG] EnemyAI encontrado em {enemyCollider.gameObject.name}. Aplicando dano...");
                enemyHit = true;
                Vector3 knockbackDirection = (enemyCollider.transform.position - transform.position).normalized;
                int finalDamage = baseDamage * currentDamageMultiplier;
                enemyAI.TakeDamage(finalDamage, knockbackDirection, enemyAI.knockbackForce);
                Debug.Log($"[DEBUG] Dano aplicado a {enemyCollider.gameObject.name}: {finalDamage}");
            }
            else
            {
                Debug.LogWarning($"[DEBUG] Componente EnemyAI NÃO encontrado em {enemyCollider.gameObject.name}. Verifique se o script está anexado e o nome da classe está correto.");
            }
        }

        if (enemyHit && musicStarted)
        {
            float beatDistance = Mathf.Min((musicTime / perfectAttackTime) % 1f, 1f - ((musicTime / perfectAttackTime) % 1f));
            if (beatDistance <= timeMargin)
            {
                currentDamageMultiplier = Mathf.Min(currentDamageMultiplier * 2, maxMultiplier);
                Debug.Log($"[DEBUG] Ataque Perfeito! Multiplicador: {currentDamageMultiplier}x");
            }
        }
    }

    void AttemptDash()
    {
        if (Time.time >= lastDashTime + dashCooldown) 
        {
            GameObject nearestEnemy = FindNearestEnemyInRange();
            if (nearestEnemy != null)
            {
                Debug.Log($"[DEBUG] Iniciando Dash em direção a {nearestEnemy.name}.");
                if (dashCoroutine != null) StopCoroutine(dashCoroutine);
                dashCoroutine = StartCoroutine(ExecuteDash(nearestEnemy.transform));
            }
            else
            {
                Debug.Log("[DEBUG] Nenhum inimigo próximo para dash.");
            }
        }
        else
        {
            Debug.Log("[DEBUG] Dash em cooldown.");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable) 
        {
            if (isInvulnerable) Debug.Log("[DEBUG] Dano ignorado (invulnerável).");
            return;
        }

        playerHealth -= damage;
        Debug.Log($"[DEBUG] Jogador tomou {damage} de dano! Vida: {playerHealth}/{maxHealth}");

        if (sfxRecebeDano != null && audioSource != null)
            audioSource.PlayOneShot(sfxRecebeDano);

        currentDamageMultiplier = 1;
        Debug.Log("[DEBUG] Multiplicador resetado para 1x.");

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

        if (animator) animator.SetTrigger("Die");

        if (sfxMorte != null && audioSource != null)
            audioSource.PlayOneShot(sfxMorte);

        if (cc != null) cc.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, dashRange);
    }

    GameObject FindNearestEnemyInRange()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, dashRange, enemyLayer);
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

    IEnumerator ExecuteDash(Transform target)
    {
        isDashing = true;
        isInvulnerable = true; 
        lastDashTime = Time.time;

        if (sfxDash != null && audioSource != null)
            audioSource.PlayOneShot(sfxDash);

        // --- CORRIGIDO: Aciona a animação MoveAttack1 com as condições corretas --- 
        if (animator) 
        {
            Debug.Log($"[DEBUG] Acionando animação MoveAttack1 (Action: {MOVE_ATTACK_ACTION_NUMBER}, TriggerNumber: {MOVE_ATTACK_TRIGGER_NUMBER})");
            // Garante que não está se movendo ou pulando para a animação (conforme condições)
            animator.SetBool("Moving", false); 
            animator.SetInteger("Jumping", 0); 
            // Define os parâmetros da ação específica
            animator.SetInteger("Action", MOVE_ATTACK_ACTION_NUMBER); 
            animator.SetInteger("TriggerNumber", MOVE_ATTACK_TRIGGER_NUMBER); 
            animator.SetTrigger("Trigger"); // Aciona o gatilho genérico
        }
        else
        {
            Debug.LogWarning("[DEBUG] Animator não encontrado para acionar animação de Dash.");
        }

        Vector3 start = transform.position;
        Vector3 end = target.position; 

        float dashDuration = Vector3.Distance(start, end) / dashSpeed;
        float elapsed = 0f;

        musicTime = musicSource != null ? musicSource.time : 0;
        float beatDistance = Mathf.Min((musicTime / perfectAttackTime) % 1f, 1f - ((musicTime / perfectAttackTime) % 1f));

        if (cc != null) cc.enabled = false; // Desabilita CC para mover manualmente

        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        if (cc != null) cc.enabled = true; // Reabilita CC

        Debug.Log("[DEBUG] Dash finalizado. Tentando causar dano...");

        Debug.Log($"[DEBUG] Tentando obter EnemyAI do objeto alvo do dash: {target.gameObject.name}");
        EnemyAI enemy = target.GetComponent<EnemyAI>(); 
        if (enemy != null)
        {
            Debug.Log($"[DEBUG] EnemyAI encontrado em {target.gameObject.name}. Aplicando dano de dash...");
            int finalDamage = baseDamage * currentDamageMultiplier;
            Vector3 knockbackDir = (target.position - transform.position).normalized;
            enemy.TakeDamage(finalDamage, knockbackDir, enemy.knockbackForce);
            Debug.Log($"[DEBUG] Dano de dash aplicado a {target.gameObject.name}: {finalDamage}");

            if (musicStarted && beatDistance <= timeMargin)
            {
                currentDamageMultiplier = Mathf.Min(currentDamageMultiplier * 2, maxMultiplier);
                Debug.Log($"[DEBUG] Dash Perfeito! Multiplicador: {currentDamageMultiplier}x");
            }
        }
         else
        {
            Debug.LogWarning($"[DEBUG] Componente EnemyAI NÃO encontrado no alvo do dash: {target.gameObject.name}. Verifique se o script está anexado e o nome da classe está correto.");
        }

        yield return new WaitForSeconds(0.1f);
        isInvulnerable = false;
        isDashing = false;

        // Resetar parâmetros do animator após o dash pode ser necessário
        // if (animator) {
        //    animator.SetBool("Moving", true); // Ou o valor que deveria ter
        // }

        Debug.Log("[DEBUG] Jogador não está mais invulnerável/dashing.");
    }

    public void ResetState()
    {
        playerHealth = maxHealth;
        isDead = false;
        isDashing = false;
        isInvulnerable = false;
        currentDamageMultiplier = 1;
        lastDashTime = -Mathf.Infinity;
        lastAttackTime = -Mathf.Infinity;
        if (cc != null) cc.enabled = true;
        if (animator) {
             animator.ResetTrigger("Die");
             // Resetar outros triggers/parâmetros se necessário
        }
        this.enabled = true;
        Debug.Log("[DEBUG] Estado do PlayerCombat resetado.");
    }
}

