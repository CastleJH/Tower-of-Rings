using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRelic        //����
{
    public int id;        //����
    public string name;     //�̸�
    public bool have;
    public bool isPure;   //���� ����
    public string pureDescription;  //�Ϲ� ����(�������� ��Ÿ�� ����)
    public string cursedDescription;  //���� ����(�������� ��Ÿ�� ����)

    public void Init()
    {
        have = false;
        isPure = true;
    }
}