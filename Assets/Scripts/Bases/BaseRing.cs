using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//링 원형의 정보. 모든 Ring은 BaseRing을 가지며 이 클래스의 변수들로 스탯값이 정해진다.
public class BaseRing
{
    //종류
    public int id;

    //이름
    public string name;

    //최대 강화레벨
    public int maxlvl;

    //db상 스탯
    public float csvATK;
    public float csvSPD;
    public float csvEFF;
    public int csvRange;
    public int csvNumTarget;
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
    public string description;  //기본 설명
    public string toSame;       //같은 링 시너지
    public string toAll;        //모든 링 시너지

    public int level = 0;

    //DB 데이터로 링 원형의 스탯 초기화(유물, 플레이어 스탯등이 적용되지 않은 최초 값) & 레벨 초기화
    public void Init()      
    {
        baseATK = csvATK;
        baseSPD = csvSPD;
        baseEFF = csvEFF;
        range = csvRange;
        baseNumTarget = csvNumTarget;
        baseRP = csvRP;

        level = 1;
    }
    
    //레벨, 유물, 플레이어 스탯을 적용한 링 원형의 스탯
    public void RenewStat()    
    {
        baseATK = csvATK * (1.0f + (level - 1) * 0.25f) * (1.0f + GameManager.instance.spiritEnhanceLevel[0] * 0.05f);
        baseSPD = csvSPD * (1.0f - (level - 1) * 0.05f) * (1.0f - GameManager.instance.spiritEnhanceLevel[1] * 0.03f);
        if (GameManager.instance.baseRelics[2].have && GameManager.instance.playerCurHP < GameManager.instance.playerMaxHP * 0.2f)
        {
            if (GameManager.instance.baseRelics[2].isPure) baseATK *= 1.2f;
            else baseATK *= 0.9f;
        }
        else if (GameManager.instance.baseRelics[3].have && GameManager.instance.playerCurHP > GameManager.instance.playerMaxHP * 0.8f)
        {
            if (GameManager.instance.baseRelics[3].isPure) baseATK *= 1.1f;
            else baseATK *= 0.9f;
        }
    }

    //prob확률로 레벨업하고 스탯을 갱신한다. 레벨업은 최대 레벨까지만 한다. 레벨업 한 경우는 true, 아닌 경우는 false를 반환한다.
    public bool Upgrade(float prob)
    {
        if (Random.Range(0.0f, 1.0f) > prob) return false;
        if (level == maxlvl) return false;
        level++;

        RenewStat();

        return true;
    }


    //레벨을 다운그래이드 하고 스탯을 갱신한다. 이미 필드에 생성되어있는 링들의 스탯 또한 갱신한다. 1레벨 까지만 다운그레이드 한다. 
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