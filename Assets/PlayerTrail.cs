using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTrail : MonoBehaviour
{
    public GameObject trailPrefab;
    public float distanceBetween = 0.5f;
    public float fadeDuration = 10f;

    private Vector3 lastPos;

    void Awake()
    {
        lastPos = transform.position;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lastPos = transform.position;
    }

    public void ResetTrail()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        if (trailPrefab == null) return;

        float dist = Vector3.Distance(transform.position, lastPos);
        if (dist >= distanceBetween)
        {
            Vector3 dir = (transform.position - lastPos).normalized;
            Vector3 spawnPos = lastPos + dir * distanceBetween;

            GameObject trail = Instantiate(trailPrefab, spawnPos, Quaternion.identity);
            SpriteRenderer sr = trail.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);

            StartCoroutine(FondreEtSuprimmer(trail));

            lastPos = spawnPos;
        }
    }

    IEnumerator FondreEtSuprimmer(GameObject trail)
    {
        SpriteRenderer sr = trail.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < fadeDuration)
        {
            if (sr == null) yield break;
            float t = elapsed / fadeDuration;
            sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(trail);
    }
}
