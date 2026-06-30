using UnityEngine;
using UnityEditor;

public class CreateTorchItem
{
    [MenuItem("Tools/Create Torch Item")]
    static void CreateTorch()
    {
        TileClass torch = ScriptableObject.CreateInstance<TileClass>();
        torch.tileName = "Torche";
        torch.isStakable = true;
        torch.maxStack = 10;
        torch.autoPickup = true;
        torch.description = "Une torche pour eclairer les tenebres";

        string path = "Assets/Torche.asset";
        AssetDatabase.CreateAsset(torch, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Torch item created at {path}. Assign a sprite and prefab in the Inspector.");
    }

    [MenuItem("Tools/Create Light Emitter Prefab")]
    static void CreateLightEmitterPrefab()
    {
        GameObject go = new GameObject("LightEmitter");
        go.AddComponent<LightEmitter>();

        string path = "Assets/LightEmitter.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        Debug.Log($"Light emitter prefab created at {path}");
    }

    [MenuItem("Tools/Create Light Potion")]
    static void CreateLightPotion()
    {
        TileClass potion = ScriptableObject.CreateInstance<TileClass>();
        potion.tileName = "Potion de Lumiere";
        potion.isConsumable = true;
        potion.isStakable = true;
        potion.maxStack = 10;
        potion.autoPickup = true;
        potion.lightBoostDuration = 20f;
        potion.description = "Donne de la lumiere pendant 20 secondes";

        string path = "Assets/LightPotion.asset";
        AssetDatabase.CreateAsset(potion, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Light potion created at {path}. Assign a sprite and prefab in the Inspector.");
    }
}
