using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class playerHealth : MonoBehaviour
{
    public static playerHealth Instance { get; private set; }

    public int currentHealth;
    public int maxHealth;

    [Range(0f, 1f)]
    public float emptyHeartScale = 0.6f;

    public playerMove playerMove;

    [Header("Coeurs UI")]
    public Image image1;
    public Image image2;
    public Image image3;
    public GameObject heartPrefab;
    public Transform heartContainer;

    [Header("Death")]
    public KeyCode respawnKey = KeyCode.Space;
    public Text deathText;
    public List<string> deathMessages = new List<string>()
    {
        "Vous etes mort. Appuyez sur Espace pour reapparaitre."
    };
    public Sprite deathContainerSprite;
    public List<string> deathContainerMessages = new List<string>()
    {
        "Ton butin est la...",
        "Tes affaires gisent au sol.",
        "Tu avais oublie ca ?",
        "Vite, recupere ton stuff !"
    };

    [Header("Respawn")]
    public string respawnSceneName = "MainScene";
    public Vector3 respawnPosition;

    private List<Image> hearts = new List<Image>();
    private List<Vector3> heartScales = new List<Vector3>();

    private bool initialise = false;
    public bool IsDead { get; private set; }
    private bool respawnRequested = false;
    private SpriteRenderer _sr;
    private gridDonjon _grilleDonjon;

    void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (DeathLootData.hasLoot && scene.name == DeathLootData.tombSceneName)
        {
            if (DeathLootData.fromDonjon)
            {
                Transform portal = null;
                foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (go.name.Contains("PortailDonjon"))
                    {
                        portal = go.transform;
                        break;
                    }
                }

                Vector3 pos = portal != null ? portal.position + new Vector3(0, -3f, 0) : new Vector3(0, -3f, 0);
                CreateDeathContainer(pos);
            }
            else
            {
                CreateDeathContainer(DeathLootData.tombPosition);
            }
        }

        if (scene.name == respawnSceneName && IsDead)
        {
            transform.position = respawnPosition;
            currentHealth = maxHealth;
            IsDead = false;
            gameObject.SetActive(true);
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _sr.enabled = true;
        }
    }

    public void Start()
    {
        CollectHearts();

        if (!initialise)
        {
            currentHealth = maxHealth;
            initialise = true;
        }

        _sr = GetComponent<SpriteRenderer>();
        _grilleDonjon = FindFirstObjectByType<gridDonjon>();
    }

    private void CollectHearts()
    {
        hearts.Clear();
        heartScales.Clear();

        if (image1 != null) { hearts.Add(image1); heartScales.Add(image1.rectTransform.localScale); }
        if (image2 != null) { hearts.Add(image2); heartScales.Add(image2.rectTransform.localScale); }
        if (image3 != null) { hearts.Add(image3); heartScales.Add(image3.rectTransform.localScale); }
    }

    public void Update()
    {
        for (int i = 0; i < hearts.Count; i++)
            UpdateHeart(hearts[i], heartScales[i], currentHealth >= i + 1);

        if (!IsDead && _grilleDonjon != null && !_grilleDonjon.EstDansLeDonjon(transform.position))
        {
            currentHealth = 0;
            Death();
        }
    }

    private void UpdateHeart(Image heart, Vector3 baseScale, bool active)
    {
        if (heart == null) return;
        heart.rectTransform.localScale = active ? baseScale : baseScale * emptyHeartScale;
    }

    private void AddHeartUI()
    {
        if (heartContainer == null) return;

        GameObject newHeart;
        if (heartPrefab != null)
        {
            newHeart = Instantiate(heartPrefab, heartContainer);
        }
        else if (hearts.Count > 0 && hearts[0] != null)
        {
            newHeart = Instantiate(hearts[0].gameObject, heartContainer);
        }
        else
        {
            return;
        }

        newHeart.SetActive(true);
        Image img = newHeart.GetComponent<Image>();
        if (img != null)
        {
            hearts.Add(img);
            heartScales.Add(img.rectTransform.localScale);
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;

        while (hearts.Count < maxHealth)
            AddHeartUI();
    }

    public void TakeDamage(int damage)
    { 
        if (IsDead) return;
        currentHealth -= damage;

        if (CombatEffects.Instance != null)
            CombatEffects.Instance.OnPlayerTakeDamage(gameObject, damage, transform.position);

        if(currentHealth <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        if (currentHealth > 0 || IsDead) return;
        IsDead = true;

        inventory inv = inventory.Instance;
        if (inv != null)
        {
            bool donjon = SceneManager.GetActiveScene().name.Contains("Donjon");
            DeathLootData.Save(inv.GetStacks(), donjon);
            inv.ClearInventory();

            if (donjon)
            {
                DeathLootData.tombSceneName = respawnSceneName;
                DeathLootData.tombPosition = respawnPosition;
            }
            else
            {
                CreateDeathContainer(transform.position);
            }
        }

        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _sr.enabled = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameOverMusic);

        if (GameManager.Instance != null && GameManager.Instance.deathScreenPanel != null)
        {
            GameManager.Instance.deathScreenPanel.SetActive(true);
            GameManager.OpenPanel();

            if (deathText != null && deathMessages.Count > 0)
                deathText.text = deathMessages[Random.Range(0, deathMessages.Count)];
        }

        StartCoroutine(WaitForRespawn());
    }

    public void OnRespawnButtonClick()
    {
        respawnRequested = true;
    }

    private IEnumerator WaitForRespawn()
    {
        while (!Input.GetKeyDown(respawnKey) && !respawnRequested)
            yield return null;

        respawnRequested = false;

        if (GameManager.Instance != null && GameManager.Instance.deathScreenPanel != null)
        {
            GameManager.Instance.deathScreenPanel.SetActive(false);
            GameManager.ClosePanel();
        }

        if (SceneManager.GetActiveScene().name != respawnSceneName)
            SceneManager.LoadScene(respawnSceneName);
        else
            Respawn();
    }

    private void Respawn()
    {
        if (DeathLootData.hasLoot && DeathLootData.fromDonjon)
            CreateDeathContainer(transform.position);

        transform.position = respawnPosition;
        currentHealth = maxHealth;
        IsDead = false;
        gameObject.SetActive(true);

        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _sr.enabled = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySceneMusic(SceneManager.GetActiveScene().name);
    }

    private void CreateDeathContainer(Vector3 position)
    {
        DeathLootData.tombPosition = position;
        DeathLootData.tombSceneName = SceneManager.GetActiveScene().name;
        DeathLootData.tombMessages = new List<string>(deathContainerMessages);

        GameObject container = new GameObject("DeathContainer");
        container.transform.position = position;

        SpriteRenderer sr = container.AddComponent<SpriteRenderer>();
        if (deathContainerSprite != null)
            sr.sprite = deathContainerSprite;
        else
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.red);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        }
        sr.sortingOrder = 1000;

        BoxCollider2D col = container.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1, 1);

        DeathContainer dc = container.AddComponent<DeathContainer>();
        dc.items = DeathLootData.GetTileClassList();
        dc.approcheMessages = deathContainerMessages;
    }
}
