using System;
using System.Collections.Generic;

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

    // JSON이 [20000,30003] 형태면 그대로 파싱됨
    // 혹시 문자열 "20000,30003" 으로 올 수도 있어 대비용 필드
    public int[] DropItem;
    public string DropItemStr;
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

[Serializable]
public class MonsterTable
{
    public List<MonsterRecord> items = new List<MonsterRecord>();
}

[Serializable]
public class ItemTable
{
    public List<ItemRecord> items = new List<ItemRecord>();
}