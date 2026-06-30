using UnityEngine;

public class MontureMovement : MonoBehaviour
{
    public float wanderRadius = 5f;
    public float moveSpeed = 2f;
    public float waitTime = 2f;

    private Monture monture;
    private SpriteRenderer sr;
    private Vector2 target;
    private float timer;
    private bool isWaiting;

    void Start()
    {
        monture = GetComponent<Monture>();
        sr = GetComponent<SpriteRenderer>();
        NewTarget();
    }

    void Update()
    {
        if (monture != null && monture.enabled && monture.IsMonte())
            return;

        if (isWaiting)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                NewTarget();
                isWaiting = false;
            }
            return;
        }

        Vector2 pos = transform.position;
        float dist = Vector2.Distance(pos, target);

        if (dist < 0.3f)
        {
            timer = waitTime;
            isWaiting = true;
            return;
        }

        Vector2 dir = (target - pos).normalized;
        transform.position = Vector2.MoveTowards(pos, target, moveSpeed * Time.deltaTime);

        if (sr != null)
            sr.flipX = dir.x < 0;
    }

    void NewTarget()
    {
        target = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
    }
}
