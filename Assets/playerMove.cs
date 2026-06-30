using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class playerMove : MonoBehaviour
{
    public float speed = 5f;
    private float speedBuff = 0f;
    public float SpeedTotal => speed + speedBuff;

    public Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer sr;
    public Animator animator;

    // Knockback
    private Vector2 knockbackVelocity = Vector2.zero;
    public float knockbackDamping = 8f;  // plus élevé = recul qui s'arrête plus vite

    [Header("Footsteps")]
    public AudioClip footstepSound;
    public float footstepInterval = 0.4f;
    public float footstepDuration = 0.15f;
    private float footstepTimer = 0f;
    private bool footstepsEnabled = true;

    public void ApplySpeedBuff(float boost, float duration)
    {
        speedBuff = boost;
        CancelInvoke(nameof(RemoveSpeedBuff));
        Invoke(nameof(RemoveSpeedBuff), duration);
    }

    private void RemoveSpeedBuff()
    {
        speedBuff = 0f;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        knockbackVelocity = direction * force;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        sr = GetComponent<SpriteRenderer>();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        footstepsEnabled = true;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        footstepsEnabled = false;
    }

    void Update()
    {
        if (GameManager.IsPlayerBlocked)
        {
            movement = Vector2.zero;
            animator.SetFloat("moveX", 0f);
            animator.SetFloat("moveY", 0f);
            return;
        }

        float x = 0f;
        float y = 0f;

        // AZERTY : Z = haut, S = bas, Q = gauche, D = droite

       

        if (Input.GetKey(KeyCode.Z)) y = 1f;
        if (Input.GetKey(KeyCode.S)) y = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;
        if (Input.GetKey(KeyCode.Q)) x = -1f;

        animator.SetFloat("moveX", x);
        animator.SetFloat("moveY", y);



        // Flèches directionnelles (universel)
        if (Input.GetKey(KeyCode.UpArrow))    y =  1f;
        if (Input.GetKey(KeyCode.DownArrow))  y = -1f;
        if (Input.GetKey(KeyCode.LeftArrow))  x = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) x =  1f;

        movement = new Vector2(x, y).normalized;

        // Flip selon la direction horizontale
        if (sr != null)
        {
            if (x < 0f) sr.flipX = true;
            else if (x > 0f) sr.flipX = false;
        }
        animator.SetFloat("moveX", x);
        animator.SetFloat("moveY", y);

        // Bruit de pas
        if (footstepsEnabled && movement.magnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        if (GameManager.IsPlayerBlocked)
        {
            knockbackVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;   // stoppe toute vélocité résiduelle
            return;
        }

        // Dissipe le knockback progressivement
        knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, knockbackDamping * Time.fixedDeltaTime);

        rb.MovePosition(rb.position + (movement * SpeedTotal + knockbackVelocity) * Time.fixedDeltaTime);
    }
}
