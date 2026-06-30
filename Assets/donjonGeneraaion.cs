using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;

public class donjonGeneraaion : MonoBehaviour
{
    [Header("References")]
    public gridDonjon grille;

    [Header("Prefabs de salles")]
    public GameObject[] prefabsSalles;

    [Header("Portes (sprites ouverte/fermee)")]
    public Sprite spritePorteOuverte;
    public Sprite spritePorteFermee;

    [Header("Portail")]
    public GameObject prefabPortail;
    public Transform pointBPortail;

    [Header("Transition")]
    public GameObject objetTransition;

    [Header("Murs exterieurs")]
    public GameObject prefabMur;

    [Header("Lumiere")]
    public bool ajouterOmbrePortees = true;
    public GameObject prefabLumiere;
    [Range(0, 4)] public int luminairesParSalle = 1;

    [Header("PNJ")]
    public GameObject prefabPNJ;

    [Header("Parametres de generation")]
    public int nombreSalles = 15;
    public int graine = 0;

    // Position de depart (salle de base)
    private Vector2Int posDepart;

    // Position monde de la salle de depart, accessible globalement pour le spawn du joueur
    public static Vector2 PositionSpawnJoueur { get; private set; }

    // Positions de toutes les salles normales placees
    private List<Vector2Int> positionsSalles = new List<Vector2Int>();

    // Liste des GameObjects instancies
    public List<GameObject> sallesInstanciees = new List<GameObject>();
    private List<GameObject> mursInstancies = new List<GameObject>();

    private void Start()
    {
        if (grille == null)
            grille = GetComponent<gridDonjon>();
        if (grille == null)
            grille = FindAnyObjectByType<gridDonjon>();

        MesureTailleSalle();
        Generer();
    }

    private void MesureTailleSalle()
    {
        if (prefabsSalles == null || prefabsSalles.Length == 0) return;
        if (grille == null) return;

        GameObject temp = Instantiate(prefabsSalles[0], Vector3.zero, Quaternion.identity);
        Tilemap[] tilemaps = temp.GetComponentsInChildren<Tilemap>();

        float maxX = 0f;
        float maxY = 0f;

        foreach (Tilemap tm in tilemaps)
        {
            tm.CompressBounds();
            float largeurTm = tm.cellBounds.size.x * tm.cellSize.x * Mathf.Abs(tm.transform.lossyScale.x);
            float hauteurTm = tm.cellBounds.size.y * tm.cellSize.y * Mathf.Abs(tm.transform.lossyScale.y);
            if (largeurTm > maxX) maxX = largeurTm;
            if (hauteurTm > maxY) maxY = hauteurTm;
        }

        DestroyImmediate(temp);

        if (maxX <= 0f || maxY <= 0f) return;

        grille.tailleCelluleX = maxX;
        grille.tailleCelluleY = maxY;
    }

    [ContextMenu("Generer le donjon")]
    public void Generer()
    {
        Nettoyer();

        if (grille == null) return;
        if (prefabsSalles == null || prefabsSalles.Length == 0) return;

        MesureTailleSalle();

        if (graine != 0)
            Random.InitState(graine);
        else
            Random.InitState(System.DateTime.Now.Millisecond);

        grille.Reinitialiser();
        positionsSalles.Clear();

        // Depart au centre
        int x = grille.largeur / 2;
        int y = grille.hauteur / 2;
        posDepart = new Vector2Int(x, y);

        // Expose la position monde du spawn pour le joueur
        PositionSpawnJoueur = grille.CelluleVersPosition(x, y);

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
        };

        int sallesPlacees = 0;
        int tentativesMax = nombreSalles * 20;
        int tentatives = 0;

        while (sallesPlacees < nombreSalles && tentatives < tentativesMax)
        {
            tentatives++;

            if (!grille.EstOccupee(x, y))
            {
                PlacerSalle(x, y);
                positionsSalles.Add(new Vector2Int(x, y));
                sallesPlacees++;
            }

            Vector2Int dir = directions[Random.Range(0, directions.Length)];
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (grille.EstDansLaGrille(nx, ny))
            {
                x = nx;
                y = ny;
            }
        }

