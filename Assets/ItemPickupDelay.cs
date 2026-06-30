using UnityEngine;

/// <summary>
/// Empêche un item d'être ramassé pendant un délai après son spawn.
/// Ajouter ce script sur le prefab de chaque item ramassable.
/// </summary>
public class ItemPickupDelay : MonoBehaviour
{
    public float delay = 2f;

    private float spawnTime;

    public bool CanPickup => Time.time >= spawnTime + delay;

    void Awake()
    {
        spawnTime = Time.time;
    }
}
