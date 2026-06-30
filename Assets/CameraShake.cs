using System.Collections;
using UnityEngine;

/// <summary>
/// Attache ce script à la caméra principale.
/// Appelle CameraShake.Instance.Shake(duration, magnitude) pour faire trembler.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <param name="duration">Durée du tremblement en secondes</param>
    /// <param name="magnitude">Amplitude du déplacement (ex: 0.3f)</param>
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 origin = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(origin.x + x, origin.y + y, origin.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = origin;
    }
}