        if (prefabPortail != null && positionsSalles.Count > 0)
        {
            Vector2Int derniere = positionsSalles[positionsSalles.Count - 1];

            GameObject salle = sallesInstanciees.Find(s => s.name == $"Salle_{derniere.x}_{derniere.y}");
            if (salle != null)
            {
                Tilemap tilemap = salle.GetComponent<Tilemap>();
                if (tilemap == null) tilemap = salle.GetComponentInChildren<Tilemap>();
                Vector2 centre = Vector2.zero;
                if (tilemap != null)
                {
                    tilemap.CompressBounds();
                    BoundsInt b = tilemap.cellBounds;
                    Vector3 cs = tilemap.cellSize;
                    centre = new Vector2((b.xMin + b.xMax) * 0.5f * cs.x, (b.yMin + b.yMax) * 0.5f * cs.y);
                }

                GameObject portail = Instantiate(prefabPortail, salle.transform);
                portail.transform.localPosition = centre;

                teleporteur tp = portail.GetComponent<teleporteur>();
                if (tp != null && pointBPortail != null)
                    tp.pointB = pointBPortail;
            }
        }

        foreach (GameObject salle in sallesInstanciees)
        {
            int sx, sy;
            if (!ExtraireCoordonnees(salle.name, out sx, out sy)) continue;
            RemplacerColliderParBox(salle, sx, sy);
        }


        // Reconnecte les triggerTransition instancies dynamiquement
        if (objetTransition != null)
        {
            foreach (triggerTransition tt in FindObjectsByType<triggerTransition>(FindObjectsSortMode.None))
            {
                if (tt.objetTransition == null)
                    tt.objetTransition = objetTransition;
            }
        }

        PlacerMursExterieurs();

        if (prefabPNJ != null && positionsSalles.Count > 0)
        {
            Vector2Int spawnCell = posDepart;
            Vector2Int farthest = positionsSalles[0];
            float maxDist = 0;
            foreach (var cell in positionsSalles)
            {
                float d = Vector2Int.Distance(cell, spawnCell);
                if (d > maxDist)
                {
                    maxDist = d;
                    farthest = cell;
                }
            }

            GameObject sallePNJ = sallesInstanciees.Find(s => s.name == $"Salle_{farthest.x}_{farthest.y}");
            if (sallePNJ != null)
            {
                Tilemap tilemap = sallePNJ.GetComponent<Tilemap>();
                if (tilemap == null) tilemap = sallePNJ.GetComponentInChildren<Tilemap>();
                Vector2 centre = Vector2.zero;
                if (tilemap != null)
                {
                    tilemap.CompressBounds();
                    BoundsInt b = tilemap.cellBounds;
                    Vector3 cs = tilemap.cellSize;
                    centre = new Vector2((b.xMin + b.xMax) * 0.5f * cs.x, (b.yMin + b.yMax) * 0.5f * cs.y);
                }
                GameObject pnj = Instantiate(prefabPNJ, sallePNJ.transform);
                pnj.transform.localPosition = centre;
            }
        }

