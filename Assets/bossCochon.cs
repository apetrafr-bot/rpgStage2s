using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class bossCochon : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip bossJumpSound;
    public float bossJumpSoundDuration;
    [Range(0f, 1f)] public float bossJumpSoundVolume = 1f;
    public AudioClip bossChargeSound;
    public float bossChargeSoundDuration;
    [Range(0f, 1f)] public float bossChargeSoundVolume = 1f;
    public AudioClip bossDeathSound;
    public float bossDeathSoundDuration;
    [Range(0f, 1f)] public float bossDeathSoundVolume = 1f;
    public AudioClip bossBombSound;
    public float bossBombSoundDuration;
    [Range(0f, 1f)] public float bossBombSoundVolume = 1f;

    [Header("Detection")]
    public float detectionRange = 10f;

    [Header("Saut")]
    public float jumpCooldown = 1.2f;
    public float jumpRange = 3f;

    [Header("Charge")]
    public float chargeSpeed = 10f;
    public float chargeCooldown = 3f;
    public float chargeRange = 7f;

    [Header("Bombes")]
    public GameObject bombePrefab;
    public float bombeCooldown = 5f;
    public int bombesParSalve = 4;
    public float bombeForce = 6f;

    [Header("Butin")]
    public GameObject lootContainer;
    public GameObject portalZelda;
    public float lootSpread = 1f;

    [Header("Barre de vie UI")]
    public Image barreVieFill;
    public GameObject barreVieContainer;

    private enum State { Idle, Jump, Charge }
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;
    private AIHealth aiHealth;
    private playerHealth _playerHealth;
    private bool isDead = false;
    private Collider2D[] _playerCheckHits = new Collider2D[10];

    private float stateCooldown = 0f;
    private bool isJumping = false;
    private bool isCharging = false;
    private Vector2 jumpStart;
    private Vector2 jumpTarget;
    private float jumpTimer = 0f;
    private float jumpDuration = 0.35f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        sr = GetComponent<SpriteRenderer>();
        aiHealth = GetComponent<AIHealth>();
    }

    private bool playerScaled = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            _playerHealth = playerObj.GetComponent<playerHealth>();
        }

        if (barreVieFill == null && GameManager.Instance != null)
            barreVieFill = GameManager.Instance.bossBarreVieFill;
        if (barreVieContainer == null && GameManager.Instance != null)
            barreVieContainer = GameManager.Instance.bossBarreVieContainer;

        if (barreVieContainer != null)
            barreVieContainer.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        if (aiHealth != null && !isDead)
        {
            MettreAJourBarreVie();
            if (aiHealth.currentHealth <= 0)
            {
                Mort();
                return;
            }
        }

        float dist = Vector2.Distance(transform.position, player.position);

        bool inRange = dist <= detectionRange;
        if (barreVieContainer != null)
            barreVieContainer.SetActive(inRange);

        if (inRange && !playerScaled)
        {
            playerScaled = true;
            player.localScale = player.localScale / 2f;
            Transform cam = player.GetComponentInChildren<Camera>().transform;
            cam.localScale = new Vector3(
                cam.localScale.x * 2f,
                cam.localScale.y * 2f,
                cam.localScale.z);
        }

        if (isJumping || isCharging) return;

        stateCooldown -= Time.deltaTime;
        if (stateCooldown > 0f) return;

        if (!inRange) return;

        if (bombePrefab != null && Random.value < 0.2f)
            StartCoroutine(LancerBombes());
        else if (dist <= chargeRange && Random.value < 0.4f)
            StartCharge();
        else
            StartJump();
    }

    private ContactFilter2D contactFilter;

    void FixedUpdate()
    {
        if (contactFilter.useTriggers == false)
        {
            contactFilter = new ContactFilter2D();
            contactFilter.useTriggers = true;
            contactFilter.useLayerMask = false;
        }

        if (isJumping)
            UpdateJump();
        else if (isCharging)
            UpdateCharge();
    }

    void StartJump()
    {
        if (player == null) return;
        isJumping = true;
        jumpStart = rb.position;

        

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        float dist = Vector2.Distance(rb.position, player.position);
        float hopDist = Mathf.Min(dist, jumpRange);

        jumpTarget = rb.position + dir * hopDist;
        jumpTimer = 0f;
        stateCooldown = jumpCooldown;

        if (sr != null)
        {
            sr.flipX = dir.x > 0f;
            StartCoroutine(SquashAndStretch());
        }
    }

    void UpdateJump()
    {
        jumpTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);

        rb.MovePosition(Vector2.Lerp(jumpStart, jumpTarget, t));

        if (t >= 1f)
        {
            isJumping = false;

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.3f, 0.4f);

            if (PlayerDansRayon(1.5f))
            {
                if (_playerHealth != null)
                    _playerHealth.TakeDamage(999);
            }
        }
    }

    void StartCharge()
    {
        if (player == null) return;
        isCharging = true;

        

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        if (sr != null)
            sr.flipX = dir.x > 0f;

        stateCooldown = chargeCooldown;
    }

    void UpdateCharge()
    {
        if (player == null) { isCharging = false; return; }

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * chargeSpeed * Time.fixedDeltaTime);

        if (PlayerDansRayon(1.5f))
        {
            if (_playerHealth != null)
                _playerHealth.TakeDamage(999);

            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.3f, 0.4f);

            isCharging = false;
        }

        float distToPlayer = Vector2.Distance(rb.position, player.position);
        if (distToPlayer > chargeRange * 1.5f)
            isCharging = false;
    }

    bool PlayerDansRayon(float rayon)
    {
        int count = Physics2D.OverlapCircle(transform.position, rayon, contactFilter, _playerCheckHits);
        for (int i = 0; i < count; i++)
        {
            if (_playerCheckHits[i] != null && _playerCheckHits[i].CompareTag("Player"))
                return true;
        }
        return false;
    }

    IEnumerator LancerBombes()
    {
        stateCooldown = bombeCooldown;

        for (int i = 0; i < bombesParSalve; i++)
        {
            if (bombePrefab == null) yield break;

            

            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 1f;
            GameObject bombe = Instantiate(bombePrefab, pos, Quaternion.identity);

            Vector2 dir = Random.insideUnitCircle.normalized;
            Rigidbody2D rbBombe = bombe.GetComponent<Rigidbody2D>();
            if (rbBombe != null)
                rbBombe.linearVelocity = dir * bombeForce;

            yield return new WaitForSeconds(0.3f);
        }
    }

    void MettreAJourBarreVie()
    {
        if (barreVieFill == null || aiHealth == null) return;
        barreVieFill.fillAmount = aiHealth.currentHealth / aiHealth.maxHealth;
    }

    void Mort()
    {
        if (isDead) return;
        isDead = true;

        if (barreVieContainer != null)
            barreVieContainer.SetActive(false);

        player.localScale = Vector3.one;
        Transform cam = player.GetComponentInChildren<Camera>().transform;
        cam.localScale = Vector3.one;

        this.enabled = false;
        rb.linearVelocity = Vector2.zero;

        

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.5f, 0.6f);

        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Color startColor = sr != null ? sr.color : Color.white;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.localScale = startScale * Mathf.Lerp(1f, 0f, t);
            if (sr != null)
            {
                Color c = startColor;
                c.a = 1f - t;
                sr.color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (portalZelda != null)
            portalZelda.SetActive(true);

        if (aiHealth != null && aiHealth.drop != null && aiHealth.drop.AIDropList.Count > 0)
        {
            if (lootContainer != null)
            {
                GameObject container = Instantiate(lootContainer, transform.position, Quaternion.identity);
                container.name = "BossLoot";
            }

            float angleStep = 360f / aiHealth.drop.AIDropList.Count;
            for (int i = 0; i < aiHealth.drop.AIDropList.Count; i++)
            {
                TileClass item = aiHealth.drop.AIDropList[i];
                if (item == null || item.tilePrefab == null) continue;

                Vector3 offset = Random.insideUnitCircle * lootSpread;
                Instantiate(item.tilePrefab, transform.position + offset, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    IEnumerator SquashAndStretch()
    {
        float duration = jumpDuration;
        float elapsed = 0f;
        Vector3 baseScale = transform.localScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            if (t < 0.2f)
            {
                float squash = Mathf.Lerp(1f, 0.6f, t / 0.2f);
                transform.localScale = new Vector3(baseScale.x / squash, baseScale.y * squash, 1f);
            }
            else if (t < 0.5f)
            {
                float stretch = Mathf.Lerp(0.6f, 1.3f, (t - 0.2f) / 0.3f);
                transform.localScale = new Vector3(baseScale.x / stretch, baseScale.y * stretch, 1f);
            }
            else
            {
                float squash = Mathf.Lerp(1.3f, 1f, (t - 0.5f) / 0.5f);
                transform.localScale = new Vector3(baseScale.x / squash, baseScale.y * squash, 1f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = baseScale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, chargeRange);
    }
}
