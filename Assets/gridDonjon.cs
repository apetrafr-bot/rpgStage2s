using UnityEngine;

/// <summary>
/// Represente la grille du donjon.
/// Chaque cellule peut contenir une salle (prefab tilemap 9x11).
/// </summary>
public class gridDonjon : MonoBehaviour
{
    [Header("Dimensions de la grille")]
    public int largeur = 10;   // nombre de cellules en X
    public int hauteur = 10;   // nombre de cellules en Y

    [Header("Taille d'une salle (en unites Unity)")]
    public float tailleCelluleX = 9f;
    public float tailleCelluleY = 11f;

    // Tableau 2D indiquant si une cellule est occupee par une salle
    private bool[,] cellules;

    private void Awake()
    {
        cellules = new bool[largeur, hauteur];
    }

    /// <summary>
    /// Retourne true si la position (x, y) est dans les limites de la grille.
    /// </summary>
    public bool EstDansLaGrille(int x, int y)
    {
        return x >= 0 && x < largeur && y >= 0 && y < hauteur;
    }

    /// <summary>
    /// Retourne true si la cellule (x, y) est deja occupee.
    /// </summary>
    public bool EstOccupee(int x, int y)
    {
        if (!EstDansLaGrille(x, y)) return true;
        return cellules[x, y];
    }

    /// <summary>
    /// Marque la cellule (x, y) comme occupee.
    /// </summary>
    public void OccuperCellule(int x, int y)
    {
        if (EstDansLaGrille(x, y))
            cellules[x, y] = true;
    }

    /// <summary>
    /// Remet toutes les cellules a vide (pour regenerer le donjon).
    /// </summary>
    public void Reinitialiser()
    {
        cellules = new bool[largeur, hauteur];
    }

    /// <summary>
    /// Convertit une position de cellule (x, y) en position mondiale Unity.
    /// </summary>
    public Vector2 CelluleVersPosition(int x, int y)
    {
        float posX = transform.position.x + x * tailleCelluleX;
        float posY = transform.position.y + y * tailleCelluleY;
        return new Vector2(posX, posY);
    }

    /// <summary>
    /// Retourne true si la position monde est dans les limites du donjon (avec une marge d'une demi-cellule).
    /// </summary>
    public bool EstDansLeDonjon(Vector2 positionMonde)
    {
        Vector2 origine = transform.position;
        float margeX = tailleCelluleX * 0.5f;
        float margeY = tailleCelluleY * 0.5f;
        return positionMonde.x >= origine.x - margeX
            && positionMonde.x <= origine.x + largeur * tailleCelluleX + margeX
            && positionMonde.y >= origine.y - margeY
            && positionMonde.y <= origine.y + hauteur * tailleCelluleY + margeY;
    }

    /// <summary>
    /// Affiche la grille dans l'editeur (Gizmos).
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);
        for (int x = 0; x < largeur; x++)
        {
            for (int y = 0; y < hauteur; y++)
            {
                Vector2 centre = new Vector2(
                    transform.position.x + x * tailleCelluleX + tailleCelluleX * 0.5f,
                    transform.position.y + y * tailleCelluleY + tailleCelluleY * 0.5f
                );
                Gizmos.DrawWireCube(centre, new Vector3(tailleCelluleX, tailleCelluleY, 0));

                if (cellules != null && cellules[x, y])
                {
                    Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.4f);
                    Gizmos.DrawCube(centre, new Vector3(tailleCelluleX * 0.9f, tailleCelluleY * 0.9f, 0));
                    Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);
                }
            }
        }
    }
}
