using UnityEngine;

/// <summary>
/// Gere les pics du donjon.
/// Deux sprites : un avec pics actifs (font des degats), un sans (inoffensif).
/// Applique des degats au joueur (playerHealth) et aux IA (AIHealth) au contact.
/// </summary>
public class Picmanager : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("Sprite quand les pics sont sortis (font des degats).")]
    public Sprite spritePicsActifs;

    [Tooltip("Sprite quand les pics sont rentres (inoffensifs).")]
    public Sprite spritePicsInactifs;

    [Header("Degats")]
    [Tooltip("Degats infliges au joueur et aux IA.")]
    public int degats = 10;

    [Tooltip("Temps en secondes entre chaque application de degats.")]
    public float intervalDegats = 1f;

    [Header("Cycle")]
    [Tooltip("Duree en secondes ou les pics sont actifs.")]
    public float dureeActive = 2f;

    [Tooltip("Duree en secondes ou les pics sont inactifs.")]
    public float dureeInactive = 2f;

    private SpriteRenderer sr;
    private bool picsActifs = true;
    private float timerCycle = 0f;
    private float timerDegats = 0f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        AppliquerSprite();
    }

    private void Update()
    {
        // Cycle actif / inactif
        timerCycle += Time.deltaTime;
        float dureeCourante = picsActifs ? dureeActive : dureeInactive;

        if (timerCycle >= dureeCourante)
        {
            timerCycle = 0f;
            picsActifs = !picsActifs;
            AppliquerSprite();
        }

        // Cooldown degats
        if (timerDegats > 0f)
            timerDegats -= Time.deltaTime;
    }

    private void AppliquerSprite()
    {
        if (sr == null) return;
        sr.sprite = picsActifs ? spritePicsActifs : spritePicsInactifs;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!picsActifs) return;
        AppliquerDegats(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!picsActifs) return;
        if (timerDegats > 0f) return;

        AppliquerDegats(other);
    }

    private void AppliquerDegats(Collider2D other)
    {
        // Joueur
        playerHealth ph = other.GetComponent<playerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(degats);
            timerDegats = intervalDegats;
            return;
        }

        // IA
        AIHealth ai = other.GetComponent<AIHealth>();
        if (ai != null)
        {
            ai.takeDamage(degats);
            timerDegats = intervalDegats;
        }
    }
}
