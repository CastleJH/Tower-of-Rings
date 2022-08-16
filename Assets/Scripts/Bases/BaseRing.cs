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
    public float csvRP;

    //���� 1�� ���� �⺻ ����
    public float lvl1ATK;
    public float lvl1SPD;

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
        baseATK = lvl1ATK = csvATK;
        baseSPD = lvl1SPD = csvSPD;
    }

    //Ȯ�� �ȿ� �ִ� �������� ��ȭ. �� �� ���ݷ�/���� ��Ÿ���� ������.
    public bool Upgrade(float poss)
    {
        if (Random.Range(0.0f, 1.0f) > poss) return false;
        if (level == maxlvl) return false;
        level++;
        baseATK = lvl1ATK;
        baseSPD = lvl1SPD;
        for (int i = 2; i <= level; i++)
        {
            baseATK += lvl1ATK * 0.5f;
            baseSPD -= lvl1SPD * 0.05f;
        }
        return true;
    }


    //�ּ� 1�������� �ٿ�׷��̵�. �� �� ���ݷ�/���� ��Ÿ���� ������.
    public void Downgrade()
    {
        if (level > 1) level--;
        baseATK = lvl1ATK;
        baseSPD = lvl1SPD;
        for (int i = 2; i <= level; i++)
        {
            baseATK += lvl1ATK * 0.5f;
            baseSPD -= lvl1SPD * 0.05f;
        }

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