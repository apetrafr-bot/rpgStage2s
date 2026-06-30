using UnityEngine;

public class Monture : MonoBehaviour
{
    [Header("Mouvement")]
    public float speed = 8f;

    [Header("Visuel")]
    public GameObject montureVisuel;
    public GameObject joueurMonteVisuel;

    [Header("Interaction")]
    public float range = 2f;

    private Transform joueur;
    private SpriteRenderer joueurSR;
    private playerMove _playerMove;
    private SpriteRenderer montureSR;
    private SpriteRenderer montureVisuelSR;
    private SpriteRenderer monteSR;
    private BoxCollider2D _collider;
    private float baseSpeed;
    private bool monte = false;
    public bool IsMonte() => monte;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            joueur = p.transform;
            joueurSR = p.GetComponent<SpriteRenderer>();
            _playerMove = p.GetComponent<playerMove>();
            baseSpeed = _playerMove != null ? _playerMove.speed : 5f;
        }
        montureSR = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
        if (montureVisuel != null)
            montureVisuelSR = montureVisuel.GetComponent<SpriteRenderer>();

        Debug.Log($"[Monture] Start sur {gameObject.name} — joueur={(joueur != null ? joueur.name : "null")}, montureVisuel={(montureVisuel != null ? montureVisuel.name : "null")}, joueurMonteVisuel={(joueurMonteVisuel != null ? joueurMonteVisuel.name : "null")}, monte={monte}");
    }

    void Update()
    {
        if (joueur == null) return;

        if (monte)
        {
            if (playerHealth.Instance != null && playerHealth.Instance.IsDead)
            {
                Descendre();
                return;
            }

            transform.position = joueur.position;
            if (joueurSR != null && montureSR != null)
                montureSR.flipX = joueurSR.flipX;
            if (joueurSR != null && monteSR != null)
                monteSR.flipX = !joueurSR.flipX;

            if (Input.GetKeyDown(KeyCode.E))
                Descendre();
            return;
        }

        float dist = Vector2.Distance(transform.position, joueur.position);
        if (Input.GetKeyDown(KeyCode.E) && dist <= range)
        {
            Debug.Log($"[Monture] Touche E détectée, dist={dist}, range={range} → appel Monter()");
            Monter();
        }
    }

    void Monter()
    {
        Debug.Log($"[Monture] Monter() appelé — joueurMonteVisuel={(joueurMonteVisuel != null ? joueurMonteVisuel.name : "null")}, montureSR={(montureSR != null ? "ok" : "null")}");

        if (_playerMove != null)
            _playerMove.speed = speed;

        if (montureSR != null)
        {
            montureSR.enabled = false;
            Debug.Log("[Monture] montureSR désactivé");
        }
        if (_collider != null)
        {
            _collider.enabled = false;
            Debug.Log("[Monture] collider désactivé");
        }

        if (joueurMonteVisuel != null)
        {
            joueurMonteVisuel.SetActive(true);
            monteSR = joueurMonteVisuel.GetComponent<SpriteRenderer>();
            Debug.Log($"[Monture] joueurMonteVisuel activé, monteSR={(monteSR != null ? "ok" : "null")}");
        }
        else
        {
            Debug.LogWarning("[Monture] joueurMonteVisuel est NULL ! Le visuel monté ne s'affichera pas.");
        }

        monte = true;
        Debug.Log("[Monture] monte = true");
    }

    void Descendre()
    {
        Debug.Log("[Monture] Descendre() appelé");

        if (_playerMove != null)
            _playerMove.speed = baseSpeed;

        if (montureSR != null) montureSR.enabled = true;
        if (_collider != null) _collider.enabled = true;

        if (joueurMonteVisuel != null)
        {
            joueurMonteVisuel.SetActive(false);
            Debug.Log("[Monture] joueurMonteVisuel désactivé");
        }

        if (joueur != null)
        {
            Vector3 dir = Vector3.right;
            joueur.position = transform.position + dir;
        }

        monte = false;
        Debug.Log("[Monture] monte = false");
    }

    void OnDestroy()
    {
        if (monte && _playerMove != null)
            _playerMove.speed = baseSpeed;

        if (montureSR != null) montureSR.enabled = true;
        if (_collider != null) _collider.enabled = true;

        if (joueurMonteVisuel != null)
            joueurMonteVisuel.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
