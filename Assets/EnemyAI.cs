using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip attackSound;
    public float attackSoundDuration;
    [Range(0f, 1f)] public float attackSoundVolume = 1f;

    [Header("Detection")]
    public float detectionRange = 6f;   // distance pour détecter le joueur
    public float loseRange = 10f;       // distance pour perdre le joueur
    public float loseDelay = 2f;        // secondes avant de lâcher le joueur après l'avoir semé

    [Header("Movement")]
    public float chaseSpeed = 3f;       // vitesse de poursuite (plus lent que le joueur)
    public float idleSpeed = 0.8f;      // vitesse de patrouille aléatoire

    [Header("Combat")]
    public float attackRange = 0.8f;    // distance pour attaquer
    public float attackCooldown = 1f;   // secondes entre chaque attaque
    public int attackDamage = 1;        // dégâts infligés

   
    [Header("Knockback")]
    public float knockbackDamping = 8f;
    private Vector2 knockbackVelocity = Vector2.zero;

    public void ApplyKnockback(Vector2 direction, float force)
    {
        knockbackVelocity = direction * force;
    }
    private enum State { Idle, Chase, Attack }
    private State currentState = State.Idle;

    private Rigidbody2D rb;
    private Transform player;

    // timer pour perdre le joueur
    private float loseTimer = 0f;
    private bool playerInSight = false;

    // timer d'attaque
    private float attackTimer = 0f;

    // patrouille aléatoire
    private Vector2 idleDirection;
    private float idleMoveTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        PickNewIdleDirection();
    }

    void Start()
    {
        // Trouve le joueur automatiquement via le tag "Player"
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("EnemyAI: aucun GameObject avec le tag 'Player' trouvé !");
    }

    void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(rb.position, player.position);

        UpdateSightAndState(distToPlayer);
        UpdateAttackTimer();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // Dissipe le knockback
        knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, knockbackDamping * Time.fixedDeltaTime);

        if (knockbackVelocity.magnitude > 0.05f)
        {
            rb.MovePosition(rb.position + knockbackVelocity * Time.fixedDeltaTime);
            return; // pendant le knockback, l'IA ne bouge pas
        }

        switch (currentState)
        {
            case State.Idle:
                DoIdlePatrol();
                break;

            case State.Chase:
                DoChase();
                break;

            case State.Attack:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    // -------------------------------------------------------
    //  Logique de détection / perte du joueur
    // -------------------------------------------------------
    void UpdateSightAndState(float dist)
    {
        playerInSight = dist <= detectionRange;

        if (playerInSight)
        {
            loseTimer = 0f;

            if (dist <= attackRange)
                currentState = State.Attack;
            else
                currentState = State.Chase;
        }
        else
        {
            // Le joueur est sorti du range de détection
            if (currentState == State.Chase || currentState == State.Attack)
            {
                // Continue de chercher pendant loseDelay secondes
                loseTimer += Time.deltaTime;

                if (loseTimer >= loseDelay)
                {
                    // Le joueur a semé l'ennemi !
                    currentState = State.Idle;
                    loseTimer = 0f;
                    PickNewIdleDirection();
                }
            }
        }
    }

    // -------------------------------------------------------
    //  Mouvement de poursuite
    // -------------------------------------------------------
    void DoChase()
    {
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * chaseSpeed * Time.fixedDeltaTime);
    }

    // -------------------------------------------------------
    //  Patrouille aléatoire à l'état Idle
    // -------------------------------------------------------
    void DoIdlePatrol()
    {
        idleMoveTimer -= Time.fixedDeltaTime;

        if (idleMoveTimer <= 0f)
            PickNewIdleDirection();

        rb.MovePosition(rb.position + idleDirection * idleSpeed * Time.fixedDeltaTime);
    }

    void PickNewIdleDirection()
    {
        // direction aléatoire, parfois s'arrête
        float rand = Random.value;
        if (rand < 0.3f)
            idleDirection = Vector2.zero; // pause
        else
            idleDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

        idleMoveTimer = Random.Range(1f, 3f);
    }

    // -------------------------------------------------------
    //  Attaque
    // -------------------------------------------------------
    void UpdateAttackTimer()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (currentState == State.Attack && attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
    }

    void PerformAttack()
    {
        if (player == null) return;

        playerHealth health = player.GetComponent<playerHealth>();

        if (health != null)
        {
            health.TakeDamage(attackDamage);


        }
    }

    // -------------------------------------------------------
    //  Gizmos — visualisation dans l'éditeur
    // -------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        // Cercle de détection (jaune)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Cercle de perte (rouge)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        // Cercle d'attaque (magenta)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
