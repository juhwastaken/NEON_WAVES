using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public int playerHealth = 100;
    public int baseDamage = 20;
    public float attackRange = 5f;
    public LayerMask enemyLayer;
    public Transform attackPoint;
    public float attackRadius = 1.5f;

    private Animator animator;
    private bool isDead = false;

    private int currentDamageMultiplier = 1;
    private int maxMultiplier = 32;
    public float perfectAttackTime = 1.0833333334f;
    public float timeMargin = 0.1f; // Margem de erro configurável
    private float lastAttackTime = -Mathf.Infinity;
    private bool musicStarted = false;
    public AudioSource musicSource;
    float musicTime;

    void Start()
    {
        animator = GetComponent<Animator>();
       // musicSource = GetComponent<AudioSource>();
        // Se a música já estiver tocando ao iniciar o jogo
        if (musicSource != null && musicSource.isPlaying)
        {
            musicStarted = true;
        }
    }

    void Update()
    {
         //Debug.Log(musicSource.time);

        if (isDead) return; // Se o jogador morreu, não pode atacar

        // Verifica se a música começou a tocar
        if (!musicStarted && musicSource.isPlaying)
        {
            musicStarted = true;
        }

        if (!musicStarted) return; // Se a música não começou, o ataque não ativa

        if (Input.GetMouseButtonDown(0)) // Clique esquerdo para atacar
        {
            AttemptAttack();
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

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRadius, enemyLayer);
        bool enemyHit = false;

        foreach (Collider enemy in hitEnemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyHit = true; // Indica que um inimigo foi atingido

                Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                int finalDamage = baseDamage * currentDamageMultiplier;
                enemyAI.TakeDamage(finalDamage, knockbackDirection, enemyAI.knockbackForce);

                Debug.Log($"Ataque acertou! Dano: {finalDamage} ({currentDamageMultiplier}x)");
            }
        }

        if (enemyHit)
        {
            Debug.Log(Mathf.Min((musicTime / perfectAttackTime) % 1f, 1-( (musicTime / perfectAttackTime) % 1f)));


            // Somente aumenta o multiplicador se o jogador acertar um inimigo no tempo correto
            if (Mathf.Abs(Mathf.Min((musicTime / perfectAttackTime) % 1f, 1-( (musicTime / perfectAttackTime) % 1f))) <= timeMargin)
            {
                currentDamageMultiplier = Mathf.Min(currentDamageMultiplier * 2, maxMultiplier);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        playerHealth -= damage;
        Debug.Log($"Jogador tomou {damage} de dano! Vida: {playerHealth}");

        // Se tomar dano, reseta o multiplicador para 1x
        currentDamageMultiplier = 1;

        if (playerHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Die"); // Ativa a animação de morte
        Debug.Log("Player morreu!");

        // Desativa o controle do jogador (opcional)
        GetComponent<CharacterController>().enabled = false;
        this.enabled = false; // Desativa o script após a morte
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
