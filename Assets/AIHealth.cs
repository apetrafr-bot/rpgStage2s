using System.Collections;
using UnityEngine;

public class AIHealth : MonoBehaviour
{
    [Header("Sante")]
    public float maxHealth;
    public float currentHealth;
    public AIDrop drop;

    [Header("Audio")]
    public AudioClip damageSound;
    [Range(0f, 1f)] public float damageSoundVolume = 1f;

    // Barre de vie flottante (optionnel)
    [Header("Barre de vie (optionnel)")]
    public SpriteRenderer healthBarFill;   // sprite blanc mis à l'échelle en X
    public Color healthBarColor = new Color(0.2f, 0.9f, 0.2f);
    public Color healthBarLowColor = new Color(0.9f, 0.2f, 0.2f);

    private SpriteRenderer _sr;
    private Coroutine _deathCoroutine;
    private bossCochon _bossCochon;

    public void Start()
    {
        currentHealth = maxHealth;
        _sr = GetComponent<SpriteRenderer>();
        _bossCochon = GetComponent<bossCochon>();
        UpdateHealthBar();
    }

    public void takeDamage(float damage)
    {

        currentHealth -= damage;
        UpdateHealthBar();

        // Le boss gère sa propre mort — ne pas le détruire ici
        if (_bossCochon != null) return;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Drops
            if (drop != null)
            {
                foreach (var item in drop.AIDropList)
                    Instantiate(item.tilePrefab, transform.position, Quaternion.identity);
            }
            // Animation de mort avant destruction
            if (_deathCoroutine == null)
                _deathCoroutine = StartCoroutine(DeathAnimation());
        }
    }

    // -------------------------------------------------------
    //  Barre de vie dynamique
    // -------------------------------------------------------
    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;
        float ratio = Mathf.Clamp01(currentHealth / maxHealth);
        // On scale le fill en X pour simuler la vidange
        Vector3 s = healthBarFill.transform.localScale;
        s.x = ratio;
        healthBarFill.transform.localScale = s;
        healthBarFill.color = Color.Lerp(healthBarLowColor, healthBarColor, ratio);
    }

    // -------------------------------------------------------
    //  Animation de mort : scale + fade out
    // -------------------------------------------------------
    IEnumerator DeathAnimation()
    {
        // Désactive l'IA pour éviter qu'elle continue d'agir
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        float duration = 0.35f;
        float elapsed  = 0f;
        Vector3 startScale = transform.localScale;
        Color startColor   = _sr != null ? _sr.color : Color.white;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Grossit légèrement puis rétrécit et disparaît
            float scaleFactor = Mathf.Lerp(1f, 0f, t);
            float punchUp     = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f; // petit rebond
            transform.localScale = startScale * scaleFactor * punchUp;

            if (_sr != null)
            {
                Color c = startColor;
                c.a = 1f - t;
                _sr.color = c;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
