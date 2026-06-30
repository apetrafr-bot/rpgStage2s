using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class triggerTransition : MonoBehaviour
{
    [Header("Tag du joueur")]
    public string tagJoueur = "Player";

    [Header("Transition")]
    [Tooltip("GameObject de transition a afficher (SetActive true/false).")]
    public GameObject objetTransition;

    [Tooltip("GameObjects supplementaires a cacher pendant la transition.")]
    public GameObject[] objetsACacher;

    [Tooltip("Duree d'affichage en secondes avant de charger la scene.")]
    public float duree = 2f;

    [Header("Scene cible")]
    [Tooltip("Nom exact de la scene a charger (Build Settings).")]
    public string nomScene = "Donjon";

    [Tooltip("Position ou le joueur apparaitra dans la scene cible.")]
    public Vector3 posTp;

    private bool declenche = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (declenche) return;
        if (!other.CompareTag(tagJoueur)) return;

        declenche = true;

        joueurSceneTransition jst = other.GetComponent<joueurSceneTransition>();

        GameObject runner = new GameObject("TransitionRunner");
        DontDestroyOnLoad(runner);
        TransitionRunner tr = runner.AddComponent<TransitionRunner>();
        tr.Lance(objetTransition, objetsACacher, duree, nomScene, posTp, other.gameObject, jst);
    }
}

public class TransitionRunner : MonoBehaviour
{
    private string nomScene;
    private Vector3 posTp;
    private GameObject joueur;
    private joueurSceneTransition jst;

    public void Lance(GameObject objetTransition, GameObject[] objetsACacher, float duree,
                      string nomScene, Vector3 posTp, GameObject joueur, joueurSceneTransition jst)
    {
        this.nomScene = nomScene;
        this.posTp = posTp;
        this.joueur = joueur;
        this.jst = jst;
        StartCoroutine(Executer(objetTransition, objetsACacher, duree, nomScene, joueur, jst, posTp));
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != nomScene) return;
        if (scene.name == "baseScene") return;
        if (joueur != null)
        {
            joueur.SetActive(true);
            joueur.transform.position = posTp;
        }
        Destroy(gameObject);
    }

    private IEnumerator Executer(GameObject objetTransition, GameObject[] objetsACacher, float duree,
                                  string nomScene, GameObject joueur, joueurSceneTransition jst,
                                  Vector3 posTp)
    {
        // Desactive la camera du joueur
        Camera cameraJoueur = joueur != null ? joueur.GetComponentInChildren<Camera>() : null;
        if (cameraJoueur != null)
            cameraJoueur.gameObject.SetActive(false);

        // Active le GameObject de transition
        if (objetTransition != null)
            objetTransition.SetActive(true);

        // Cache le joueur
        if (joueur != null)
            joueur.SetActive(false);

        // Cache le canvas HUD
        if (jst != null && jst.canvasHUD != null)
            jst.canvasHUD.SetActive(false);

        // Cache les objets supplementaires
        foreach (GameObject obj in objetsACacher)
            if (obj != null) obj.SetActive(false);

        yield return new WaitForSeconds(duree);

        // Remet la camera du joueur
        if (cameraJoueur != null)
            cameraJoueur.gameObject.SetActive(true);

        // Desactive le GameObject de transition
        if (objetTransition != null)
            objetTransition.SetActive(false);

        SaveManager.Save();
        SceneManager.LoadScene(nomScene);
    }
}
