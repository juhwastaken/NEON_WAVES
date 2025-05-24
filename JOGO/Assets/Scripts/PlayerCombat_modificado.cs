using UnityEngine;
using System.Collections;

public class PlayerCombat_modificado : MonoBehaviour
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
    private CharacterController characterController;
    private bool isJumping = false;
    private bool isDoubleJumping = false;
    private float verticalVelocity = 0f;
    private float jumpForce = 8f;
    private float gravity = 20f;
    private bool isGrounded = true;

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
    [SerializeField] private AudioClip sfxJump;

    // Controle de animação de ataque
    private int attackCombo = 0;
    private float comboResetTime = 1.5f;
    private float lastComboTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        if (musicSource != null && musicSource.isPlaying)
        {
            musicStarted = true;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (!musicStarted && musicSource != null && musicSource.isPlaying)
        {
            musicStarted = true;
        }

        if (!musicStarted) return;

        // Verifica se o combo deve ser resetado
        if (Time.time > lastComboTime + comboResetTime)
        {
            attackCombo = 0;
        }

        // Controle de movimento
        HandleMovement();

        // Controle de pulo
        HandleJump();

        // Controle de ataque
        if (Input.GetMouseButtonDown(0))
        {
            AttemptAttack();
        }

        // Controle de dash
        if (Input.GetMouseButtonDown(1))
        {
            AttemptDash();
        }
    }

    void HandleMovement()
    {
        // Captura input de movimento
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Cria vetor de movimento
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;
        
        // Verifica se está se movendo
        bool isMoving = moveDirection.magnitude > 0.1f;
        
        // Atualiza parâmetro de movimento no Animator
        animator.SetBool("Moving", isMoving);
        
        // Se estiver se movendo, define a velocidade de movimento
        if (isMoving)
        {
            // Verifica se está correndo (segurando Shift)
            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            
            // Atualiza parâmetro de corrida no Animator
            animator.SetBool("Running", isRunning);
            
            // Define a velocidade baseada em correr ou andar
            float speed = isRunning ? 5f : 3f;
            
            // Aplica movimento via CharacterController
            if (characterController != null && !isDashing)
            {
                // Rotaciona o personagem na direção do movimento
                transform.rotation = Quaternion.LookRotation(moveDirection);
                
                // Aplica movimento horizontal
                characterController.Move(moveDirection * speed * Time.deltaTime);
            }
        }
        else
        {
            // Se não está se movendo, desativa animação de corrida
            animator.SetBool("Running", false);
        }
    }

    void HandleJump()
    {
        // Verifica se está no chão
        isGrounded = characterController != null && characterController.isGrounded;
        
        // Atualiza parâmetro de grounded no Animator
        animator.SetBool("Grounded", isGrounded);
        
        // Aplica gravidade
        if (!isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = -0.5f; // Pequena força para baixo quando no chão
            
            // Reseta flags de pulo quando tocar o chão
            if (isJumping || isDoubleJumping)
            {
                isJumping = false;
                isDoubleJumping = false;
                
                // Trigger de aterrissagem
                animator.SetTrigger("Land");
            }
        }
        
        // Verifica input de pulo
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Pulo normal se estiver no chão
            if (isGrounded)
            {
                Jump();
            }
            // Pulo duplo se já estiver no ar e não tiver feito pulo duplo ainda
            else if (isJumping && !isDoubleJumping)
            {
                DoubleJump();
            }
        }
        
        // Aplica movimento vertical
        if (characterController != null && !isDashing)
        {
            characterController.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
        }
    }

    void Jump()
    {
        // Aplica força de pulo
        verticalVelocity = jumpForce;
        isJumping = true;
        
        // Configura parâmetros do Animator
        animator.SetTrigger("Jump");
        animator.SetBool("Jumping", true);
        
        // Toca som de pulo
        if (sfxJump != null && audioSource != null)
            audioSource.PlayOneShot(sfxJump);
    }

    void DoubleJump()
    {
        // Aplica força de pulo novamente
        verticalVelocity = jumpForce * 0.8f; // Um pouco menos de força no segundo pulo
        isDoubleJumping = true;
        
        // Configura parâmetros do Animator
        animator.SetTrigger("DoubleJump");
        
        // Toca som de pulo
        if (sfxJump != null && audioSource != null)
            audioSource.PlayOneShot(sfxJump);
    }

    void AttemptAttack()
    {
        float currentTime = Time.time;
        float timeSinceLastAttack = currentTime - lastAttackTime;
        musicTime = musicSource.time;

        // Incrementa o combo
        attackCombo = (attackCombo % 3) + 1;
        lastComboTime = currentTime;
        
        // Configura parâmetros do Animator para o ataque
        animator.SetInteger("Action", attackCombo);
        animator.SetInteger("TriggerNumber", 4); // Valor para ataques normais
        animator.SetTrigger("Trigger");

        lastAttackTime = currentTime;
        Attack(timeSinceLastAttack);
    }

    void Attack(float timeSinceLastAttack)
    {
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
            else
            {
                // Reseta o multiplicador se errar o ritmo
                currentDamageMultiplier = 1;
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
                // Configura parâmetros do Animator para o dash
                animator.SetInteger("Action", 1);
                animator.SetInteger("TriggerNumber", 11); // Valor para MoveAttack1 (dash)
                animator.SetTrigger("Trigger");
                
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

        // Configura parâmetros do Animator para receber dano
        animator.SetTrigger("GetHit");

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

        if (characterController != null)
            characterController.enabled = false;
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
            else
            {
                // Reseta o multiplicador se errar o ritmo
                currentDamageMultiplier = 1;
            }
        }

        yield return new WaitForSeconds(0.5f);
        isInvulnerable = false;
        isDashing = false;

        Debug.Log("Jogador não está mais invulnerável.");
    }
}
