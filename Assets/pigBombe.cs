using System.Collections;
using UnityEngine;

public class pigBombe : MonoBehaviour
{
    public BoxCollider2D boxCollider;
    public GameObject piece;
    public float explosionRadius = 5f;
    public float explosionForce = 10f;

    [Header("Traînée de pièces")]
    public float pieceSpawnInterval = 0.2f;
    public float minMoveSpeed = 0.05f;

    [Header("Animation explosion")]
    public float scaleMult      = 1.5f;   // grossissement max
    public float growDuration   = 0.15f;  // durée pour grossir
    public float shrinkDelay    = 0.08f;  // pause à la taille max
    public float shrinkDuration = 0.1f;   // durée pour rétrécir
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

        // Lance la pulse en boucle dès le spawn
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

        // Flip selon la direction horizontale de déplacement
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
        float riseTime    = 0.07f;  // montée très rapide
        float fallTime    = 0.28f;  // descente plus lente, accélérée
        float peakMult    = 1.25f;  // taille max au sommet
        float squishX     = 1.15f;  // écrasement horizontal à l'atterrissage
        float squishY     = 0.85f;  // écrasement vertical à l'atterrissage
        float squishTime  = 0.05f;
        float recoverTime = 0.07f;

        // Montée rapide avec easing out (démarre vite, ralentit au pic)
        float t = 0f;
        while (t < riseTime)
        {
            if (p == null) yield break;
            float ratio = 1f - Mathf.Pow(1f - t / riseTime, 3f); // ease out cubic
            p.transform.localScale = Vector3.Lerp(baseScale, baseScale * peakMult, ratio);
            t += Time.deltaTime;
            yield return null;
        }

        // Descente avec easing in (accélère en tombant)
        t = 0f;
        while (t < fallTime)
        {
            if (p == null) yield break;
            float ratio = Mathf.Pow(t / fallTime, 2f); // ease in quadratic
            p.transform.localScale = Vector3.Lerp(baseScale * peakMult, baseScale, ratio);
            t += Time.deltaTime;
            yield return null;
        }

        // Écrasement à l'atterrissage
        Vector3 squishScale = new Vector3(baseScale.x * squishX, baseScale.y * squishY, baseScale.z);
        t = 0f;
        while (t < squishTime)
        {
            if (p == null) yield break;
            p.transform.localScale = Vector3.Lerp(baseScale, squishScale, t / squishTime);
            t += Time.deltaTime;
            yield return null;
        }

        // Récupération vers taille normale
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
        if (!collision.CompareTag("Player")&&!collision.CompareTag("Balle")) Explode();
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")&& !collision.collider.CompareTag("Balle")) Explode();
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

        // Camera shake
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.2f, 0.1f);

        // Force sur les objets proches
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D col in colliders)
        {
            Vector2 dir = (col.transform.position - transform.position).normalized;

            // Ennemis : ApplyKnockback via EnemyAI
            EnemyAI ai = col.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.ApplyKnockback(dir, explosionForce);
                continue;
            }

            // Joueur : ApplyKnockback via playerMove
            playerMove pm = col.GetComponent<playerMove>();
            if (pm != null)
            {
                pm.ApplyKnockback(dir, explosionForce);
                continue;
            }

            // Autres rigidbodies (décors, etc.)
            Rigidbody2D crb = col.GetComponent<Rigidbody2D>();
            if (crb != null)
                crb.AddForce(dir * explosionForce, ForceMode2D.Impulse);
        }

        // Spawn pièces explosion
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

        // Animation grossissement + rougissement puis destruction
        StartCoroutine(ExplodeAnim());
    }

    IEnumerator PulseLoop()
    {
        Vector3 targetScale = originalScale * scaleMult;

        while (!hasExploded)
        {
            // Grossit + rougit
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

            // Pause au max
            yield return new WaitForSeconds(shrinkDelay);
            if (hasExploded) yield break;

            // Rétrécit + couleur normale
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

            // Petite pause entre chaque pulse
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator ExplodeAnim()
    {
        Vector3 targetScale = originalScale * scaleMult * 2f;
        float elapsed = 0f;

        // Grossit + rougit
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

        // Pause
        //yield return new WaitForSeconds(shrinkDelay);

       
        // Récupère TOUS les ennemis dans le rayon (frappe en arc)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("AI")) continue;

            AIHealth aiHealth = hit.GetComponent<AIHealth>();
            if (aiHealth != null)
            {
                aiHealth.takeDamage(100);

                // Effets visuels de combat
                if (CombatEffects.Instance != null)
                    CombatEffects.Instance.OnPlayerHitEnemy(hit.gameObject, 100, hit.transform.position);

                //hitAnything = true;
            }
            else
            {
                //Debug.LogError("AIHealth introuvable sur " + hit.gameObject.name);
            }
        }
        Destroy(gameObject);
    }
}
