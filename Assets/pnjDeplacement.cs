using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class pnjDeplacement : MonoBehaviour
{
    public List<GameObject> waitPoint = new List<GameObject>();
    public float speed = 1f;
    public float tempsAttente = 2f;

    private int indexCourant = 0;
    private bool enAttente = false;
    private pnjManager pnjMgr;

    private void Awake()
    {
        pnjMgr = GetComponent<pnjManager>();
    }

    public void Update()
    {
        // Bloque le deplacement si le PNJ est en interaction
        if (pnjMgr != null && pnjMgr.isOpen) return;

        if (waitPoint.Count == 0 || enAttente) return;

        Transform cible = waitPoint[indexCourant].transform;
        transform.position = Vector3.MoveTowards(transform.position, cible.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, cible.position) < 0.1f)
            StartCoroutine(AttendrePuisSuivant());
    }

    private IEnumerator AttendrePuisSuivant()
    {
        enAttente = true;
        yield return new WaitForSeconds(tempsAttente);
        indexCourant = (indexCourant + 1) % waitPoint.Count;
        enAttente = false;
    }
}
