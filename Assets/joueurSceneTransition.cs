using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// A placer sur le joueur (DontDestroyOnLoad).
/// Gere le passage du joueur et du canvas HUD entre les scenes.
/// </summary>
public class joueurSceneTransition : MonoBehaviour
{
    public static joueurSceneTransition Instance;

    [Tooltip("Nom exact de la scene du donjon.")]
    public string nomSceneDonjon = "Donjon";

    [Tooltip("Canvas HUD (vie, inventaire) a faire persister entre les scenes.")]
    public GameObject canvasHUD;

    [Tooltip("GameObject de transition a faire persister entre les scenes.")]
    public GameObject objetTransition;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasHUD != null)
            DontDestroyOnLoad(canvasHUD);

        if (objetTransition != null)
            DontDestroyOnLoad(objetTransition);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveManager.CleanupScene();

        if (EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(es);
        }

        if (objetTransition != null)
            objetTransition.SetActive(false);

        gameObject.SetActive(true);

        if (canvasHUD != null)
            canvasHUD.SetActive(true);

        if (scene.name == "baseScene") return;
        if (scene.name.IndexOf("donjon", System.StringComparison.OrdinalIgnoreCase) < 0) return;

        Debug.Log("[TP] OnSceneLoaded Donjon detectee, TP immediat...");

        // TP immediat (avant TransitionRunner et autres handlers)
        gridDonjon grid = FindAnyObjectByType<gridDonjon>();
        if (grid != null)
        {
            Vector2 spawn = grid.CelluleVersPosition(grid.largeur / 2, grid.hauteur / 2);
            transform.position = new Vector3(spawn.x, spawn.y, transform.position.z);
            PlayerTrail trail = GetComponent<PlayerTrail>();
            if (trail != null) trail.ResetTrail();
            Debug.Log($"[TP] TP immediat → {transform.position}");
        }
        else
        {
            Debug.LogWarning("[TP] gridDonjon introuvable au chargement");
        }

        StartCoroutine(AttendrePuisTeleporter());
    }

    private IEnumerator AttendrePuisTeleporter()
    {
        Vector2 spawn = Vector2.zero;

        // Attend jusqu'à 1 seconde que la grille soit prête
        for (int i = 0; i < 60; i++)
        {
            spawn = donjonGeneraaion.PositionSpawnJoueur;
            if (spawn.x != 0f || spawn.y != 0f) break;

            gridDonjon grid = FindAnyObjectByType<gridDonjon>();
            if (grid != null)
            {
                spawn = grid.CelluleVersPosition(grid.largeur / 2, grid.hauteur / 2);
                break;
            }

            yield return null;
        }

        // Attend que TransitionRunner.OnSceneLoaded ait fini (frame suivante)
        yield return null;

        Debug.Log($"[TP] Donjon spawn = {spawn}, actuelle = {transform.position}");

        if (spawn.x != 0f || spawn.y != 0f)
        {
            transform.position = new Vector3(spawn.x, spawn.y, transform.position.z);
            PlayerTrail trail = GetComponent<PlayerTrail>();
            if (trail != null) trail.ResetTrail();
            SaveManager.Save();
        }
        else
        {
            Debug.LogWarning("[TP] IMPOSSIBLE de trouver la position de spawn du donjon !");
        }
    }
}
