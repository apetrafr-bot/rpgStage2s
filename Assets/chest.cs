using System.Collections.Generic;
using UnityEngine;

public class chest : MonoBehaviour
{
    [Header("Contenu du coffre")]
    public List<TileClass> items = new List<TileClass>();

    [Header("Interaction")]
    public float interactRange = 2f;

    private Transform player;
    private pikupItem playerPickup;

    void Start()
    {
        GameObject joueur = GameObject.FindWithTag("Player");
        if (joueur != null)
        {
            player = joueur.transform;
            playerPickup = joueur.GetComponentInChildren<pikupItem>();
        }
    }

    void Update()
    {
        if (player == null) return;
        if (playerHealth.Instance != null && playerHealth.Instance.IsDead) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= interactRange)
        {
            playerPickup.showText = true;
            if (Input.GetKeyDown(KeyCode.E))
            {
                OpenChest();
            }
        }
        else
        {
            playerPickup.showText = false;
        }
    }

    void OpenChest()
    {
        playerPickup.showText = false; // Cache le texte d'interaction
        // Droppe chaque item du coffre au sol autour du coffre
        foreach (TileClass tileClass in items)
        {
            if (tileClass == null || tileClass.tilePrefab == null) continue;

            Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
            GameObject dropped = Instantiate(tileClass.tilePrefab, spawnPos, Quaternion.identity);

            // S'assure que le prefab a un refTile avec la bonne référence
            refTile rt = dropped.GetComponent<refTile>();
            if (rt == null) rt = dropped.AddComponent<refTile>();
            rt.tileClass = tileClass;

            // S'assure que l'item a le bon tag pour être ramassé par pikupItem
            dropped.tag = "Item";
        }

        // Détruit le coffre
        Destroy(gameObject);
    }

    // Visualise la portée d'interaction dans l'éditeur
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
