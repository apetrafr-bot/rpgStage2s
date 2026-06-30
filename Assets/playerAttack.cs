using UnityEngine;
using System.Collections;

public class playerAttack : MonoBehaviour
{
    public inventory playerInventory;
    public HotBar hotBar;

    [Header("Arme en main")]
    public GameObject weaponObject;
    public SpriteRenderer weaponRenderer;

    private Camera mainCam;
    private bool isFiring = false;
    private int damageBuff = 0;
    public bool hasLightBuff = false;
    private ContactFilter2D _swordFilter;
    private Collider2D[] _swordHits = new Collider2D[20];

    void Start()
    {
        mainCam = Camera.main;
        if (weaponObject != null)
            weaponObject.SetActive(false);

        _swordFilter = new ContactFilter2D();
        _swordFilter.useTriggers = true;
        _swordFilter.useLayerMask = false;
    }

    void Update()
    {
        RefreshHeldItemInHand();

        if (playerInventory == null) return;
        if (GameManager.IsPlayerBlocked) return;

        TileClass item = hotBar != null ? hotBar.GetSelectedItem() : null;
        if (item == null) return;

        // --- Épée ---
        if (item.isSword && Input.GetKeyDown(KeyCode.Mouse0))
        {
            int count = Physics2D.OverlapCircle(transform.position, item.range, _swordFilter, _swordHits);
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _swordHits[i];
                if (hit == null || !hit.CompareTag("AI")) continue;
                AIHealth aiHealth = hit.GetComponent<AIHealth>();
                if (aiHealth != null)
                {
                    int dmg = item.damaged + damageBuff;
                    aiHealth.takeDamage(dmg);
                    if (CombatEffects.Instance != null)
                        CombatEffects.Instance.OnPlayerHitEnemy(hit.gameObject, dmg, hit.transform.position);
                }
            }
        }

        // --- Gun ---
        if (item.isGun && !isFiring)
        {
            bool shoot = item.isAutoFire ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);
            if (shoot)
            {
                StartCoroutine(FireBurst(item));
            }
        }

        // --- Boomerang ---
        if (item.isBoomerang && !isFiring && Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (item.balle == null) return;

            if (mainCam == null) mainCam = Camera.main;
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 dir = (mouseWorld - weaponObject.transform.position).normalized;

            GameObject b = Instantiate(item.balle, weaponObject.transform.position, Quaternion.identity);
            b.transform.right = dir;
            BoomerangProjectile boom = b.GetComponent<BoomerangProjectile>();
            if (boom != null)
            {
                boom.damage = item.damaged + damageBuff;
                boom.speed = item.speedBalle;
                boom.returnSpeed = item.speedBalle * 1.5f;
                boom.maxDistance = item.range;
            }
            isFiring = true;
            StartCoroutine(WaitForBoomerang(b));
        }

        // --- Consommable (clic droit) ---
        if (item.isConsumable && Input.GetKeyDown(KeyCode.Mouse1))
        {
            playerHealth health = GetComponent<playerHealth>();
            if (health == null) return;

            bool used = false;

            if (item.healAmount > 0 && health.currentHealth < health.maxHealth)
            {
                health.Heal(item.healAmount);
                used = true;
            }

            if (item.addMaxHealth > 0)
            {
                health.IncreaseMaxHealth(item.addMaxHealth);
                used = true;
            }

            if (item.speedBoost > 0f && item.speedBoostDuration > 0f)
            {
                playerMove move = GetComponent<playerMove>();
                if (move != null) move.ApplySpeedBuff(item.speedBoost, item.speedBoostDuration);
                used = true;
            }

            if (item.damageBoost > 0 && item.damageBoostDuration > 0f)
            {
                ApplyDamageBuff(item.damageBoost, item.damageBoostDuration);
                used = true;
            }

            if (item.lightBoostDuration > 0f)
            {
                ApplyLightBuff(item.lightBoostDuration);
                used = true;
            }

            if (used)
            {
                playerInventory.RemoveFromSlot(hotBar.GetSelectedIndex());
            }
        }

    }

    public void ApplyDamageBuff(int boost, float duration)
    {
        damageBuff = boost;
        CancelInvoke(nameof(RemoveDamageBuff));
        Invoke(nameof(RemoveDamageBuff), duration);
    }

    private void RemoveDamageBuff()
    {
        damageBuff = 0;
    }

    public void ApplyLightBuff(float duration)
    {
        hasLightBuff = true;
        CancelInvoke(nameof(RemoveLightBuff));
        Invoke(nameof(RemoveLightBuff), duration);
    }

    private void RemoveLightBuff()
    {
        hasLightBuff = false;
    }

    private IEnumerator WaitForBoomerang(GameObject boomerang)
    {
        while (boomerang != null)
            yield return null;
        isFiring = false;
    }

    private IEnumerator FireBurst(TileClass item)
    {
        if (item.balle == null) yield break;
        isFiring = true;

        if (mainCam == null) mainCam = Camera.main;
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - weaponObject.transform.position).normalized;

        for (int i = 0; i < Mathf.Max(1, item.balleCount); i++)
        {
            GameObject b = Instantiate(item.balle, weaponObject.transform.position, Quaternion.identity);
            Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = dir * item.speedBalle;

            if (i < item.balleCount - 1)
                yield return new WaitForSeconds(item.balleDelay);
        }

        // Cooldown avant le prochain tir
        yield return new WaitForSeconds(item.balleDelay);

        isFiring = false;
    }

    private void RefreshHeldItemInHand()
    {
        if (weaponObject == null) return;
        if (mainCam == null) mainCam = Camera.main;

        TileClass item = (hotBar != null) ? hotBar.GetSelectedItem() : null;
        bool hasWeapon = item != null && (item.isSword || item.isGun || item.isBoomerang);

        weaponObject.SetActive(hasWeapon);
        if (!hasWeapon) return;

        // Applique le sprite de l'arme sélectionnée
        if (weaponRenderer != null)
            weaponRenderer.sprite = item.tileSprite;

        // Flip horizontal selon la position de la souris
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        bool mouseOnLeft = mouseWorld.x < weaponObject.transform.position.x;
        if (weaponRenderer != null)
            weaponRenderer.flipX = mouseOnLeft;
    }
}
