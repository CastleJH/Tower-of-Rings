using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�� ������ ����. ��� Ring�� BaseRing�� ������ �� Ŭ������ ������� ���Ȱ��� ��������.
public class BaseRing
{
    //����
    public int id;

    //�̸�
    public string name;

    //�ִ� ��ȭ����
    public int maxlvl;

    //db�� ����
    public float csvATK;
    public float csvSPD;
    public float csvEFF;
    public int csvRange;
    public int csvNumTarget;
    public float csvRP;

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
    public string description;  //�⺻ ����
    public string toSame;       //���� �� �ó���
    public string toAll;        //��� �� �ó���

    public int level = 0;

    //DB �����ͷ� �� ������ ���� �ʱ�ȭ(����, �÷��̾� ���ȵ��� ������� ���� ���� ��) & ���� �ʱ�ȭ
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
    
    //����, ����, �÷��̾� ������ ������ �� ������ ����
    public void RenewStat()    
    {
        baseATK = csvATK * (1.0f + (level - 1) * 0.25f);
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

    //probȮ���� �������ϰ� ������ �����Ѵ�. �������� �ִ� ���������� �Ѵ�. ������ �� ���� true, �ƴ� ���� false�� ��ȯ�Ѵ�.
    public bool Upgrade(float prob)
    {
        if (Random.Range(0.0f, 1.0f) > prob) return false;
        if (level == maxlvl) return false;
        level++;

        RenewStat();

        return true;
    }


    //������ �ٿ�׷��̵� �ϰ� ������ �����Ѵ�. �̹� �ʵ忡 �����Ǿ��ִ� ������ ���� ���� �����Ѵ�. 1���� ������ �ٿ�׷��̵� �Ѵ�. 
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