using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Slot d'inventaire avec drag & drop intégré.
/// </summary>
public class slot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Références UI")]
    public Image background;
    public Image icon;
    public Text stackText;

    [Header("Couleurs")]
    public Color emptyColor  = new Color(1f, 1f, 1f, 0.15f);
    public Color filledColor = new Color(1f, 1f, 1f, 1f);

    [HideInInspector] public inventory linkedInventory;
    [HideInInspector] public int slotIndex;

    private TileClass currentItem;
    private int currentCount = 0;

    // Image fantôme créée dynamiquement au début du drag
    private static GameObject ghostObj;
    private static Image      ghostImage;
    private static Canvas     rootCanvas;
    private static readonly List<RaycastResult> _dragResults = new List<RaycastResult>();

    // -------------------------------------------------------
    //  Initialisation
    // -------------------------------------------------------
    public void Init()
    {
        Image rootImg = GetComponent<Image>();
        if (rootImg == null)
        {
            rootImg = gameObject.AddComponent<Image>();
            rootImg.color = Color.clear;
        }
        rootImg.raycastTarget = true;

        if (background != null) background.color = emptyColor;
        if (icon != null)       { icon.enabled = false; icon.sprite = null; }
        if (stackText != null)  stackText.gameObject.SetActive(false);
        currentItem  = null;
        currentCount = 0;
    }

    // -------------------------------------------------------
    //  Affiche un item
    // -------------------------------------------------------
    public void SetItem(TileClass item, int count)
    {
        if (item == null) { ClearSlot(); return; }

        currentItem  = item;
        currentCount = count;

        if (icon != null)
        {
            icon.sprite  = item.tileSprite;
            icon.enabled = true;
            icon.color   = filledColor;
        }
        if (background != null) background.color = filledColor;
        if (stackText != null)
        {
            bool show = count > 1;
            stackText.gameObject.SetActive(show);
            if (show) stackText.text = count.ToString();
        }
    }

    // -------------------------------------------------------
    //  Vide le slot visuellement
    // -------------------------------------------------------
    public void ClearSlot()
    {
        currentItem  = null;
        currentCount = 0;
        if (icon != null)       { icon.enabled = false; icon.sprite = null; }
        if (background != null) background.color = emptyColor;
        if (stackText != null)  stackText.gameObject.SetActive(false);
    }

    public TileClass GetItem()  => currentItem;
    public int       GetCount() => currentCount;
    public bool      IsEmpty()  => currentItem == null;

    // -------------------------------------------------------
    //  Drag & Drop
    // -------------------------------------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsEmpty()) return;

        // Trouve le Canvas racine à chaque drag
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
        if (rootCanvas == null) return;

        // Crée l'image fantôme
        ghostObj = new GameObject("DragGhost");
        ghostObj.transform.SetParent(rootCanvas.transform, false);
        ghostImage = ghostObj.AddComponent<Image>();

        // Ignore les raycasts pour ne pas bloquer le OnDrop
        ghostImage.raycastTarget = false;

        RectTransform rt = ghostObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50f, 50f);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

        ghostObj.transform.SetAsLastSibling(); // toujours au-dessus
        ghostImage.sprite  = icon != null ? icon.sprite : null;
        ghostImage.enabled = true;

        // Cache l'icône du slot source
        if (icon != null) icon.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostObj == null || rootCanvas == null) return;

        // Convertit la position souris en coordonnées locales du Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            eventData.position,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventData.pressEventCamera,
            out Vector2 localPos);

        ghostObj.GetComponent<RectTransform>().localPosition = localPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostObj != null) { ghostObj.SetActive(false); Destroy(ghostObj); ghostObj = null; ghostImage = null; }

        if (linkedInventory != null && EventSystem.current != null)
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = eventData.position;
            _dragResults.Clear();
            EventSystem.current.RaycastAll(pointer, _dragResults);

            foreach (RaycastResult r in _dragResults)
            {
                slot target = r.gameObject.GetComponentInParent<slot>();
                if (target != null && target != this && target.linkedInventory == linkedInventory)
                {
                    linkedInventory.SwapStacks(slotIndex, target.slotIndex);
                    break;
                }
            }
        }

        if (!IsEmpty() && icon != null) icon.enabled = true;
    }
}
