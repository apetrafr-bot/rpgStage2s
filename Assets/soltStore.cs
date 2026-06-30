using UnityEngine;
using UnityEngine.UI;
public class soltStore : MonoBehaviour
{
    public TileClass item;
    public int cost;
    public Text textCost;
    public Image itemImage;
    public Button buyButton;
    //on cherche l'inventaire du joueur
    [SerializeField]
    private inventory playerInventory;
    public TileClass coins; // R�f�rence � l'objet repr�sentant les pi�ces
    public void Start()
    {
        playerInventory = FindFirstObjectByType<inventory>();
        buyButton.onClick.AddListener(BuyObject);
    }
    public void BuyObject()
    {
        //on verifie si le joueur a assez d'argent
        if (playerInventory.HasItem(coins, cost))
        {
            //on retire l'argent du joueur
            playerInventory.RemoveItem(coins, cost);
            //on ajoute l'objet au joueur
            playerInventory.AddItem(item, 1);
        }
        else
        {
            Debug.Log("Pas assez d'argent");
        }

    }
    public void SetItem(TileClass newItem, int newCost)
    {
        item = newItem;
        cost = newCost;
        // Update the UI elements here if needed
        if (textCost != null)
        {
            textCost.text = cost.ToString();
        }
        if (itemImage != null && item != null)
        {
            itemImage.sprite = item.tileSprite;
        }
    }
}
