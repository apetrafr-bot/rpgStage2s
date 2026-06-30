using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Singleton — place ce script sur un GameObject "CombatEffects" dans la scène.
/// Il gère : screen shake, hit-flash ennemi, hit-flash joueur, popup de dégâts,
/// freeze frame, et vignette d'impact.
/// </summary>
public class CombatEffects : MonoBehaviour
{
    public static CombatEffects Instance { get; private set; }

    // -------------------------------------------------------
    //  Screen Shake
    // -------------------------------------------------------
    [Header("Screen Shake")]
    public Camera cam;
    [Tooltip("Durée du tremblement quand le joueur frappe")]
    public float shakeDurationHit    = 0.18f;
    [Tooltip("Magnitude du tremblement quand le joueur frappe")]
    public float shakeMagnitudeHit   = 0.12f;
    [Tooltip("Durée du tremblement quand le joueur reçoit un coup")]
    public float shakeDurationDamage = 0.25f;
    [Tooltip("Magnitude du tremblement quand le joueur reçoit un coup")]
    public float shakeMagnitudeDamage = 0.2f;

    private Vector3 _camOriginalPos;
    private Coroutine _shakeCoroutine;

    // -------------------------------------------------------
    //  Freeze Frame (micro-pause à l'impact)
    // -------------------------------------------------------
    [Header("Freeze Frame")]
    [Tooltip("Durée de la micro-pause à l'impact (secondes)")]
    public float freezeDuration = 0.06f;

    // -------------------------------------------------------
    //  Hit Flash ennemi
    // -------------------------------------------------------
    [Header("Hit Flash ennemi")]
    public Color enemyFlashColor = Color.white;
    public float enemyFlashDuration = 0.12f;

    // -------------------------------------------------------
    //  Hit Flash joueur (rouge)
    // -------------------------------------------------------
    [Header("Hit Flash joueur")]
    public Color playerFlashColor = new Color(1f, 0.2f, 0.2f, 0.8f);
    public float playerFlashDuration = 0.2f;

    // -------------------------------------------------------
    //  Popup texte dégâts
    // -------------------------------------------------------
    [Header("Popup dégâts")]
    [Tooltip("Prefab TextMeshPro flottant (optionnel)")]
    public GameObject damagePopupPrefab;

    // -------------------------------------------------------
    //  Particules de sang / étincelles
    // -------------------------------------------------------
    [Header("Particules d'impact")]
    [Tooltip("Prefab ParticleSystem pour l'impact sur l'ennemi (optionnel)")]
    public GameObject hitParticlePrefab;
    [Tooltip("Prefab ParticleSystem pour l'impact sur le joueur (optionnel)")]
    public GameObject playerHitParticlePrefab;

    // -------------------------------------------------------
    //  Vignette URP (optionnel)
    // -------------------------------------------------------
    [Header("Vignette URP (optionnel)")]
    public Volume globalVolume;
    private Vignette _vignette;
    private Coroutine _vignetteCoroutine;

    // -------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (cam == null) cam = Camera.main;
        if (cam != null) _camOriginalPos = cam.transform.localPosition;

        if (globalVolume != null)
            globalVolume.profile.TryGet(out _vignette);
    }

    // ================================================================
    //  API publique
    // ================================================================

    /// <summary>Appelé quand le JOUEUR frappe un ennemi.</summary>
    public void OnPlayerHitEnemy(GameObject enemy, float damage, Vector3 hitPos)
    {
        // 1. Freeze frame
        StartCoroutine(FreezeFrame());

        // 2. Screen shake
        DoShake(shakeDurationHit, shakeMagnitudeHit);

        // 3. Flash blanc sur l'ennemi
        StartCoroutine(FlashSprite(enemy, enemyFlashColor, enemyFlashDuration));

        // 4. Knockback ennemi
        StartCoroutine(KnockbackEnemy(enemy, hitPos, 3.5f, 0.1f));

        // 5. Particules d'impact
        SpawnParticles(hitParticlePrefab, hitPos);

        // 6. Popup dégâts
        SpawnDamagePopup(hitPos, (int)damage, Color.yellow);
    }

    /// <summary>Appelé quand le JOUEUR reçoit des dégâts.</summary>
    public void OnPlayerTakeDamage(GameObject player, int damage, Vector3 hitPos)
    {
        // 1. Screen shake plus fort
        DoShake(shakeDurationDamage, shakeMagnitudeDamage);

        // 2. Flash rouge sur le joueur
        StartCoroutine(FlashSprite(player, playerFlashColor, playerFlashDuration));

        // 3. Vignette rouge URP
        if (_vignette != null)
        {
            if (_vignetteCoroutine != null) StopCoroutine(_vignetteCoroutine);
            _vignetteCoroutine = StartCoroutine(PulseVignette(0.55f, 0.3f));
        }

        // 4. Particules
        SpawnParticles(playerHitParticlePrefab, hitPos);

        // 5. Popup dégâts rouge
        SpawnDamagePopup(hitPos, damage, new Color(1f, 0.3f, 0.3f));
    }

    // ================================================================
    //  Implémentations internes
    // ================================================================

    void DoShake(float duration, float magnitude)
    {
        if (cam == null) return;
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Magnitude décroissante
            float m = Mathf.Lerp(magnitude, 0f, t);
            cam.transform.localPosition = _camOriginalPos + (Vector3)Random.insideUnitCircle * m;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cam.transform.localPosition = _camOriginalPos;
    }

    IEnumerator FreezeFrame()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(freezeDuration);
        Time.timeScale = 1f;
    }

    IEnumerator FlashSprite(GameObject target, Color flashColor, float duration)
    {
        if (target == null) yield break;

        if (!target.TryGetComponent(out SpriteRenderer sr)) yield break;

        Color original = sr.color;
        sr.color = flashColor;
        yield return new WaitForSecondsRealtime(duration);
        if (sr != null) sr.color = original;
    }

    IEnumerator KnockbackEnemy(GameObject enemy, Vector3 hitOrigin, float force, float duration)
    {
        if (enemy == null) yield break;
        if (!enemy.TryGetComponent(out Rigidbody2D rb)) yield break;

        Vector2 dir = ((Vector2)(enemy.transform.position - hitOrigin)).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    IEnumerator PulseVignette(float targetIntensity, float duration)
    {
        if (_vignette == null) yield break;

        float start = _vignette.intensity.value;
        float half  = duration * 0.5f;

        for (float t = 0; t < half; t += Time.unscaledDeltaTime)
        {
            _vignette.intensity.Override(Mathf.Lerp(start, targetIntensity, t / half));
            yield return null;
        }
        for (float t = 0; t < half; t += Time.unscaledDeltaTime)
        {
            _vignette.intensity.Override(Mathf.Lerp(targetIntensity, start, t / half));
            yield return null;
        }
        _vignette.intensity.Override(start);
    }

    void SpawnParticles(GameObject prefab, Vector3 pos)
    {
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            Destroy(go, ps.main.duration + ps.main.startLifetime.constantMax + 0.5f);
        }
        else
        {
            Destroy(go, 2f);
        }
    }

    void SpawnDamagePopup(Vector3 worldPos, int amount, Color color)
    {
        if (damagePopupPrefab == null) return;
        Vector3 spawnPos = worldPos + new Vector3(Random.Range(-0.2f, 0.2f), 0.4f, 0f);
        GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
        DamagePopup dp = popup.GetComponent<DamagePopup>();
        if (dp != null) dp.Init(amount, color);
        else Destroy(popup, 1f);
    }
}
