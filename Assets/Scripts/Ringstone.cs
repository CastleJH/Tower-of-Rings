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
    public float dbATK;
    public float dbSPD;

    //실제 게임에서의 기본 스탯
    public float baseATK;
    public float baseSPD;
    public float baseEFF;
    public int range;
    public int baseNumTarget;
    public float baseRP;

    //불렛 이동 속도
    public float bulletSPD = 30.0f;

    //설명
    public string description;

    //시너지
    public string identical;
    public string different;

    public int level = 0;

    //최대 10레벨까지 강화. 그 후 공격력/공격 쿨타임을 변경함.
    public bool Upgrade()
    {
        if (level == 10) return false;
        level++;
        baseATK = dbATK;
        baseSPD = dbSPD;
        for (int i = 2; i <= level; i++)
            if (i % 2 == 0) baseATK += dbATK * 0.1f;
            else baseSPD *= 0.95f;
        return true;
    }


    //최소 1레벨까지 다운그레이드. 그 후 공격력/공격 쿨타임을 변경함.
    public void Downgrade()
    {
        if (level > 1) level--;
        baseATK = dbATK;
        baseSPD = dbSPD;
        for (int i = 2; i <= level; i++)
            if (i % 2 == 0) baseATK += dbATK * 0.1f;
            else baseSPD *= 0.95f;
    }
}