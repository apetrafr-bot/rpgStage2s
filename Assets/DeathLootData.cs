using System.Collections.Generic;
using UnityEngine;

public static class DeathLootData
{
    public static bool hasLoot = false;
    public static List<inventory.ItemStack> items = new List<inventory.ItemStack>();
    public static bool fromDonjon = false;
    public static Vector3 tombPosition;
    public static string tombSceneName;
    public static List<string> tombMessages = new List<string>();

    public static void Save(List<inventory.ItemStack> loot, bool donjon)
    {
        items.Clear();
        foreach (var s in loot)
        {
            if (s != null && s.item != null)
                items.Add(new inventory.ItemStack(s.item, s.count));
        }
        hasLoot = items.Count > 0;
        fromDonjon = donjon;
    }

    public static List<TileClass> GetTileClassList()
    {
        List<TileClass> list = new List<TileClass>();
        foreach (var s in items)
        {
            if (s != null && s.item != null)
            {
                for (int i = 0; i < s.count; i++)
                    list.Add(s.item);
            }
        }
        return list;
    }

    public static void Clear()
    {
        items.Clear();
        hasLoot = false;
        fromDonjon = false;
        tombPosition = Vector3.zero;
        tombSceneName = null;
        tombMessages.Clear();
    }
}
