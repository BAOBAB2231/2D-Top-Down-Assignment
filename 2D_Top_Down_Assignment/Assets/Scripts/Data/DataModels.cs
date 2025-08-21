using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonsterRecord
{
    public string MonsterID;
    public string Name;
    public string Description;

    public int Attack;
    public float AttackMul;
    public int MaxHP;
    public float MaxHPMul;

    public int AttackRange;
    public float AttackRangeMul;
    public float AttackSpeed;

    public float MoveSpeed;

    public int MinExp;
    public int MaxExp;

    public int[] DropItem;     // [20000,30003]
    public string DropItemStr; // "20000, 30003" 같은 CSV가 올 때 대비
}

[Serializable]
public class ItemRecord
{
    public int ItemID;
    public string Name;
    public string Description;

    public int UnlockLev;

    public int MaxHP;
    public float MaxHPMul;

    public int MaxMP;
    public float MaxMPMul;

    public int MaxAtk;
    public float MaxAtkMul;

    public int MaxDef;
    public float MaxDefMul;

    public int Status;
}

/* ---------- 루트 래퍼: 다양한 키 이름 모두 수용 ---------- */

[Serializable]
public class MonsterTable
{
    // 흔한 패턴들
    public List<MonsterRecord> items;
    public List<MonsterRecord> Items;
    public List<MonsterRecord> monster;
    public List<MonsterRecord> Monster;
    public List<MonsterRecord> monsters;
    public List<MonsterRecord> Monsters;

    public List<MonsterRecord> GetList()
    {
        return items ?? Items ?? monster ?? Monster ?? monsters ?? Monsters
               ?? new List<MonsterRecord>();
    }
}

[Serializable]
public class ItemTable
{
    public List<ItemRecord> items;
    public List<ItemRecord> Items;
    public List<ItemRecord> item;
    public List<ItemRecord> Item;

    public List<ItemRecord> GetList()
    {
        return items ?? Items ?? item ?? Item
               ?? new List<ItemRecord>();
    }
}