        if (ajouterOmbrePortees)
        {
            foreach (GameObject salle in sallesInstanciees)
                AjouterOmbreEtLumieres(salle);
        }
    }

    private void RemplacerColliderParBox(GameObject salle, int x, int y)
    {
        Tilemap tilemap = salle.GetComponent<Tilemap>();
        if (tilemap == null) return;

        tilemap.CompressBounds();
        BoundsInt b = tilemap.cellBounds;
        Vector3 cs = tilemap.cellSize;

        float xMin = b.xMin * cs.x;
        float xMax = b.xMax * cs.x;
        float yMin = b.yMin * cs.y;
        float yMax = b.yMax * cs.y;

       
        
    }


    private bool ExtraireCoordonnees(string nom, out int x, out int y)
    {
        x = y = 0;
        int idx = nom.IndexOf(" (Clone)");
        if (idx >= 0) nom = nom.Substring(0, idx);
        string[] parts = nom.Split('_');
        if (parts.Length >= 3 && int.TryParse(parts[1], out x) && int.TryParse(parts[2], out y))
            return true;
        return false;
    }

   

    private void AjouterOmbreEtLumieres(GameObject salle)
    {
        TilemapCollider2D[] tilemapColliders = salle.GetComponentsInChildren<TilemapCollider2D>();
        foreach (var tc in tilemapColliders)
        {
            if (tc.GetComponent<ShadowCaster2D>() == null)
            {
                ShadowCaster2D sc = tc.gameObject.AddComponent<ShadowCaster2D>();
                sc.selfShadows = false;
            }
        }

        if (prefabLumiere != null && luminairesParSalle > 0)
        {
            Tilemap tilemap = salle.GetComponentInChildren<Tilemap>();
            if (tilemap != null)
            {
                tilemap.CompressBounds();
                BoundsInt b = tilemap.cellBounds;
                Vector3 cs = tilemap.cellSize;
                float xMin = b.xMin * cs.x;
                float xMax = b.xMax * cs.x;
                float yMin = b.yMin * cs.y;
                float yMax = b.yMax * cs.y;

                int count = Random.Range(1, luminairesParSalle + 1);
                for (int i = 0; i < count; i++)
                {
                    float px = Random.Range(xMin + 1f, xMax - 1f);
                    float py = Random.Range(yMin + 1f, yMax - 1f);
                    GameObject lumiere = Instantiate(prefabLumiere, salle.transform);
                    lumiere.transform.localPosition = new Vector3(px, py, 0);
                }
            }
        }
    }

    private void PlacerMursExterieurs()
    {
        if (prefabMur == null) return;

        for (int x = 0; x < grille.largeur; x++)
        {
            for (int y = 0; y < grille.hauteur; y++)
            {
                if (!grille.EstOccupee(x, y))
                {
                    grille.OccuperCellule(x, y);

                    Vector2 posMonde = grille.CelluleVersPosition(x, y);
                    GameObject mur = Instantiate(prefabMur, posMonde, Quaternion.identity, transform);
                    mur.name = $"Mur_{x}_{y}";
                    mursInstancies.Add(mur);
                }
            }
        }

        for (int x = -1; x <= grille.largeur; x++)
        {
            int bas = -1;
            if (!grille.EstDansLaGrille(x, bas))
            {
                Vector2 posMonde = grille.CelluleVersPosition(x, bas);
                GameObject mur = Instantiate(prefabMur, posMonde, Quaternion.identity, transform);
                mur.name = $"MurPerimeter_{x}_{bas}";
                mursInstancies.Add(mur);
            }

            int haut = grille.hauteur;
            if (!grille.EstDansLaGrille(x, haut))
            {
                Vector2 posMonde = grille.CelluleVersPosition(x, haut);
                GameObject mur = Instantiate(prefabMur, posMonde, Quaternion.identity, transform);
                mur.name = $"MurPerimeter_{x}_{haut}";
                mursInstancies.Add(mur);
            }
        }

        for (int y = 0; y < grille.hauteur; y++)
        {
            int gauche = -1;
            if (!grille.EstDansLaGrille(gauche, y))
            {
                Vector2 posMonde = grille.CelluleVersPosition(gauche, y);
                GameObject mur = Instantiate(prefabMur, posMonde, Quaternion.identity, transform);
                mur.name = $"MurPerimeter_{gauche}_{y}";
                mursInstancies.Add(mur);
            }

            int droite = grille.largeur;
            if (!grille.EstDansLaGrille(droite, y))
            {
                Vector2 posMonde = grille.CelluleVersPosition(droite, y);
                GameObject mur = Instantiate(prefabMur, posMonde, Quaternion.identity, transform);
                mur.name = $"MurPerimeter_{droite}_{y}";
                mursInstancies.Add(mur);
            }
        }
    }

    private void AjusterOrderInLayer(GameObject salle, int y)
    {
        int decalage = -y;

        TilemapRenderer[] tilemaps = salle.GetComponentsInChildren<TilemapRenderer>();
        foreach (TilemapRenderer tmr in tilemaps)
            tmr.sortingOrder += decalage;

        SpriteRenderer[] sprites = salle.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in sprites)
            sr.sortingOrder += decalage;
    }

    private void PlacerSalle(int x, int y)
    {
        grille.OccuperCellule(x, y);

        Vector2 position = grille.CelluleVersPosition(x, y);
        GameObject prefab = prefabsSalles[Random.Range(0, prefabsSalles.Length)];
        GameObject salle = Instantiate(prefab, position, Quaternion.identity, transform);
        salle.name = $"Salle_{x}_{y}";

        foreach (Transform enfant in salle.GetComponentsInChildren<Transform>(true))
            enfant.gameObject.SetActive(true);

        AjusterOrderInLayer(salle, y);

        sallesInstanciees.Add(salle);
    }

    [ContextMenu("Nettoyer le donjon")]
    public void Nettoyer()
    {
        foreach (GameObject salle in sallesInstanciees)
        {
            if (salle != null)
                DestroyImmediate(salle);
        }
        sallesInstanciees.Clear();

        foreach (GameObject mur in mursInstancies)
        {
            if (mur != null)
                DestroyImmediate(mur);
        }
        mursInstancies.Clear();

        positionsSalles.Clear();

        if (grille != null)
            grille.Reinitialiser();
    }
}
