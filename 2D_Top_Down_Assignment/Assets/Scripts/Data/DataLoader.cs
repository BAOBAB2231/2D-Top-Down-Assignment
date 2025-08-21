using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DataLoader
{
    /// <summary>
    /// 루트가 배열([])이면 {"items": ...} 로 감싸 JsonUtility가 파싱 가능하게 만든다.
    /// </summary>
    private static string WrapIfArray(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{\"items\":[]}";
        string t = raw.TrimStart();
        if (t.StartsWith("[")) return "{\"items\":" + raw + "}";
        return raw;
    }

    /// <summary>
    /// 특정 키(예: "Monster", "Item") 뒤에 오는 최상위 배열을 찾아 {"items":[...]}로 감싼다.
    /// JSON 구조가 { "Monster":[ ... ] } 처럼 객체 루트인 경우에 사용.
    /// </summary>
    private static string ExtractArrayAndWrap(string raw, params string[] keys)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{\"items\":[]}";
        foreach (var key in keys)
        {
            // "Monster" 또는 'Monster' 뒤에 나오는 최초의 [ ... ] 범위를 찾는다.
            var match = Regex.Match(raw, $"\"{key}\"\\s*:\\s*\\[", RegexOptions.IgnoreCase);
            if (!match.Success) continue;

            int start = match.Index;
            int bracketStart = raw.IndexOf('[', start);
            if (bracketStart < 0) continue;

            // 대괄호 짝 맞추기
            int depth = 0;
            for (int i = bracketStart; i < raw.Length; i++)
            {
                char c = raw[i];
                if (c == '[') depth++;
                else if (c == ']')
                {
                    depth--;
                    if (depth == 0)
                    {
                        int bracketEnd = i;
                        string arr = raw.Substring(bracketStart, bracketEnd - bracketStart + 1);
                        return "{\"items\":" + arr + "}";
                    }
                }
            }
        }
        return raw; // 못 찾으면 원본 그대로
    }

    public static bool TryLoadFromResources<TTable>(string resourcePath, out TTable table) where TTable : class
    {
        table = null;
        TextAsset ta = Resources.Load<TextAsset>(resourcePath);
        if (ta == null)
        {
            Debug.LogWarning($"[DataLoader] Resources.Load 실패: {resourcePath}");
            return false;
        }

        string json = ta.text;
        if (typeof(TTable) == typeof(MonsterTable))
            json = ExtractArrayAndWrap(json, "Monster", "Monsters", "items");
        else if (typeof(TTable) == typeof(ItemTable))
            json = ExtractArrayAndWrap(json, "Item", "Items", "items");

        json = WrapIfArray(json);

        try
        {
            table = JsonUtility.FromJson<TTable>(json);
            return table != null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DataLoader] Resources 파싱 실패: {e.Message}");
            return false;
        }
    }

    public static bool TryLoadFromStreamingAssets<TTable>(string relativePath, out TTable table) where TTable : class
    {
        table = null;
        try
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[DataLoader] 파일 없음: {fullPath}");
                return false;
            }

            string raw = File.ReadAllText(fullPath);

            string json = raw;
            if (typeof(TTable) == typeof(MonsterTable))
                json = ExtractArrayAndWrap(raw, "Monster", "Monsters", "items");
            else if (typeof(TTable) == typeof(ItemTable))
                json = ExtractArrayAndWrap(raw, "Item", "Items", "items");

            json = WrapIfArray(json);

            table = JsonUtility.FromJson<TTable>(json);
            return table != null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DataLoader] StreamingAssets 로드/파싱 실패: {e.Message}");
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
                var parts = m.DropItemStr.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var ids = new List<int>();
                foreach (var p in parts)
                    if (int.TryParse(p.Trim(), out int id)) ids.Add(id);
                m.DropItem = ids.ToArray();
            }
        }
    }
}