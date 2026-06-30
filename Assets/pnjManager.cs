using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pnjManager : MonoBehaviour
{
    private static HashSet<string> dejaParle = new HashSet<string>();

    public static List<string> DejaParleList()
    {
        return new List<string>(dejaParle);
    }

    public static void SetDejaParle(List<string> list)
    {
        dejaParle.Clear();
        foreach (var id in list)
            dejaParle.Add(id);
    }

    public static bool TousParles(int total)
    {
        return total > 0 && dejaParle.Count >= total;
    }

    public static int NombreParles() => dejaParle.Count;

    [Header("Audio")]
    public AudioClip talkSound;

    [Header("Identifiant unique du PNJ (pour memoire)")]
    public string npcId;

    [Header("store")]
    public Transform panelStore;
    public Transform panelTospeak;
    public GameObject slot;
    public List<itemStore> items = new List<itemStore>();
    public int nomberOfSlots = 5;
    public Transform player;
    public pikupItem playerPickup;
    public float interactRange = 2f;
    public bool isOpen = false;
    private bool wasInRange = false;
    [Header("Quête")]
    public bool reveleZelda = false;
    public bool isZelda = false;

    [Header("Dialogue")]
    public List<String> dialogue = new List<String>();
    public Text dialogueText;
    public int currentTextIndex = 0;

    private void Start()
    {
        if (string.IsNullOrEmpty(npcId))
            npcId = gameObject.name;

        ResoudreReferences();
    }

    /// <summary>
    /// Trouve automatiquement les references du joueur et du HUD
    /// dans la scene courante (utile quand le PNJ est dans une scene differente).
    /// </summary>
    private void ResoudreReferences()
    {
        // Joueur (toujours recherché, évite les refs mortes après un changement de scene)
        GameObject joueurGO = GameObject.FindWithTag("Player");
        if (joueurGO != null)
            player = joueurGO.transform;

        // pikupItem
        if (player != null)
            playerPickup = player.GetComponentInChildren<pikupItem>();

        // Panels et dialogue depuis le GameManager persistant
        if (GameManager.Instance != null)
        {
            if (panelTospeak == null) panelTospeak = GameManager.Instance.panelTospeak;
            if (panelStore == null)   panelStore   = GameManager.Instance.panelStore;
            if (dialogueText == null) dialogueText  = GameManager.Instance.dialogueText;
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRange = dist <= interactRange;

        // Incrémente/décrémente le compteur seulement quand l'état change
        if (inRange && !wasInRange)
            playerPickup.RequestShowText();
        else if (!inRange && wasInRange)
            playerPickup.RequestHideText();

        wasInRange = inRange;

        if (inRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (isOpen)
                {
                    CloseAllPanels();
                }
                else
                {
                    if (GameManager.PnjActif != null && GameManager.PnjActif != this)
                        GameManager.PnjActif.CloseAllPanels();

                    if (items.Count > 0 && panelStore != null)
                        OpenPanel(panelStore);

                    bool aDejaParle = !string.IsNullOrEmpty(npcId) && dejaParle.Contains(npcId);

                    if (dialogue.Count > 0 && panelTospeak != null && !aDejaParle)
                    {
                        OpenPanel(panelTospeak);
                        currentTextIndex = 0;
                        RefreshDialogue(currentTextIndex);
                    }
                }
            }
        }
    }

    private void CloseAllPanels()
    {
        if (panelStore != null && panelStore.gameObject.activeSelf)
            ClosePanel(panelStore);
        if (panelTospeak != null && panelTospeak.gameObject.activeSelf)
            ClosePanel(panelTospeak);
        isOpen = false;
    }

    public void PassDialoque()
    {
        currentTextIndex++;
        if (currentTextIndex < dialogue.Count)
            RefreshDialogue(currentTextIndex);
        else
        {
            if (!string.IsNullOrEmpty(npcId))
                dejaParle.Add(npcId);

            if (reveleZelda && questManager.Instance != null)
                questManager.Instance.OnZeldaRevele();

            if (isZelda && GameManager.Instance != null)
                GameManager.Instance.ActiverPanelZelda();

            ClosePanel(panelTospeak);
            if (panelStore == null || !panelStore.gameObject.activeSelf)
                isOpen = false;
        }
    }

    public void RefreshDialogue(int countText)
    {
        if (dialogue.Count == 0 || countText < 0 || countText >= dialogue.Count) return;
        dialogueText.text = dialogue[countText];
    }
 
    [System.Serializable]
    public class itemStore
    {
        public TileClass item;
        public int cost;
    }

    public void RefreshStore()
    {
        if (panelStore == null) return;
        foreach (Transform child in panelStore.transform)
            Destroy(child.gameObject);
        //on crée les nouveaux slots
        for (int i = 0; i < nomberOfSlots; i++)
        {
            GameObject newSlot = Instantiate(slot, panelStore.transform);
            soltStore ss = newSlot.GetComponent<soltStore>();
            if (ss == null)
            {
                Debug.LogError("Le prefab 'slot' n'a pas le script soltStore attaché !");
                continue;
            }
            if (i < items.Count)
            {

                ss.SetItem(items[i].item, items[i].cost);
                ss.itemImage.gameObject.SetActive(true);

            }
            else
            {

                ss.SetItem(null, 0);
                ss.itemImage.gameObject.SetActive(false);
            }
        }
    }
    public void OpenPanel(Transform panel)
    {
        if (panel == null) return;
        isOpen = true;
        panel.gameObject.SetActive(true);
        GameManager.OpenPanel();
        if (panel == panelTospeak)
        {
            GameManager.SetPnjActif(this);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PauseMusic();
                AudioManager.Instance.PlayTalkSound(talkSound);
            }
        }
        if (panel == panelStore)
            RefreshStore();
    }
    public void ClosePanel(Transform panel)
    {
        if (panel == null || !panel.gameObject.activeSelf) return;
        panel.gameObject.SetActive(false);
        GameManager.ClosePanel();
        if (panel == panelTospeak)
        {
            GameManager.ClearPnjActif();
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ResumeMusic();
                AudioManager.Instance.PauseTalkSound();
            }
        }
    }
    
}
