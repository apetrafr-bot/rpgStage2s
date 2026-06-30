using UnityEngine;
using System.Collections.Generic;

public class BoomerangProjectile : MonoBehaviour
{
    public int damage = 1;
    public float speed = 10f;
    public float maxDistance = 5f;
    public float returnSpeed = 15f;
    public float rotateSpeed = 720f;

    private Vector2 startPos;
    private Vector2 flyDir;
    private Transform player;
    private bool returning = false;
    private HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();

    void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Collider2D[] cols = GetComponents<Collider2D>();
        foreach (Collider2D c in cols)
            c.isTrigger = true;

        startPos = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Start()
    {
        flyDir = transform.right;
    }

    void Update()
    {
        if (player == null) { Destroy(gameObject); return; }

        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        float dist = Vector2.Distance(transform.position, startPos);

        if (!returning && dist >= maxDistance)
        {
            returning = true;
            alreadyHit.Clear();
        }

        if (returning)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, returnSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, player.position) < 0.3f)
                Destroy(gameObject);
        }
        else
        {
            transform.position += (Vector3)flyDir * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("AI") && !alreadyHit.Contains(other))
        {
            alreadyHit.Add(other);

            AIHealth ai = other.GetComponent<AIHealth>();
            if (ai != null)
            {
                ai.takeDamage(damage);
                if (CombatEffects.Instance != null)
                    CombatEffects.Instance.OnPlayerHitEnemy(other.gameObject, damage, other.transform.position);
            }

        }
    }
}
