using System.Collections;
using UnityEngine;

/// <summary>
/// Fait grossir et rougir le cochon au moment de l'explosion,
/// puis le ramène à son état normal.
/// Attache ce script sur le même GameObject que pigBombe.
/// </summary>
public class modifPig : MonoBehaviour
{
    [Header("Grossissement")]
    public float scaleMult     = 1.4f;   // taille maximale (x1.4)
    public float growDuration  = 0.15f;  // secondes pour grossir
    public float shrinkDelay   = 0.1f;   // pause à la taille max avant de rétrécir
    public float shrinkDuration= 0.1f;   // secondes pour revenir à la normale

    [Header("Rougissement")]
    public Color flashColor    = new Color(1f, 0.2f, 0.2f, 1f);  // rouge vif

    private Vector3       originalScale;
    private Color         originalColor;
    private SpriteRenderer sr;
    private bool          isPlaying = false;

    void Awake()
    {
        sr            = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
        if (sr != null) originalColor = sr.color;
    }

    /// <summary>Appelle cette méthode depuis pigBombe.Explode() juste avant Destroy.</summary>
    public void PlayExplodeEffect()
    {
        if (isPlaying) return;
        StartCoroutine(ExplodeEffect());
    }

    private IEnumerator ExplodeEffect()
    {
        isPlaying = true;

        // --- Grossit + rougit ---
        float elapsed = 0f;
        Vector3 targetScale = originalScale * scaleMult;

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

        // --- Pause à la taille max ---
        yield return new WaitForSeconds(shrinkDelay);

        // --- Rétrécit + couleur normale ---
        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            float t = elapsed / shrinkDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            if (sr != null) sr.color = Color.Lerp(flashColor, originalColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
        if (sr != null) sr.color = originalColor;

        isPlaying = false;
    }
}
