using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int currentHealth;
    public int maxHealth;
    public float playerPosX, playerPosY;
    public string currentScene;
    public List<SavedStack> inventoryStacks = new List<SavedStack>();
    public int hotBarSelectedIndex;
    public bool questCompleted;
    public List<string> npcsAlreadyTalkedTo = new List<string>();
    public List<string> collectedItems = new List<string>();
}

[System.Serializable]
public class SavedStack
{
    public string itemName;
    public int count;
}

public static class SaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static HashSet<string> collectedItems = new HashSet<string>();

    public static void RegisterCollectedItem(Vector3 pos)
    {
        collectedItems.Add(ItemKey(pos));
    }

    public static void CleanupScene()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            if (item != null && collectedItems.Contains(ItemKey(item.transform.position)))
                GameObject.Destroy(item);
        }
    }

    private static string ItemKey(Vector3 pos)
    {
        return $"{pos.x:F1},{pos.y:F1}";
    }

    public static void Save()
    {
        SaveData data = new SaveData();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            data.playerPosX = pos.x;
            data.playerPosY = pos.y;

            playerHealth health = player.GetComponent<playerHealth>();
            if (health != null)
            {
                data.currentHealth = health.currentHealth;
                data.maxHealth = health.maxHealth;
            }

            inventory inv = inventory.Instance;
            if (inv != null)
            {
                foreach (var stack in inv.GetStacks())
                {
                    if (stack != null && stack.item != null && stack.count > 0)
                        data.inventoryStacks.Add(new SavedStack { itemName = stack.item.name, count = stack.count });
                }
            }

            HotBar hotbar = player.GetComponentInChildren<HotBar>();
            if (hotbar != null)
                data.hotBarSelectedIndex = hotbar.GetSelectedIndex();
        }

        data.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (questManager.Instance != null)
            data.questCompleted = questManager.Instance.EstQuestCompletee();

        data.npcsAlreadyTalkedTo.AddRange(pnjManager.DejaParleList());
        data.collectedItems.AddRange(collectedItems);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Partie sauvegardee dans " + SavePath);
    }

    public static bool Load()
    {
        if (!File.Exists(SavePath)) return false;

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null) return false;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Debug.Log($"[SaveManager.Load] Position chargée = ({data.playerPosX}, {data.playerPosY})");
            player.transform.position = new Vector3(data.playerPosX, data.playerPosY, 0f);

            playerHealth health = player.GetComponent<playerHealth>();
            if (health != null)
            {
                health.maxHealth = data.maxHealth;
                health.currentHealth = data.currentHealth;
            }

            inventory inv = inventory.Instance;
            if (inv != null)
            {
                inv.ClearInventory();
                foreach (var s in data.inventoryStacks)
                {
                    TileClass item = FindTileClassByName(s.itemName);
                    if (item != null)
                        inv.AddItem(item, s.count);
                }
            }

            HotBar hotbar = player.GetComponentInChildren<HotBar>();
            if (hotbar != null)
                hotbar.SetSelectedIndex(data.hotBarSelectedIndex);
        }

        if (questManager.Instance != null)
            questManager.Instance.SetQuestCompleted(data.questCompleted);

        pnjManager.SetDejaParle(data.npcsAlreadyTalkedTo);

        collectedItems.Clear();
        foreach (var key in data.collectedItems)
            collectedItems.Add(key);

        CleanupScene();

        Debug.Log("Partie chargee depuis " + SavePath);
        return true;
    }

    public static bool SaveExists()
    {
        return File.Exists(SavePath);
    }

    private static Dictionary<string, TileClass> tileClassCache = null;

    private static TileClass FindTileClassByName(string name)
    {
        if (tileClassCache == null)
        {
            tileClassCache = new Dictionary<string, TileClass>();
            TileClass[] all = Resources.FindObjectsOfTypeAll<TileClass>();
            foreach (var t in all)
            {
                if (t != null && !tileClassCache.ContainsKey(t.name))
                    tileClassCache[t.name] = t;
            }
            TileClass[] loaded = Resources.LoadAll<TileClass>("");
            foreach (var t in loaded)
            {
                if (t != null && !tileClassCache.ContainsKey(t.name))
                    tileClassCache[t.name] = t;
            }
        }
        tileClassCache.TryGetValue(name, out TileClass result);
        return result;
    }
}
