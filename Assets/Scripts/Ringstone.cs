using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ringstone
{
    //����
    public int id;

    //��͵�
    public int rarity;

    //�̸�
    public string name;

    //DB�� ����
    public float dbDMG;
    public float dbSPD;

    //���� ���ӿ����� �⺻ ����
    public float baseDMG;
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