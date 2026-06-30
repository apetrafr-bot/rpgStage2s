using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class pikupItem : MonoBehaviour
{
    public float playerRange = 1f;
    public inventory playerInventory;
    public Text textInteract;
    public bool showText = false;
    public questManager questManager;

    [Header("Audio")]
    public AudioClip pickupSound;
    public float pickupSoundDuration;

    private int showTextRequestCount = 0;
    private GameObject itemInRange = null;
    private ContactFilter2D _pickupFilter;
    private Collider2D[] _pickupHits = new Collider2D[10];

    public void RequestShowText()  { showTextRequestCount++; }
    public void RequestHideText()  { showTextRequestCount = Mathf.Max(0, showTextRequestCount - 1); }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        showTextRequestCount = 0;
        showText = false;
        itemInRange = null;

        if (textInteract != null)
        {
            textInteract.enabled = false;
        }

        Debug.Log($"[pikupItem] Scene chargée ({scene.name}) — état réinitialisé");
    }

    void Start()
    {
        if (questManager == null)
            questManager = questManager.Instance;

        if (textInteract != null)
        {
            textInteract.text = "Interact";
            textInteract.enabled = false;
        }

        _pickupFilter = new ContactFilter2D();
        _pickupFilter.useTriggers = true;
        _pickupFilter.useLayerMask = false;
    }

    void Update()
    {
        DetecterEtRamasserItems();

        if (textInteract != null)
        {
            bool itemProche = itemInRange != null && !EstAutoPickup(itemInRange);
            textInteract.enabled = itemProche || showTextRequestCount > 0 || showText;
            if (itemProche)
                textInteract.text = "Interact";
        }

        if (itemInRange != null && Input.GetKeyDown(KeyCode.E))
        {
            RamasserItem(itemInRange);
            itemInRange = null;
        }
    }

    private void DetecterEtRamasserItems()
    {
        itemInRange = null;

        int count = Physics2D.OverlapCircle(transform.position, playerRange, _pickupFilter, _pickupHits);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = _pickupHits[i];
            if (hit == null) continue;

            if (hit.CompareTag("Item"))
            {
                ItemPickupDelay delay = hit.GetComponent<ItemPickupDelay>();
                if (delay != null && !delay.CanPickup) continue;

                refTile refT = hit.GetComponent<refTile>();
                if (refT == null || refT.tileClass == null) continue;

                if (refT.tileClass.autoPickup)
                {
                    RamasserItem(hit.gameObject);
                    break;
                }

                itemInRange = hit.gameObject;
                break;
            }
        }
    }

    private bool EstAutoPickup(GameObject item)
    {
        refTile refT = item.GetComponent<refTile>();
        return refT != null && refT.tileClass != null && refT.tileClass.autoPickup;
    }

    private void RamasserItem(GameObject item)
    {
        if (playerInventory == null)
        {
            Debug.LogError("pikupItem : playerInventory non assigné !");
            return;
        }

        refTile refT = item.GetComponent<refTile>();
        if (refT == null || refT.tileClass == null)
        {
            Debug.LogError("pikupItem : refTile ou tileClass manquant sur " + item.name);
            return;
        }

        if (!playerInventory.AddItem(refT.tileClass))
        {
            Debug.Log("Inventaire plein !");
            return;
        }



        if (questManager != null && refT.tileClass.isSword)
            questManager.OnSwordFound();

        SaveManager.RegisterCollectedItem(item.transform.position);
        Destroy(item);
    }
}
