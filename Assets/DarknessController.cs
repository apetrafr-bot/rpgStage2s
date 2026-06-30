using UnityEngine;
using UnityEngine.SceneManagement;

public class DarknessController : MonoBehaviour
{
    [Header("Darkness")]
    public Color darknessColor = new Color(0, 0, 0, 1);
    public string sortingLayerName = "Default";
    public int sortingOrder = 10000;

    [Header("Torch du joueur")]
    public TileClass torchItem;
    public float torchRadius = 5f;
    public float torchIntensity = 1f;

    public Material darknessMaterial;
    private Material darknessMat;
    private SpriteRenderer overlay;
    private Camera cam;
    private HotBar hotBar;
    private bool isDonjon = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        hotBar = FindFirstObjectByType<HotBar>();

        Shader shader = Shader.Find("Custom/DarknessOverlay");
        if (shader == null && darknessMaterial != null)
            shader = darknessMaterial.shader;

        if (shader == null)
        {
            Debug.LogError("DarknessOverlay shader not found!");
            enabled = false;
            return;
        }

        darknessMat = new Material(shader);
        darknessMat.SetColor("_DarknessColor", darknessColor);

        GameObject overlayGO = new GameObject("DarknessOverlay");
        overlayGO.transform.SetParent(transform);
        overlayGO.transform.localPosition = new Vector3(0, 0, 5);
        overlayGO.transform.localScale = Vector3.one;

        overlay = overlayGO.AddComponent<SpriteRenderer>();
        overlay.sprite = CreateFullScreenSprite();
        overlay.material = darknessMat;
        overlay.sortingLayerName = sortingLayerName;
        overlay.sortingOrder = sortingOrder;
        overlay.color = Color.white;

        SceneManager.sceneLoaded += OnSceneLoaded;
        isDonjon = SceneManager.GetActiveScene().name.Contains("Donjon");
        overlay.gameObject.SetActive(isDonjon);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isDonjon = scene.name.Contains("Donjon");
        if (overlay != null)
            overlay.gameObject.SetActive(isDonjon);
    }

    Sprite CreateFullScreenSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }

    void LateUpdate()
    {
        if (darknessMat == null || !isDonjon) return;

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;
        overlay.transform.localScale = new Vector3(worldWidth, worldHeight, 1);

        bool hasLight = false;
        bool hasLightBuff = false;
        Vector3 playerPos = Vector3.zero;
        playerMove player = FindFirstObjectByType<playerMove>();
        playerAttack attack = FindFirstObjectByType<playerAttack>();

        if (player != null)
            playerPos = player.transform.position;

        if (attack != null)
            hasLightBuff = attack.hasLightBuff;

        if (hotBar != null && torchItem != null)
        {
            TileClass selected = hotBar.GetSelectedItem();
            hasLight = (selected == torchItem) || hasLightBuff;
        }
        else
        {
            hasLight = hasLightBuff;
        }

        darknessMat.SetVector("_PlayerPos", hasLight ? (Vector4)playerPos : new Vector4(-9999, -9999, 0, 0));
        darknessMat.SetFloat("_TorchRadius", torchRadius);
        darknessMat.SetFloat("_TorchIntensity", hasLight ? torchIntensity : 0);

        LightEmitter[] emitters = FindObjectsByType<LightEmitter>(FindObjectsSortMode.None);
        int count = Mathf.Min(emitters.Length, 32);
        darknessMat.SetInt("_LightCount", count);

        Vector4[] lightData = new Vector4[32];
        for (int i = 0; i < count; i++)
        {
            Vector3 p = emitters[i].transform.position;
            lightData[i] = new Vector4(p.x, p.y, emitters[i].GetCurrentRadius(), emitters[i].GetCurrentIntensity());
        }
        darknessMat.SetVectorArray("_LightData", lightData);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (darknessMat != null)
            Destroy(darknessMat);
    }
}
