using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class questManager : MonoBehaviour
{
    public static questManager Instance { get; private set; }

    private static int questStep = 0;
    // 0 = "Cherche une épée dans ta maison"
    // 1 = "Parle à tous les PNJ"
    // 2 = "Retrouve Zelda"
    // 3 = terminé

    [Header("UI Quest")]
    public Text questText;
    public float fadeDuration = 1f;

    [Header("Messages des quêtes")]
    public string messageEpee = "Cherche une épée dans ta maison";
    public string messageParlePNJ = "Parle à tous les PNJ";
    public string messageRetrouveZelda = "Retrouve Zelda...";
    public int totalPNJ = 0;

    [Header("Inventaire joueur")]
    public inventory playerInventory;

    private CanvasGroup canvasGroup;
    private bool questActive = false;
    private static string prevScene = "";
    private static bool forceEtape2 = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (playerInventory == null)
            playerInventory = inventory.Instance;
        ResoudreReferences();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResoudreReferences();
    }

    private void ResoudreReferences()
    {
        Text nouveauQuestText = null;
        GameObject goQuest = GameObject.Find("Quest");
        if (goQuest == null)
        {
            try { goQuest = GameObject.FindWithTag("Quest"); }
            catch { /* tag non definie */ }
        }
        if (goQuest != null)
            nouveauQuestText = goQuest.GetComponent<Text>();
        if (nouveauQuestText != null)
            questText = nouveauQuestText;

        canvasGroup = questText != null ? questText.GetComponent<CanvasGroup>() : null;
        if (canvasGroup == null && questText != null)
            canvasGroup = questText.gameObject.AddComponent<CanvasGroup>();

        if (playerInventory == null)
            playerInventory = inventory.Instance;

        if (questStep >= 3)
        {
            if (questText != null)
                questText.gameObject.SetActive(false);
            return;
        }

        if (forceEtape2)
        {
            questStep = 2;
            AfficherQuest(messageRetrouveZelda);
            return;
        }

        if (playerInventory != null && PossedeEpee())
        {
            if (questStep == 0)
                questStep = 1;
        }

        AfficherQuestSelonEtape();
    }

    void Update()
    {
        if (questStep >= 3 || playerInventory == null) return;

        if (forceEtape2)
            return;

        string currentScene = SceneManager.GetActiveScene().name;

        if (questStep == 0)
        {
            if (PossedeEpee() && prevScene == "house2" && currentScene != "house2")
            {
                questStep = 1;
                AfficherQuest(messageParlePNJ);
            }
            else if (PossedeEpee())
            {
                questStep = 1;
                AfficherQuest(messageParlePNJ);
            }
        }

        if (questStep == 1)
        {
            if (pnjManager.TousParles(totalPNJ))
            {
                questStep = 2;
                AfficherQuest(messageRetrouveZelda);
            }
        }

        prevScene = currentScene;
    }

    public void OnZeldaRevele()
    {
        if (questStep < 2)
        {
            forceEtape2 = true;
            questStep = 2;
            AfficherQuest(messageRetrouveZelda);
        }
    }

    public void OnParleAPNJImportant()
    {
        if (questStep < 2)
        {
            forceEtape2 = true;
            questStep = 2;
            AfficherQuest(messageRetrouveZelda);
        }
    }

    public void OnTousPNJParle()
    {
        if (questStep == 1)
        {
            questStep = 2;
            AfficherQuest(messageRetrouveZelda);
        }
    }

    private void AfficherQuestSelonEtape()
    {
        if (questText == null) return;

        switch (questStep)
        {
            case 0: AfficherQuest(messageEpee); break;
            case 1: AfficherQuest(messageParlePNJ); break;
            case 2: AfficherQuest(messageRetrouveZelda); break;
            default:
                if (questText != null)
                    questText.gameObject.SetActive(false);
                break;
        }
    }

    private bool PossedeEpee()
    {
        foreach (var stack in playerInventory.GetStacks())
        {
            if (stack.item != null && stack.item.isSword)
                return true;
        }
        return false;
    }

    public void AfficherQuest(string message)
    {
        if (questText == null) return;
        questText.text = message;
        questActive = true;
        canvasGroup.alpha = 1f;
    }

    public void OnSwordFound()
    {
        if (questStep == 0)
        {
            questStep = 1;
            AfficherQuest(messageParlePNJ);
        }
    }

    public void CompleterQuest()
    {
        questStep = 3;
        questActive = false;
        if (questText != null)
            questText.gameObject.SetActive(false);
    }

    public bool EstQuestActive() => questActive;
    public bool EstQuestCompletee() => questStep >= 3;
    public void SetQuestCompleted(bool value) { if (value) questStep = 3; }
    public int GetQuestStep() => questStep;
    public void SetQuestStep(int step) => questStep = step;
}
