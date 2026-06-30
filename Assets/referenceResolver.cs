using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class referenceResolver : MonoBehaviour
{
    [Header("Joueur (auto-trouvé)")]
    public GameObject joueur;

    [Header("UI - Coeurs (playerHealth)")]
    public Image coeur1;
    public Image coeur2;
    public Image coeur3;
    public GameObject heartPrefab;
    public Transform heartContainer;

    [Header("UI - Inventaire")]
    public GameObject inventoryPanel;
    public Transform slotsParent;
    public HotBar hotBar;

    [Header("UI - PNJ (paneaux)")]
    public Transform panelTospeak;
    public Transform panelStore;
    public Text dialogueText;

    [Header("UI - Death Screen")]
    public GameObject deathScreenPanel;

    [Header("Transition")]
    public GameObject canvasHUD;
    public GameObject objetTransition;

    [Header("Tags")]
    public string tagJoueur = "Player";
    public string tagCanvasHUD = "CanvasHUD";
    public string tagPanelTospeak = "PanelTospeak";
    public string tagPanelStore = "PanelStore";
    public string tagDeathScreen = "DeathScreenPanel";

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResoudreToutesReferences();
    }

    public void ResoudreToutesReferences()
    {
        // Toujours rechercher le joueur par tag (evite les refs mortes apres changement de scene)
        GameObject go = GameObject.FindWithTag(tagJoueur);
        if (go != null) joueur = go;

        if (joueur == null) return;

        AssignerReferencesScene();
        ResoudreHealth();
        ResoudreInventaire();
        ResoudreTransition();
        ResoudrePNJ();
    }

    private void ResoudreHealth()
    {
        playerHealth health = joueur.GetComponent<playerHealth>();
        if (health == null) return;

        if (coeur1 != null) health.image1 = coeur1;
        if (coeur2 != null) health.image2 = coeur2;
        if (coeur3 != null) health.image3 = coeur3;

        if (heartContainer == null && coeur1 != null)
            heartContainer = coeur1.transform.parent;
        if (heartContainer != null) health.heartContainer = heartContainer;

        if (heartPrefab == null && coeur1 != null)
            heartPrefab = coeur1.gameObject;
        if (heartPrefab != null) health.heartPrefab = heartPrefab;
    }

    private void ResoudreInventaire()
    {
        inventory inv = inventory.Instance;
        if (inv == null) return;

        if (inventoryPanel != null) inv.inventoryPanel = inventoryPanel;
        if (slotsParent != null) inv.slotsParent = slotsParent;
        if (hotBar != null) inv.hotBar = hotBar;
    }

    private void ResoudreTransition()
    {
        joueurSceneTransition jst = joueur.GetComponent<joueurSceneTransition>();
        if (jst == null) return;

        if (canvasHUD != null) jst.canvasHUD = canvasHUD;
        if (objetTransition != null) jst.objetTransition = objetTransition;

        foreach (triggerTransition tt in FindObjectsByType<triggerTransition>(FindObjectsSortMode.None))
        {
            if (tt.objetTransition == null && objetTransition != null)
                tt.objetTransition = objetTransition;
        }
    }

    private void ResoudrePNJ()
    {
        if (GameManager.Instance == null) return;

        if (panelTospeak != null) GameManager.Instance.panelTospeak = panelTospeak;
        if (panelStore != null) GameManager.Instance.panelStore = panelStore;
        if (dialogueText != null) GameManager.Instance.dialogueText = dialogueText;
        if (deathScreenPanel != null) GameManager.Instance.deathScreenPanel = deathScreenPanel;
    }

    public void AssignerReferencesScene()
    {
        if (canvasHUD == null)
        {
            GameObject go = GameObject.FindWithTag(tagCanvasHUD);
            if (go != null) canvasHUD = go;
        }

        if (panelTospeak == null)
        {
            GameObject go = GameObject.FindWithTag(tagPanelTospeak);
            if (go != null) panelTospeak = go.transform;
        }

        if (panelStore == null)
        {
            GameObject go = GameObject.FindWithTag(tagPanelStore);
            if (go != null) panelStore = go.transform;
        }

        if (inventoryPanel == null && canvasHUD != null)
        {
            Transform found = canvasHUD.transform.Find("InventoryPanel");
            if (found != null) inventoryPanel = found.gameObject;
        }

        if (hotBar == null && canvasHUD != null)
        {
            hotBar = canvasHUD.GetComponentInChildren<HotBar>();
        }

        if (slotsParent == null && inventoryPanel != null)
        {
            Transform found = inventoryPanel.transform.Find("SlotsParent");
            if (found != null) slotsParent = found;
        }

        if (coeur1 == null && canvasHUD != null)
        {
            Image[] images = canvasHUD.GetComponentsInChildren<Image>();
            foreach (Image img in images)
            {
                if (img.name == "Coeur1") coeur1 = img;
                else if (img.name == "Coeur2") coeur2 = img;
                else if (img.name == "Coeur3") coeur3 = img;
            }
        }

        if (heartContainer == null && coeur1 != null)
            heartContainer = coeur1.transform.parent;

        if (heartPrefab == null && coeur1 != null)
            heartPrefab = coeur1.gameObject;

        if (dialogueText == null && canvasHUD != null)
        {
            dialogueText = canvasHUD.GetComponentInChildren<Text>();
        }

        if (deathScreenPanel == null)
        {
            GameObject go = GameObject.FindWithTag(tagDeathScreen);
            if (go != null) deathScreenPanel = go;
        }
    }
}
