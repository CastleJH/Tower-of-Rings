using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRing
{
    //종류
    public int id;

    //희귀도
    public int rarity;

    //이름
    public string name;

    //최대 강화레벨
    public int maxlvl;

    //db상 스탯
    public float csvATK;
    public float csvSPD;
    public float csvRP;

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
    public string toSame;
    public string toAll;

    public int level = 0;


    public void Init()
    {
        level = 1;
        baseATK = csvATK;
        baseSPD = csvSPD;
    }

    public void RenewStat()
    {
        baseATK = csvATK * (1.0f + (level - 1) * 0.5f);
        baseSPD = csvSPD * (1.0f - (level - 1) * 0.05f);
        if (GameManager.instance.baseRelics[2].have && GameManager.instance.playerCurHP < GameManager.instance.playerMaxHP * 0.2f)
        {
            if (GameManager.instance.baseRelics[2].isPure) baseATK *= 1.2f;
            else baseATK *= 0.9f;
        }
        else if (GameManager.instance.baseRelics[3].have && GameManager.instance.playerCurHP > GameManager.instance.playerMaxHP * 0.8f)
        {
            if (GameManager.instance.baseRelics[2].isPure) baseATK *= 1.1f;
            else baseATK *= 0.9f;
        }
    }

    //확률 안에 최대 레벨까지 강화. 그 후 공격력/공격 쿨타임을 변경함.
    public bool Upgrade(float poss)
    {
        if (Random.Range(0.0f, 1.0f) > poss) return false;
        if (level == maxlvl) return false;
        level++;

        RenewStat();

        return true;
    }


    //최소 1레벨까지 다운그레이드. 그 후 공격력/공격 쿨타임을 변경함.
    public void Downgrade()
    {
        if (level > 1) level--;

        RenewStat();

        Ring ring;
        for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--)
        {
            ring = DeckManager.instance.rings[i];
            ring.ChangeCurATK(0.0f);
            ring.ChangeCurSPD(0.0f);
            ring.ChangeCurNumTarget(0.0f);
            ring.ChangeCurEFF(0.0f, '*');
        }
    }
}