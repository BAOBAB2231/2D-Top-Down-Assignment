using System.Collections.Generic;
using UnityEngine;

public class DataRepository : MonoBehaviour
{
    public static DataRepository Instance { get; private set; }

    [Header("Load Mode")]
    [SerializeField] private bool loadFromResources = true;

    [Header("Resources 경로 (확장자 제외)")]
    [SerializeField] private string monsterResourcePath = "Data/Monster";
    [SerializeField] private string itemResourcePath = "Data/Item";

    [Header("StreamingAssets 상대 경로 (확장자 포함)")]
    [SerializeField] private string monsterStreamingPath = "Data/Monster.json";
    [SerializeField] private string itemStreamingPath = "Data/Item.json";

    public IReadOnlyList<MonsterRecord> Monsters => monsters;
    public IReadOnlyList<ItemRecord> Items => items;

    private List<MonsterRecord> monsters = new List<MonsterRecord>();
    private List<ItemRecord> items = new List<ItemRecord>();
    private Dictionary<string, MonsterRecord> monsterById = new Dictionary<string, MonsterRecord>();
    private Dictionary<int, ItemRecord> itemById = new Dictionary<int, ItemRecord>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAll();
    }

    public void LoadAll()
    {
        monsters.Clear(); items.Clear();
        monsterById.Clear(); itemById.Clear();

        // ---- Monster ----
        MonsterTable mTable;
        bool mOk = loadFromResources
            ? DataLoader.TryLoadFromResources<MonsterTable>(monsterResourcePath, out mTable)
            : DataLoader.TryLoadFromStreamingAssets<MonsterTable>(monsterStreamingPath, out mTable);

        if (mOk && mTable != null)
        {
            var list = mTable.GetList();
            DataLoader.FixupDropItems(list);
            monsters.AddRange(list);
            foreach (var m in monsters)
                if (!monsterById.ContainsKey(m.MonsterID))
                    monsterById.Add(m.MonsterID, m);
        }
        else
        {
            Debug.LogWarning("[DataRepository] Monster table load failed.");
        }

        // ---- Item ----
        ItemTable iTable;
        bool iOk = loadFromResources
            ? DataLoader.TryLoadFromResources<ItemTable>(itemResourcePath, out iTable)
            : DataLoader.TryLoadFromStreamingAssets<ItemTable>(itemStreamingPath, out iTable);

        if (iOk && iTable != null)
        {
            var list = iTable.GetList();
            items.AddRange(list);
            foreach (var it in items)
                if (!itemById.ContainsKey(it.ItemID))
                    itemById.Add(it.ItemID, it);
        }
        else
        {
            Debug.LogWarning("[DataRepository] Item table load failed.");
        }
    }

    public bool TryGetMonster(string monsterId, out MonsterRecord record)
        => monsterById.TryGetValue(monsterId, out record);

    public bool TryGetItem(int itemId, out ItemRecord record)
        => itemById.TryGetValue(itemId, out record);
}