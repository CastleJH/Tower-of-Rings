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

    //DB�� ����
    public float dbATK;
    public float dbSPD;
    public float dbRP;

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

    //�ִ� 10�������� ��ȭ. �� �� ���ݷ�/���� ��Ÿ���� ������.
    public bool Upgrade()
    {
        if (level == 5) return false;
        level++;
        baseATK = dbATK;
        baseSPD = dbSPD;
        for (int i = 2; i <= level; i++)
        {
            baseATK += dbATK * 0.5f;
            baseSPD -= dbSPD * 0.05f;
        }
        return true;
    }


    //�ּ� 1�������� �ٿ�׷��̵�. �� �� ���ݷ�/���� ��Ÿ���� ������.
    public void Downgrade()
    {
        if (level > 1) level--;
        baseATK = dbATK;
        baseSPD = dbSPD;
        for (int i = 2; i <= level; i++)
        {
            baseATK += dbATK * 0.5f;
            baseSPD -= dbSPD * 0.05f;
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