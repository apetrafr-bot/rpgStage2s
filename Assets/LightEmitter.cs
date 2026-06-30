using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    public float radius = 4f;
    public float intensity = 1f;

    public bool flicker = true;
    public float flickerSpeed = 6f;
    public float flickerAmount = 0.2f;

    private float currentRadius;
    private float currentIntensity;
    private float seed;

    void Start()
    {
        seed = Random.Range(0f, 100f);
        currentRadius = radius;
        currentIntensity = intensity;
    }

    void Update()
    {
        if (flicker)
        {
            float f = Mathf.PerlinNoise(seed + Time.time * flickerSpeed, 0f);
            currentIntensity = intensity * Mathf.Lerp(1f - flickerAmount, 1f + flickerAmount, f);
            currentRadius = radius * Mathf.Lerp(1f - flickerAmount * 0.3f, 1f + flickerAmount * 0.3f, f);
        }
    }

    public float GetCurrentRadius() => currentRadius;
    public float GetCurrentIntensity() => currentIntensity;
}
