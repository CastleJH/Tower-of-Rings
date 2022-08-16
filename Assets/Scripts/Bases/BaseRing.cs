using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRing
{
    //����
    public int id;

    //��͵�
    public int rarity;

    //�̸�
    public string name;

    //�ִ� ��ȭ����
    public int maxlvl;

    //db�� ����
    public float csvATK;
    public float csvSPD;

    //���� ���ӿ����� �⺻ ����
    public float baseATK;
    public float baseSPD;
    public float baseEFF;
    public int range;
    public int baseNumTarget;
    public float baseRP;

    //�ҷ� �̵� �ӵ�
    public float bulletSPD = 30.0f;

    //����
    public string description;

    //�ó���
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
            if (GameManager.instance.baseRelics[3].isPure) baseATK *= 1.1f;
            else baseATK *= 0.9f;
        }
    }

    //Ȯ�� �ȿ� �ִ� �������� ��ȭ. �� �� ���ݷ�/���� ��Ÿ���� ������.
    public bool Upgrade(float poss)
    {
        if (Random.Range(0.0f, 1.0f) > poss) return false;
        if (level == maxlvl) return false;
        level++;

        RenewStat();

        return true;
    }


    //�ּ� 1�������� �ٿ�׷��̵�. �� �� ���ݷ�/���� ��Ÿ���� ������.
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