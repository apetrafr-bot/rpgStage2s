using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathContainer : MonoBehaviour
{
    public List<TileClass> items = new List<TileClass>();
    public List<string> approcheMessages = new List<string>()
    {
        "Ton butin est la...",
        "Tes affaires gisent au sol.",
        "Tu avais oublie ca ?",
        "Vite, recupere ton stuff !"
    };
    public float interactRange = 2f;

    private Transform player;
    private pikupItem playerPickup;
    private string currentMessage;
    private bool wasInRange = false;

    void Start()
    {
        GameObject joueur = GameObject.FindWithTag("Player");
        if (joueur != null)
        {
            player = joueur.transform;
            playerPickup = joueur.GetComponentInChildren<pikupItem>();
        }
        currentMessage = approcheMessages.Count > 0 ? approcheMessages[Random.Range(0, approcheMessages.Count)] : "";
    }

    void Update()
    {
        if (player == null) return;
        if (playerHealth.Instance != null && playerHealth.Instance.IsDead) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRange = dist <= interactRange;

        if (inRange && !wasInRange)
            currentMessage = approcheMessages[Random.Range(0, approcheMessages.Count)];

        if (inRange)
        {
            if (playerPickup != null && playerPickup.textInteract != null)
                playerPickup.textInteract.text = currentMessage;
            if (playerPickup != null)
                playerPickup.showText = true;

            if (Input.GetKeyDown(KeyCode.E))
                Ouvrir();
        }
        else
        {
            if (playerPickup != null)
                playerPickup.showText = false;
        }

        wasInRange = inRange;
    }

    void Ouvrir()
    {
        if (playerPickup != null)
        {
            playerPickup.showText = false;
            if (playerPickup.textInteract != null)
                playerPickup.textInteract.text = "Interact";
        }

        inventory inv = inventory.Instance;
        if (inv == null) return;

        foreach (TileClass item in items)
        {
            if (item == null) continue;
            if (!inv.AddItem(item))
                break;
        }

        DeathLootData.Clear();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
