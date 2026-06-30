using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Attache ce script sur le prefab du popup de dégâts (qui a un TextMeshPro).
/// Le popup monte et disparaît en fondu.
/// Pas de TextMeshPro ? Pas de problème — CombatEffects ne l'instanciera pas.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [Header("Mouvement")]
    public float floatSpeed  = 1.2f;
    public float lifetime    = 0.8f;
    public float scaleEffect = 1.4f; // scale initial (rebondit vers 1)

    private TextMeshPro _tmp;
    private float _elapsed;
    private Color _baseColor;

    void Awake()
    {
        _tmp = GetComponentInChildren<TextMeshPro>();
    }

    public void Init(int amount, Color color)
    {
        if (_tmp == null) return;
        _tmp.text  = "-" + amount.ToString();
        _tmp.color = color;
        _baseColor = color;
        transform.localScale = Vector3.one * scaleEffect;
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        _elapsed = 0f;
        while (_elapsed < lifetime)
        {
            float t = _elapsed / lifetime;

            // Monte vers le haut
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // Scale rebondit de scaleEffect -> 1 rapidement, puis reste à 1
            float s = Mathf.Lerp(scaleEffect, 1f, Mathf.Min(t * 3f, 1f));
            transform.localScale = Vector3.one * s;

            // Fondu en fin de vie
            if (_tmp != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, Mathf.Max(0f, t - 0.5f) / 0.5f);
                Color c = _baseColor;
                c.a = alpha;
                _tmp.color = c;
            }

            _elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
