using System.Collections;
using UnityEngine;

public class bombeJoueur : MonoBehaviour
{
    public BoxCollider2D boxCollider;
    public GameObject piece;
    public float explosionRadius = 5f;
    public float explosionForce = 10f;
    public int damage = 1;

    [Header("Traînée de pièces")]
    public float pieceSpawnInterval = 0.2f;
    public float minMoveSpeed = 0.05f;

    [Header("Animation explosion")]
    public float scaleMult      = 1.5f;
    public float growDuration   = 0.15f;
    public float shrinkDelay    = 0.08f;
    public float shrinkDuration = 0.1f;
    public Color flashColor     = new Color(1f, 0.15f, 0.15f, 1f);

    private bool hasExploded  = false;
    private bool timerStarted = false;
    private float pieceTimer  = 0f;

    private Rigidbody2D    rb;
    private Vector2        lastPosition;
    private SpriteRenderer sr;
    private Vector3        originalScale;
    private Color          originalColor;

    void Start()
    {
        rb            = GetComponent<Rigidbody2D>();
        sr            = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
        originalColor = sr != null ? sr.color : Color.white;
        lastPosition  = rb != null ? rb.position : (Vector2)transform.position;

        StartCoroutine(PulseLoop());
    }

    void Update()
    {
        if (!timerStarted)
        {
            timerStarted = true;
            StartCoroutine(ExplodeAfterDelay(3f));
        }
    }

    void FixedUpdate()
    {
        if (hasExploded || piece == null) return;

        Vector2 currentPos = rb != null ? rb.position : (Vector2)transform.position;
        float moved = Vector2.Distance(currentPos, lastPosition);

        if (sr != null && rb != null)
        {
            if (rb.linearVelocity.x < -0.01f) sr.flipX = false;
            else if (rb.linearVelocity.x > 0.01f) sr.flipX = true;
        }

        lastPosition = currentPos;

        if (moved >= minMoveSpeed)
        {
            pieceTimer -= Time.fixedDeltaTime;
            if (pieceTimer <= 0f)
            {
                SpawnFallingPiece(transform.position);
                pieceTimer = pieceSpawnInterval;
            }
        }
    }

    void SpawnFallingPiece(Vector2 spawnPos)
    {
        GameObject p = Instantiate(piece, spawnPos, Quaternion.identity);
        Rigidbody2D prb = p.GetComponent<Rigidbody2D>();
        if (prb == null) prb = p.AddComponent<Rigidbody2D>();
        prb.gravityScale = 0f;
        prb.linearDamping = 4f;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float speed = Random.Range(1f, 3f);
        prb.linearVelocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
        StartCoroutine(ArcScale(p));
    }

    IEnumerator ArcScale(GameObject p)
    {
        if (p == null) yield break;

        Vector3 baseScale = p.transform.localScale;
        float riseTime    = 0.07f;
        float fallTime    = 0.28f;
        float peakMult    = 1.25f;
        float squishX     = 1.15f;
        float squishY     = 0.85f;
        float squishTime  = 0.05f;
        float recoverTime = 0.07f;

        float t = 0f;
        while (t < riseTime)
        {
            if (p == null) yield break;
            float ratio = 1f - Mathf.Pow(1f - t / riseTime, 3f);
            p.transform.localScale = Vector3.Lerp(baseScale, baseScale * peakMult, ratio);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < fallTime)
        {
            if (p == null) yield break;
            float ratio = Mathf.Pow(t / fallTime, 2f);
            p.transform.localScale = Vector3.Lerp(baseScale * peakMult, baseScale, ratio);
            t += Time.deltaTime;
            yield return null;
        }

        Vector3 squishScale = new Vector3(baseScale.x * squishX, baseScale.y * squishY, baseScale.z);
        t = 0f;
        while (t < squishTime)
        {
            if (p == null) yield break;
            p.transform.localScale = Vector3.Lerp(baseScale, squishScale, t / squishTime);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < recoverTime)
        {
            if (p == null) yield break;
            p.transform.localScale = Vector3.Lerp(squishScale, baseScale, t / recoverTime);
            t += Time.deltaTime;
            yield return null;
        }

        if (p != null) p.transform.localScale = baseScale;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") && !collision.CompareTag("Balle")) Explode();
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player") && !collision.collider.CompareTag("Balle")) Explode();
    }

    IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.2f, 0.1f);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D col in colliders)
        {
            Vector2 dir = (col.transform.position - transform.position).normalized;

            EnemyAI ai = col.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.ApplyKnockback(dir, explosionForce);
                continue;
            }

            playerMove pm = col.GetComponent<playerMove>();
            if (pm != null)
            {
                pm.ApplyKnockback(dir, explosionForce);
                continue;
            }

            Rigidbody2D crb = col.GetComponent<Rigidbody2D>();
            if (crb != null)
                crb.AddForce(dir * explosionForce, ForceMode2D.Impulse);
        }

        if (piece != null)
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject p = Instantiate(piece, (Vector2)transform.position, Quaternion.identity);
                Rigidbody2D prb = p.GetComponent<Rigidbody2D>();
                if (prb == null) prb = p.AddComponent<Rigidbody2D>();
                prb.gravityScale = 0f;
                prb.linearDamping = 4f;
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float speed = Random.Range(2f, 5f);
                prb.linearVelocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                StartCoroutine(ArcScale(p));
            }
        }

        StartCoroutine(ExplodeAnim());
    }

    IEnumerator PulseLoop()
    {
        Vector3 targetScale = originalScale * scaleMult;

        while (!hasExploded)
        {
            float elapsed = 0f;
            while (elapsed < growDuration && !hasExploded)
            {
                float t = elapsed / growDuration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                if (sr != null) sr.color = Color.Lerp(originalColor, flashColor, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (hasExploded) yield break;
            transform.localScale = targetScale;
            if (sr != null) sr.color = flashColor;

            yield return new WaitForSeconds(shrinkDelay);
            if (hasExploded) yield break;

            elapsed = 0f;
            while (elapsed < shrinkDuration && !hasExploded)
            {
                float t = elapsed / shrinkDuration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                if (sr != null) sr.color = Color.Lerp(flashColor, originalColor, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (hasExploded) yield break;
            transform.localScale = originalScale;
            if (sr != null) sr.color = originalColor;

            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator ExplodeAnim()
    {
        Vector3 targetScale = originalScale * scaleMult * 2f;
        float elapsed = 0f;

        while (elapsed < growDuration)
        {
            float t = elapsed / growDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            if (sr != null) sr.color = Color.Lerp(originalColor, flashColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
        if (sr != null) sr.color = flashColor;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            playerHealth health = hit.GetComponent<playerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);

                if (CombatEffects.Instance != null)
                    CombatEffects.Instance.OnPlayerTakeDamage(hit.gameObject, damage, hit.transform.position);
            }
        }

        Destroy(gameObject);
    }
}
