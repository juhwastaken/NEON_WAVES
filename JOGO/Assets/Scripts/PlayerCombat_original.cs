using UnityEngine;
using System.Collections;

public class PlayerCombat_original : MonoBehaviour
{
    public int playerHealth = 100;
    public int baseDamage = 20;
    public float attackRange = 5f;
    public LayerMask enemyLayer;
    public Transform attackPoint;
    public float attackRadius = 1.5f;

    public float dashRange = 10f;
    public float dashSpeed = 20f;
    public float dashCooldown = 5f;
    private float lastDashTime = -Mathf.Infinity;
    private bool isDashing = false;
    private bool isInvulnerable = false;
    private Coroutine dashCoroutine;

    private Animator animator;
    private bool isDead = false;

    private int currentDamageMultiplier = 1;
    private int maxMultiplier = 32;
    public float perfectAttackTime = 1.0833333334f;
    public float timeMargin = 0.1f;
    private float lastAttackTime = -Mathf.Infinity;
    private bool musicStarted = false;
    public AudioSource musicSource;
    private float musicTime;

    // 🎵 SFX de combate
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxAtaque;
    [SerializeField] private AudioClip sfxRecebeDano;
    [SerializeField] private AudioClip sfxDash;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (musicSource != null && musicSource.isPlaying)
        {
            musicStarted = true;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (!musicStarted && musicSource.isPlaying)
        {
            musicStarted = true;
        }

        if (!musicStarted) return;

        if (Input.GetMouseButtonDown(0))
        {
            AttemptAttack();
        }

        if (Input.GetMouseButtonDown(1))
        {
            AttemptDash();
        }
    }

    void AttemptAttack()
    {
        float currentTime = Time.time;
        float timeSinceLastAttack = currentTime - lastAttackTime;
        musicTime = musicSource.time;

        lastAttackTime = currentTime;
        Attack(timeSinceLastAttack);
    }

    void Attack(float timeSinceLastAttack)
    {
        animator.SetTrigger("Atacar");

        // 🔊 Toca som de ataque
        if (sfxAtaque != null && audioSource != null)
            audioSource.PlayOneShot(sfxAtaque);

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRadius, enemyLayer);
        bool enemyHit = false;

        foreach (Collider enemy in hitEnemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyHit = true;

                Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                int finalDamage = baseDamage * currentDamageMultiplier;
                enemyAI.TakeDamage(finalDamage, knockbackDirection, enemyAI.knockbackForce);

                Debug.Log($"Ataque acertou! Dano: {finalDamage} ({currentDamageMultiplier}x)");
            }
        }

        if (enemyHit)
        {
            float beatDistance = Mathf.Min((musicTime / perfectAttackTime) % 1f, 1f - ((musicTime / perfectAttackTime) % 1f));
            Debug.Log($"Beat Distance (Attack): {beatDistance}");

            if (beatDistance <= timeMargin)
            {
                currentDamageMultiplier = Mathf.Min(currentDamageMultiplier * 2, maxMultiplier);
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
                if (dashCoroutine != null) StopCoroutine(dashCoroutine);
                dashCoroutine = StartCoroutine(ExecuteDash(nearestEnemy.transform));
            }
            else
            {
                Debug.Log("Nenhum inimigo próximo para dash.");
            }
        }
        else
        {
            Debug.Log("Dash em cooldown.");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (isInvulnerable)
        {
            Debug.Log("Dano ignorado (invulnerável durante o dash).");
            return;
        }

        playerHealth -= damage;
        Debug.Log($"Jogador tomou {damage} de dano! Vida: {playerHealth}");

        // 🔊 Toca som de dano
        if (sfxRecebeDano != null && audioSource != null)
            audioSource.PlayOneShot(sfxRecebeDano);

        currentDamageMultiplier = 1;

        if (playerHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        Debug.Log("Player morreu!");

        GetComponent<CharacterController>().enabled = false;
        this.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
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

        // 🔊 Toca som de dash
        if (sfxDash != null && audioSource != null)
            audioSource.PlayOneShot(sfxDash);

        Vector3 start = transform.position;
        Vector3 end = target.position;

        float dashDuration = Vector3.Distance(start, end) / dashSpeed;
        float elapsed = 0f;

        musicTime = musicSource.time;
        float beatDistance = Mathf.Min((musicTime / perfectAttackTime) % 1f, 1f - ((musicTime / perfectAttackTime) % 1f));
        Debug.Log($"Beat Distance (Dash): {beatDistance}");

        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        Debug.Log("Dash finalizado. Causando dano...");

        EnemyAI enemy = target.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            int finalDamage = baseDamage * currentDamageMultiplier;
            Vector3 knockbackDir = (target.position - transform.position).normalized;
            enemy.TakeDamage(finalDamage, knockbackDir, enemy.knockbackForce);

            Debug.Log($"Dano de dash aplicado: {finalDamage}");

            if (beatDistance <= timeMargin)
            {
                currentDamageMultiplier = Mathf.Min(currentDamageMultiplier * 2, maxMultiplier);
            }
        }

        yield return new WaitForSeconds(0.5f);
        isInvulnerable = false;
        isDashing = false;

        Debug.Log("Jogador não está mais invulnerável.");
    }
}
