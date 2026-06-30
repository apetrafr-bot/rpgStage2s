using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Hotbar de 3 slots FIXES, toujours visibles à l'écran.
/// Indépendante de l'inventaire panel (ne s'ouvre/ferme pas).
/// Sélection : touches 1/2/3 ou molette souris.
/// </summary>
public class HotBar : MonoBehaviour
{
    public const int SIZE = 3;

    [Header("Slots (à assigner dans l'Inspector)")]
    public slot[] slots = new slot[SIZE];

    [Header("Sélection visuelle")]
    public Color selectedColor = new Color(1f, 0.85f, 0f, 1f);   // jaune
    public Color defaultColor  = new Color(1f, 1f,   1f, 1f);    // blanc

    private int selectedIndex = 0;
    private Image[] backgrounds = new Image[SIZE];

   
    void Start()
    {
        for (int i = 0; i < SIZE; i++)
        {
            if (slots[i] != null)
            {
                slots[i].Init();
                backgrounds[i] = slots[i].GetComponent<Image>();
            }
        }
        UpdateHighlight();
    }

    void Update()
    {
        // AZERTY : & (Alpha1) = slot 1, é (Alpha2) = slot 2, " (Alpha3) = slot 3
        if (Input.GetKeyDown(KeyCode.Ampersand)) Select(0);  // &
        if (Input.GetKeyDown(KeyCode.Alpha2))    Select(1);  // é
        if (Input.GetKeyDown(KeyCode.Quote))     Select(2);  // "

        // Molette
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) Select((selectedIndex - 1 + SIZE) % SIZE);
        if (scroll < 0f) Select((selectedIndex + 1)        % SIZE);
    }

   
    public void RefreshHotBar(List<inventory.ItemStack> stacks)
    {
        for (int i = 0; i < SIZE; i++)
        {
            if (slots[i] == null) continue;

            if (i < stacks.Count && stacks[i] != null && stacks[i].item != null)
                slots[i].SetItem(stacks[i].item, stacks[i].count);
            else
                slots[i].ClearSlot();
        }
    }

   
    void Select(int index)
    {
        selectedIndex = index;
        UpdateHighlight();
    }

    void UpdateHighlight()
    {
        for (int i = 0; i < SIZE; i++)
        {
            if (backgrounds[i] != null)
                backgrounds[i].color = (i == selectedIndex) ? selectedColor : defaultColor;
        }
    }

    
    public TileClass GetSelectedItem()
    {
        if (slots[selectedIndex] == null || slots[selectedIndex].IsEmpty()) return null;
        return slots[selectedIndex].GetItem();
    }

    public int GetSelectedIndex() => selectedIndex;
    public void SetSelectedIndex(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, SIZE - 1);
        UpdateHighlight();
    }
}
