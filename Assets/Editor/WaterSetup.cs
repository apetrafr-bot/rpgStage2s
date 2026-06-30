using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Menu Tools > Create Water (Pixel Art)
/// 1. Applique WaterPixelArt.shader (HLSL)
/// 2. Crée un matériau WaterPixelArt.mat
/// 3. Ajoute un GameObject "Water" dans la scène active
/// </summary>
public static class WaterSetup
{
    const string SHADER_PATH = "Assets/WaterPixelArt.shader";
    const string MAT_PATH    = "Assets/WaterPixelArt.mat";

    [MenuItem("Tools/Create Water (Pixel Art)")]
    static void CreateWater()
    {
        Material mat = CreateMaterial();
        CreateWaterGameObject(mat);
        AssetDatabase.Refresh();
        Debug.Log("[WaterSetup] Terminé ! Sélectionne le GameObject 'Water' et ajuste sa taille.");
    }

    // ─────────────────────────────────────────────────────────────
    //  Matériau
    // ─────────────────────────────────────────────────────────────
    static Material CreateMaterial()
    {
        Shader shader = Shader.Find("Custom/WaterPixelArt");

        if (shader == null)
        {
            Debug.LogError("[WaterSetup] Shader 'Custom/WaterPixelArt' introuvable ! Vérifie que WaterPixelArt.shader est bien dans Assets/.");
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MAT_PATH);
        if (mat == null)
        {
            mat = new Material(shader);
            mat.SetColor("_WaterColorA", new Color(0.05f, 0.28f, 0.55f, 1f));
            mat.SetColor("_WaterColorB", new Color(0.18f, 0.58f, 0.88f, 1f));
            mat.SetColor("_FoamColor",   new Color(0.85f, 0.95f, 1.00f, 1f));
            mat.SetFloat("_WaveSpeed",   1.5f);
            mat.SetFloat("_WaveFreqX",   6.0f);
            mat.SetFloat("_WaveFreqY",   3.0f);
            mat.SetFloat("_WaveAmp",     0.08f);
            mat.SetFloat("_PixelSize",   24.0f);
            mat.SetFloat("_FoamThresh",  0.72f);
            AssetDatabase.CreateAsset(mat, MAT_PATH);
            Debug.Log("[WaterSetup] WaterPixelArt.mat créé.");
        }
        else
        {
            mat.shader = shader;
            EditorUtility.SetDirty(mat);
            Debug.Log("[WaterSetup] WaterPixelArt.mat mis à jour.");
        }
        return mat;
    }

    // ─────────────────────────────────────────────────────────────
    //  GameObject
    // ─────────────────────────────────────────────────────────────
    static void CreateWaterGameObject(Material mat)
    {
        GameObject existing = GameObject.Find("Water");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
            Debug.Log("[WaterSetup] Ancien GameObject 'Water' remplacé.");
        }

        GameObject water = new GameObject("Water");
        water.transform.position   = Vector3.zero;
        water.transform.localScale = new Vector3(50f, 50f, 1f);

        SpriteRenderer sr = water.AddComponent<SpriteRenderer>();
        sr.sprite       = GetOrCreateWhiteSprite();
        sr.material     = mat;
        sr.sortingOrder = -10;

        Undo.RegisterCreatedObjectUndo(water, "Create Water");
        Selection.activeGameObject = water;

        Debug.Log("[WaterSetup] GameObject 'Water' créé. Ajuste Scale dans le Transform pour couvrir ta tilemap.");
    }

    // Sprite blanc 1×1 — sert de base au shader
    static Sprite GetOrCreateWhiteSprite()
    {
        const string path = "Assets/WaterWhite.png";

        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        File.WriteAllBytes(Path.GetFullPath(path), tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null)
        {
            imp.textureType        = TextureImporterType.Sprite;
            imp.spriteImportMode   = SpriteImportMode.Single;
            imp.filterMode         = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
