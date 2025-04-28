using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private Transform player;
    public float speed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public int enemyHealth = 100;
    public int maxHealth = 100; // Vida máxima
    public int damage = 10;

    private float attackCooldown = 1f;
    private float nextAttackTime = 0f;

    private Rigidbody rb;
    private bool isKnockedBack = false;
    private float knockbackDuration = 0.2f;

    public float knockbackForce = 10f;

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

        // Faz o Rigidbody ignorar a gravidade para poder voar
        rb.useGravity = false;
    }

    void Update()
    {
        if (player == null || isKnockedBack) return;

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
    }

    void MoveTowardsPlayer()
    {
        // Move tanto na horizontal quanto na vertical
        Vector3 direction = (player.position - transform.position).normalized;

        // Move usando Rigidbody (levando o Y em conta também!)
        rb.MovePosition(transform.position + direction * speed * Time.deltaTime);

        // Faz o inimigo olhar pro player (apenas no plano horizontal se quiser)
        Vector3 lookDirection = new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position;
        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
    }

    void AttackPlayer()
    {
        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage, Vector3 knockbackDirection, float knockbackForce)
    {
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
        Debug.Log("Enemy morreu!");
        Destroy(gameObject);
    }
}
