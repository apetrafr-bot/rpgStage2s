using UnityEngine;

[CreateAssetMenu(fileName = "TileClass", menuName = "Scriptable Objects/TileClass")]
public class TileClass : ScriptableObject
{
    public string tileName;
    public Sprite tileSprite;
    public bool isSword;
    public string description;
    public GameObject tilePrefab;
    public int maxStack = 10;   // taille max d'un stack (1 = non stackable)
    public float range;
    public int damaged;
    public bool isStakable;
    public bool isGun;
    public bool isBoomerang;
    public bool autoPickup;
    public bool isAutoFire;          // tir continu tant que la souris est maintenue
    public float speedBalle;
    public GameObject balle;
    public int balleCount = 1;       // nombre de balles tirées en rafale
    public float balleDelay = 0.3f;  // délai en secondes entre chaque balle
    public float knockback = 0f;     // recul appliqué au joueur à chaque tir / coup d'épée

    [Header("Audio")]
    public AudioClip useSound;
    public float useSoundDuration;
    public float useSoundOffset;

    [Header("Consommable")]
    public bool isConsumable;
    public int healAmount;
    public int addMaxHealth;

    [Header("Potion de vitesse")]
    public float speedBoost;
    public float speedBoostDuration;

    [Header("Potion de force")]
    public int damageBoost;
    public float damageBoostDuration;

    [Header("Potion de lumiere")]
    public float lightBoostDuration;
}
