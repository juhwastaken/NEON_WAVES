using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private Transform player;
    public float speed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public int enemyHealth = 100;
    public int maxHealth = 100;
    public int damage = 10;

    private float attackCooldown = 1f;
    private float nextAttackTime = 0f;

    private Rigidbody rb;
    private bool isKnockedBack = false;
    private float knockbackDuration = 0.2f;

    public float knockbackForce = 10f;

    private Animator animator; // <<< adicionado

    private bool isDead = false; // <<< adicionado para não mover/atacar depois da morte

    private AudioSource musicPlayer;


    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player não encontrado! Certifique-se de que o jogador tem a tag 'Player'.");
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody não encontrado no inimigo.");
        }

        rb.useGravity = false;

        animator = GetComponent<Animator>(); // <<< adiciona o Animator
        if (animator == null)
        {
            Debug.LogError("Animator não encontrado no inimigo.");
        }
        musicPlayer = GameObject.Find("GameplayMusicPlayer")?.GetComponent<AudioSource>();
        if (musicPlayer == null)
        {
            Debug.LogError("AudioSource da música não encontrado! Verifique se o objeto 'GameplayMusicPlayer' possui um AudioSource.");
        }
    }

    void Update()
    {
        if (player == null || isKnockedBack || isDead) return; // <<< impede movimentação se morreu

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < detectionRange)
        {
            MoveTowardsPlayer();
        }

        if (distance < attackRange && Time.time >= nextAttackTime)
        {
            AttackPlayer();
            nextAttackTime = Time.time + attackCooldown;
        }
        if (musicPlayer != null && !musicPlayer.isPlaying)
        {
            Destroy(gameObject);
            return;
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        rb.MovePosition(transform.position + direction * speed * Time.deltaTime);

        Vector3 lookDirection = new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position;
        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
    }

    void AttackPlayer()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack"); // <<< toca animação de ataque
        }

        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage, Vector3 knockbackDirection, float knockbackForce)
    {
        if (isDead) return; // <<< evita tomar dano depois de morto

        enemyHealth -= damage;

        if (enemyHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(Knockback(knockbackDirection, knockbackForce));
        }
    }

    IEnumerator Knockback(Vector3 direction, float force)
    {
        isKnockedBack = true;
        rb.velocity = direction * force;
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true; // <<< marca como morto
        Debug.Log("Enemy morreu!");

        if (animator != null)
        {
            animator.SetTrigger("Death"); // <<< toca animação de morte
        }

        // Destroi o inimigo depois que a animação terminar
        Destroy(gameObject, 2f); // <<< tempo suficiente pra animação de morte rodar (ajuste se precisar)
    }
}
