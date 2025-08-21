using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DataLoader
{
    private static string WrapIfArray(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{\"items\":[]}";
        string t = raw.TrimStart();
        if (t.StartsWith("[")) return "{\"items\":" + raw + "}";
        return raw; // 이미 객체면 그대로
    }

    public static bool TryLoadFromResources<TTable>(string resourcePath, out TTable table) where TTable : class
    {
        table = null;
        TextAsset ta = Resources.Load<TextAsset>(resourcePath);
        if (ta == null) return false;

        string json = WrapIfArray(ta.text);
        table = JsonUtility.FromJson<TTable>(json);
        return table != null;
    }

    public static bool TryLoadFromStreamingAssets<TTable>(string relativePath, out TTable table) where TTable : class
    {
        table = null;
        try
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            if (!File.Exists(fullPath)) return false;

            string raw = File.ReadAllText(fullPath);
            string json = WrapIfArray(raw);
            table = JsonUtility.FromJson<TTable>(json);
            return table != null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DataLoader] StreamingAssets load failed: {e.Message}");
            return false;
        }
    }

    public static void FixupDropItems(List<MonsterRecord> list)
    {
        if (list == null) return;
        foreach (var m in list)
        {
            if ((m.DropItem == null || m.DropItem.Length == 0) && !string.IsNullOrWhiteSpace(m.DropItemStr))
            {
                string[] parts = m.DropItemStr.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> ids = new List<int>();
                foreach (var p in parts)
                    if (int.TryParse(p.Trim(), out int id)) ids.Add(id);
                m.DropItem = ids.ToArray();
            }
        }
    }
}