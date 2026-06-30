using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Inventaire à grille fixe (comme Minecraft / Zelda).
/// Les slots sont créés une seule fois au Start() et ne sont JAMAIS détruits.
/// On les met à jour en place à chaque changement.
/// Ouvrir/fermer avec Tab ou I.
/// </summary>
public class inventory : MonoBehaviour
{
    public static inventory Instance { get; private set; }

    [System.Serializable]
    public class ItemStack
    {
        public TileClass item;
        public int count;
        public ItemStack(TileClass i, int c) { item = i; count = c; }
    }

    [Header("Taille de l'inventaire")]
    public int totalSlots = 20;             // nombre de slots dans la grille

    private List<ItemStack> stacks = new List<ItemStack>();

    
    [Header("UI - Inventaire")]
    public GameObject inventoryPanel;       // Panel parent (on l'active/désactive)
    public Transform  slotsParent;          // GridLayoutGroup contenant les slots
    public GameObject slotPrefab;           // Prefab d'un slot

    [Header("UI - HotBar")]
    public HotBar hotBar;

    private slot[] allSlots;                // tableau fixe de tous les slots instanciés
    private bool isOpen = false;

    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // L'image de fond du panel ne doit pas bloquer les events de drag
        if (inventoryPanel != null)
        {
            Image panelImage = inventoryPanel.GetComponent<Image>();
            if (panelImage != null) panelImage.raycastTarget = false;
        }

        // Les Textes UI ne doivent pas bloquer les clics sur les slots
        Text qt = GameObject.Find("quetteText")?.GetComponent<Text>();
        if (qt != null) qt.raycastTarget = false;

