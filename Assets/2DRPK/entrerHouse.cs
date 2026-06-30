using UnityEngine;
using UnityEngine.Rendering;

public class entrerHouse : MonoBehaviour
{
    public string sceneName; // Name of the scene to load when entering the house
    
    public Vector3 posTp = new Vector3(0, 0, 0); // Position to teleport the player in the new scene
    public void OnTriggerEnter2D(Collider2D collision)
    {
        //on detect le joueuret on le teleporte a l'interieur de la maison(new scene)
        GameObject player = collision.gameObject;
        if (player.CompareTag("Player"))
        {
            //on load la scene de la maison
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            //on peut aussi teleporter le joueur a une position specifique dans la nouvelle scene si necessaire
            player.transform.position = posTp; // Change this to the desired position in the new scene
        }
    }
}
