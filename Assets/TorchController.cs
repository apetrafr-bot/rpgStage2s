using System.Collections.Generic;
using UnityEngine;

public class TorchController : MonoBehaviour
{
    public TileClass torchItem;

    [Header("Placement")]
    public KeyCode placeKey = KeyCode.Mouse0;
    public GameObject placedTorchPrefab;
    public int maxPlacedTorches = 10;

    public HotBar hotBar;
    public inventory inv;
    private List<GameObject> placedTorches = new List<GameObject>();

    void Update()
    {
        if (hotBar == null || torchItem == null) return;

        TileClass selected = hotBar.GetSelectedItem();
        bool hasTorch = (selected == torchItem);

        if (hasTorch && Input.GetKeyDown(placeKey))
        {
            PlacerTorche();
        }
    }

    void PlacerTorche()
    {
        Vector3 pos = transform.position + new Vector3(0, -0.5f, 0);

        if (placedTorches.Count >= maxPlacedTorches)
        {
            GameObject oldest = placedTorches[0];
            placedTorches.RemoveAt(0);
            Destroy(oldest);
        }

        GameObject go;

        if (placedTorchPrefab != null)
        {
            go = Instantiate(placedTorchPrefab, pos, Quaternion.identity);
        }
        else
        {
            go = new GameObject("PlacedTorche");
            go.transform.position = pos;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = torchItem.tileSprite;
            sr.sortingOrder = 0;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1f);
        }

        if (go.GetComponent<LightEmitter>() == null)
        {
            LightEmitter le = go.AddComponent<LightEmitter>();
            le.radius = 4f;
            le.intensity = 1f;
            le.flicker = true;
            le.flickerSpeed = 6f;
        }

        if (go.GetComponent<PlacedTorch>() == null)
        {
            PlacedTorch pt = go.AddComponent<PlacedTorch>();
            pt.torchItem = torchItem;
        }

        placedTorches.Add(go);
        inv.RemoveFromSlot(hotBar.GetSelectedIndex());
    }

    public void RetirerDeLaListe(GameObject torch)
    {
        placedTorches.Remove(torch);
    }
}
