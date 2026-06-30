using UnityEngine;

public class PlacedTorch : MonoBehaviour
{
    public TileClass torchItem;
    public float interactRange = 2f;

    private Transform player;
    private pikupItem playerPickup;
    private inventory inv;
    private bool wasInRange = false;

    void Start()
    {
        GameObject joueur = GameObject.FindWithTag("Player");
        if (joueur != null)
        {
            player = joueur.transform;
            inv = inventory.Instance;
            playerPickup = joueur.GetComponentInChildren<pikupItem>();
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRange = dist <= interactRange;

        if (inRange && !wasInRange)
        {
            if (playerPickup != null)
                playerPickup.RequestShowText();
        }
        else if (!inRange && wasInRange)
        {
            if (playerPickup != null)
                playerPickup.RequestHideText();
        }

        wasInRange = inRange;

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        if (playerPickup != null)
            playerPickup.RequestHideText();

        if (inv != null && torchItem != null)
        {
            if (inv.AddItem(torchItem))
            {
                TorchController tc = FindFirstObjectByType<TorchController>();
                if (tc != null)
                    tc.RetirerDeLaListe(gameObject);


                Destroy(gameObject);
            }
        }
    }

    void OnDestroy()
    {
        if (playerPickup != null && wasInRange)
            playerPickup.RequestHideText();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