        allSlots = new slot[totalSlots];
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotsParent);
            go.name = "Slot_" + i;
            allSlots[i] = go.GetComponent<slot>();
            allSlots[i].Init();

            // S'assure que le root du slot a une Image raycastable
            // (nécessaire pour recevoir les events BeginDrag/Drag/EndDrag)
            if (go.GetComponent<Image>() == null)
            {
                Image raycastImg = go.AddComponent<Image>();
                raycastImg.color = Color.clear;
                raycastImg.raycastTarget = true;
            }

            // Drag & drop : lie chaque slot à cet inventaire
            // Les slots de l'inventaire commencent après les slots hotbar
            allSlots[i].linkedInventory = this;
            allSlots[i].slotIndex       = HotBar.SIZE + i;
        }

        // Lie aussi les slots de la hotbar à cet inventaire
        // Ils occupent les indices 0..HotBar.SIZE-1 dans la liste stacks
        if (hotBar != null)
        {
            for (int i = 0; i < HotBar.SIZE; i++)
            {
                if (hotBar.slots[i] != null)
                {
                    hotBar.slots[i].linkedInventory = this;
                    hotBar.slots[i].slotIndex       = i;
                }
            }
        }

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (inventoryPanel == null || slotsParent == null || allSlots == null || allSlots.Length == 0)
        {
            Debug.LogWarning("Inventory references perdues, tentative de réinitialisation...");
            return;
        }

        for (int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i] != null)
            {
                allSlots[i].linkedInventory = this;
                allSlots[i].slotIndex = HotBar.SIZE + i;
            }
        }

        if (hotBar != null)
        {
            for (int i = 0; i < HotBar.SIZE; i++)
            {
                if (hotBar.slots[i] != null)
                {
                    hotBar.slots[i].linkedInventory = this;
                    hotBar.slots[i].slotIndex = i;
                }
            }
        }

        Refresh();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        // Ouvrir / fermer avec Tab
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        if (inventoryPanel != null)
            inventoryPanel.SetActive(isOpen);
        if (isOpen) GameManager.OpenPanel();
        else        GameManager.ClosePanel();
    }


    // Capacité totale = slots hotbar + slots grille inventaire
    private int TotalCapacity => HotBar.SIZE + totalSlots;

    private int CountNonNullStacks()
    {
        int count = 0;
        for (int i = 0; i < stacks.Count; i++)
            if (stacks[i] != null && stacks[i].item != null) count++;
        return count;
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < stacks.Count; i++)
            if (stacks[i] == null || stacks[i].item == null) return i;
        return -1;
    }

    public bool AddItem(TileClass item, int amount = 1)
    {
        bool allAdded = true;

        if (item != null && !item.isStakable)
        {
            while (amount > 0)
            {
                if (CountNonNullStacks() >= TotalCapacity)
                {
                    allAdded = false;
                    break;
                }

                int emptyIdx = FindFirstEmptySlot();
                if (emptyIdx >= 0)
                    stacks[emptyIdx] = new ItemStack(item, 1);
                else
                    stacks.Add(new ItemStack(item, 1));
                amount--;
            }

            Refresh();
            return allAdded && amount == 0;
        }

        int maxStack = Mathf.Max(1, item.maxStack);

        foreach (ItemStack stack in stacks)
        {
            if (stack != null && stack.item == item && stack.count < maxStack)
            {
                int space = maxStack - stack.count;
                int added = Mathf.Min(space, amount);
                stack.count += added;
                amount -= added;
                if (amount <= 0) break;
            }
        }

        while (amount > 0)
        {
            if (CountNonNullStacks() >= TotalCapacity)
            {
                allAdded = false;
                break;
            }
            int emptyIdx = FindFirstEmptySlot();
            int toAdd = Mathf.Min(amount, maxStack);
            if (emptyIdx >= 0)
                stacks[emptyIdx] = new ItemStack(item, toAdd);
            else
                stacks.Add(new ItemStack(item, toAdd));
            amount -= toAdd;
        }

        Refresh();
        return allAdded && amount == 0;
    }

  
    public void RemoveItem(TileClass item, int amount = 1)
    {
        for (int i = stacks.Count - 1; i >= 0; i--)
        {
            if (stacks[i] != null && stacks[i].item == item)
            {
                int removed = Mathf.Min(stacks[i].count, amount);
                stacks[i].count -= removed;
                amount -= removed;
                if (stacks[i].count <= 0)
                    stacks[i] = null;
                if (amount <= 0) break;
            }
        }
        Refresh();
    }

    
    public void RemoveFromSlot(int index, int amount = 1)
    {
        if (index < 0 || index >= stacks.Count) return;
        if (stacks[index] == null || stacks[index].item == null) return;

        stacks[index].count -= amount;
        if (stacks[index].count <= 0)
            stacks[index] = null;
        Refresh();
    }

    public void Refresh()
    {
        // La hotbar occupe les premiers HotBar.SIZE stacks.
        // La grille d'inventaire affiche uniquement les stacks suivants.
        int hotBarSize = (hotBar != null) ? HotBar.SIZE : 0;

        for (int i = 0; i < totalSlots; i++)
        {
            int stackIndex = hotBarSize + i;   // décalage : on saute les slots hotbar
            if (stackIndex < stacks.Count && stacks[stackIndex] != null && stacks[stackIndex].item != null)
                allSlots[i].SetItem(stacks[stackIndex].item, stacks[stackIndex].count);
            else
                allSlots[i].ClearSlot();
        }

        // Met à jour la hotbar (elle affiche toujours les premiers stacks)
        if (hotBar != null)
            hotBar.RefreshHotBar(stacks);
    }
    public void returnObjectSelect()
    {

    }
    
    public int CountItem(TileClass item)
    {
        int total = 0;
        foreach (var s in stacks)
            if (s != null && s.item == item) total += s.count;
        return total;
    }

    public bool HasItem(TileClass item, int amount = 1) => CountItem(item) >= amount;

    public List<ItemStack> GetStacks() => new List<ItemStack>(stacks);

    public void ClearInventory()
    {
        stacks.Clear();
        Refresh();
    }

    /// <summary>Échange deux stacks par leur index absolu (hotbar incluse).</summary>
    public void SwapStacks(int indexA, int indexB)
    {
        if (indexA == indexB) return;

        int needed = Mathf.Max(indexA, indexB) + 1;
        while (stacks.Count < needed)
            stacks.Add(null);

        ItemStack tmp  = stacks[indexA];
        stacks[indexA] = stacks[indexB];
        stacks[indexB] = tmp;

        for (int i = stacks.Count - 1; i >= 0; i--)
        {
            if (stacks[i] == null)
                stacks.RemoveAt(i);
            else
                break;
        }

        Refresh();
    }
}
