using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMonster        //������ ����
{
    public int type;        //����
    public string name;     //�̸�


    public int csvATK;     //���ݷ�
    public float csvHP;        //�⺻ HP
    public float csvSPD;       //�̵��ӵ�


    public int atk;     //���ݷ�
    public float hp;        //�⺻ HP
    public float spd;       //�̵��ӵ�


    public char tier;
    public string description;  //����(�������� ��Ÿ�� ����)


    public void Init()
    {
        atk = csvATK;
        hp = csvHP;
        spd = csvSPD;
    }
}