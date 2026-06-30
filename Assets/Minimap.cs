using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Minimap : MonoBehaviour
{
    [Header("Taille / Apparence")]
    public float zoom = 40f;
    public int resolution = 256;
    public Color fondCouleur = new Color(0.1f, 0.1f, 0.1f);

    [Header("UI")]
    public Vector2 position = new Vector2(-10, -10);
    public Vector2 taille = new Vector2(200, 200);

    private Camera minimapCamera;
    private RenderTexture rt;
    private RawImage image;
    private GameObject canvas;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        Setup();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Setup();
    }

    void Setup()
    {
        TrouverCanvas();

        if (image == null)
            CreerImage();

        if (rt == null || !rt.IsCreated())
            CreerRenderTexture();

        if (minimapCamera == null)
            CreerCamera();

        image.texture = rt;
        minimapCamera.targetTexture = rt;
    }

    void TrouverCanvas()
    {
        GameObject prev = canvas;

        canvas = GameObject.FindWithTag("CanvasHUD");
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>()?.gameObject;

        if (prev != canvas && prev != null)
            Debug.Log("[Minimap] Canvas changé");
    }

    void CreerImage()
    {
        if (canvas == null) return;

        GameObject go = new GameObject("Minimap_Image", typeof(RawImage));
        go.transform.SetParent(canvas.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = position;
        rt.sizeDelta = taille;

        image = go.GetComponent<RawImage>();
        image.raycastTarget = false;
    }

    void CreerRenderTexture()
    {
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
        }

        rt = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32);
        rt.name = "MinimapRT";
        rt.Create();
    }

    void CreerCamera()
    {
        GameObject go = new GameObject("Minimap_Camera");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        minimapCamera = go.AddComponent<Camera>();
        DestroyImmediate(go.GetComponent<AudioListener>());
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = zoom;
        minimapCamera.cullingMask = ~(1 << 5);
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = fondCouleur;
        minimapCamera.depth = -100;

        if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null)
        {
            var udata = go.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            udata.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Base;
        }
    }

    void OnDestroy()
    {
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
        }
    }
}
