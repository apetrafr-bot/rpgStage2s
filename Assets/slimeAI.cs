using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class slimeAI : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip hopSound;
    public float hopSoundDuration;
    [Range(0f, 1f)] public float hopSoundVolume = 1f;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float loseRange = 8f;
    public float loseDelay = 3f;

    [Header("Saut")]
    public float hopSpeed = 4f;
    public float hopCooldown = 0.8f;
    public float idleHopCooldown = 2f;

    [Header("Combat")]
    public float attackRange = 1f;
    public int attackDamage = 1;

    private enum State { Idle, Chase, Attack }
    private State currentState = State.Idle;

    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;

    private float stateCooldown = 0f;
    private bool isHopping = false;
    private Vector2 hopTarget;
    private Vector2 hopStart;
    private float hopTimer = 0f;
    private float hopDuration = 0.3f;

    private float loseTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        sr = GetComponent<SpriteRenderer>();
        stateCooldown = idleHopCooldown;
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        UpdateState(dist);
    }

    void FixedUpdate()
    {
        if (isHopping)
            UpdateHop();
    }

    void UpdateState(float dist)
    {
        if (isHopping) return;

        bool canSeePlayer = dist <= detectionRange;

        if (canSeePlayer)
        {
            loseTimer = 0f;

            if (dist <= attackRange)
                currentState = State.Attack;
            else
                currentState = State.Chase;
        }
        else
        {
            if (currentState == State.Chase || currentState == State.Attack)
            {
                loseTimer += Time.deltaTime;
                if (loseTimer >= loseDelay)
                {
                    currentState = State.Idle;
                    loseTimer = 0f;
                    stateCooldown = idleHopCooldown;
                }
            }
        }

        stateCooldown -= Time.deltaTime;

        if (stateCooldown > 0f) return;

        switch (currentState)
        {
            case State.Idle:
                HopRandom();
                break;
            case State.Chase:
                HopToward(player.position);
                break;
            case State.Attack:
                HopToward(player.position);
                currentState = State.Chase;
                break;
        }
    }

    void HopRandom()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        hopTarget = rb.position + dir * Random.Range(1f, 2.5f);
        StartHop();
    }

    void HopToward(Vector3 target)
    {
        Vector2 dir = ((Vector2)target - rb.position).normalized;
        hopTarget = rb.position + dir * Random.Range(1.5f, 3f);
        StartHop();
    }

    void StartHop()
    {
        isHopping = true;
        hopStart = rb.position;
        hopTimer = 0f;
        stateCooldown = hopCooldown;
        UpdateFlip();



        if (sr != null)
            StartCoroutine(SquashAndStretch());
    }

    void UpdateHop()
    {
        hopTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(hopTimer / hopDuration);

        Vector2 pos = Vector2.Lerp(hopStart, hopTarget, t);
        rb.MovePosition(pos);

        if (t >= 1f)
        {
            isHopping = false;
            transform.localScale = Vector3.one;

            Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange);
            if (hit != null && hit.CompareTag("Player"))
            {
                playerHealth health = hit.GetComponent<playerHealth>();
                if (health != null)
                    health.TakeDamage(attackDamage);
            }
        }
    }

    IEnumerator SquashAndStretch()
    {
        float duration = hopDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            if (t < 0.2f)
            {
                float squash = Mathf.Lerp(1f, 0.6f, t / 0.2f);
                transform.localScale = new Vector3(1f / squash, squash, 1f);
            }
            else if (t < 0.5f)
            {
                float stretch = Mathf.Lerp(0.6f, 1.3f, (t - 0.2f) / 0.3f);
                transform.localScale = new Vector3(1f / stretch, stretch, 1f);
            }
            else
            {
                float squash = Mathf.Lerp(1.3f, 1f, (t - 0.5f) / 0.5f);
                transform.localScale = new Vector3(1f / squash, squash, 1f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    void UpdateFlip()
    {
        if (sr == null) return;
        Vector2 dir = hopTarget - hopStart;
        if (Mathf.Abs(dir.x) > 0.01f)
            sr.flipX = dir.x < 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
