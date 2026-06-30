using UnityEngine;

public class teleporteur : MonoBehaviour
{
    [Header("Cible")]
    [Tooltip("Transform de destination (point B).")]
    public Transform pointB;

    [Tooltip("Tag du joueur.")]
    public string tagJoueur = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(tagJoueur)) return;
        if (pointB == null) return;

        other.transform.position = pointB.position;

        PlayerTrail trail = other.GetComponent<PlayerTrail>();
        if (trail != null) trail.ResetTrail();
    }
}
