using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int openPanelCount = 0;

    public static bool IsPlayerBlocked => Instance != null && Instance.openPanelCount > 0;

    // PNJ dont le dialogue est actuellement ouvert
    public static pnjManager PnjActif { get; private set; }

    [Header("References UI globales (persistantes)")]
    [Tooltip("Panel du dialogue PNJ (DontDestroyOnLoad).")]
    public Transform panelTospeak;

    [Tooltip("Panel du store PNJ (DontDestroyOnLoad).")]
    public Transform panelStore;

    [Tooltip("Text du dialogue PNJ (DontDestroyOnLoad).")]
    public Text dialogueText;

    [Header("Death Screen")]
    public GameObject deathScreenPanel;

    [Header("Boss")]
    [Tooltip("Image de remplissage de la barre de vie du boss.")]
    public Image bossBarreVieFill;
    [Tooltip("Conteneur de la barre de vie du boss.")]
    public GameObject bossBarreVieContainer;

    [Header("Zelda")]
    public GameObject panelZelda;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        if (SaveManager.SaveExists())
            SaveManager.Load();
    }

    public void ActiverPanelZelda()
    {
        if (panelZelda != null)
            panelZelda.SetActive(true);
    }

    public static void OpenPanel()
    {
        if (Instance != null)
            Instance.openPanelCount++;
    }

    public static void ClosePanel()
    {
        if (Instance != null)
            Instance.openPanelCount = Mathf.Max(0, Instance.openPanelCount - 1);
    }

    public static void SetPnjActif(pnjManager pnj) => PnjActif = pnj;
    public static void ClearPnjActif()             => PnjActif = null;

    public static void PassDialogue()
    {
        if (PnjActif != null)
            PnjActif.PassDialoque();
    }
}
