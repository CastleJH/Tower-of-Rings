using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ringstone
{
    //종류
    public int id;

    //희귀도
    public int rarity;

    //이름
    public string name;

    //DB의 스탯
    public float dbDMG;
    public float dbSPD;

    //실제 게임에서의 기본 스탯
    public float baseDMG;
    public float baseSPD;
    public float baseEFF;
    public int baseNumTarget;
    public float baseRP;

    //불렛 이동 속도
    public float bulletSPD = 30.0f;

    //설명
    public string description;

    //시너지 타입
    public int range;
    public string identical;
    public string different;

    public int level = 0;

    public bool Upgrade()
    {
        if (level == 10) return false;
        level++;
        baseDMG = dbDMG;
        baseSPD = dbSPD;
        for (int i = 2; i <= level; i++)
            if (i % 2 == 0) baseDMG += dbDMG * 0.1f;
            else baseSPD *= 0.95f;
        return true;
    }

    public void Downgrade()
    {
        if (level > 1) level--;
        baseDMG = dbDMG;
        baseSPD = dbSPD;
        for (int i = 2; i <= level; i++)
            if (i % 2 == 0) baseDMG += dbDMG * 0.1f;
            else baseSPD *= 0.95f;
    }
}