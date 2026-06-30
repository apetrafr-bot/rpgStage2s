using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class portailDonjon : MonoBehaviour
{
    [Header("Scene")]
    public string nomSceneDonjon = "Donjon";
    public string tagJoueur = "Player";

    [Header("Assombrissement")]
    public float distanceAssombrissement = 5f;
    public float opaciteMax = 0.5f;

    private Transform joueur;
    private Light2D lumiere;

    void Start()
    {
        GameObject p = GameObject.FindWithTag(tagJoueur);
        if (p != null) joueur = p.transform;

        GameObject go = new GameObject("PortalDarkness");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        lumiere = go.AddComponent<Light2D>();
        lumiere.lightType = Light2D.LightType.Global;
        lumiere.blendStyleIndex = 0;
        lumiere.intensity = 0f;
        lumiere.color = Color.black;
    }

    void Update()
    {
        if (joueur == null || lumiere == null) return;

        float dist = Vector2.Distance(transform.position, joueur.position);
        float t = 1f - Mathf.Clamp01(dist / distanceAssombrissement);
        lumiere.intensity = t * opaciteMax;
    }

    private void OnDestroy()
    {
        if (lumiere != null && lumiere.gameObject != null)
            Destroy(lumiere.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(tagJoueur)) return;

        SaveManager.Save();
        SceneManager.LoadScene(nomSceneDonjon);
    }
}
