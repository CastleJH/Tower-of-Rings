using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Monster : MonoBehaviour
{
    public int id;  //���� ���̵�(�ҷ��� �ùٸ� Ÿ���� ������ �� ����)

    //�⺻ ����
    public BaseMonster baseMonster;

    //���� ����
    public float baseHP;
    public float curHP;
    public float baseSPD;
    public float curSPD;

    //�̵� ����
    public PathCreator path;    //�̵� ���
    public float movedDistance; //�ʿ����� �̵� �Ÿ�
    public bool noBarrierBlock; //���κ��� �̵��� ���ع��� �ʴ��� ����

    //�׷���
    SpriteRenderer spriteRenderer;  //���� �̹���

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (curHP > 0)  //����ִ� ��쿡�� ������ �� ����.
        {
            curSPD = baseSPD;
            /*
            ���� ����: �̵� �ӵ� ����
            */
            spriteRenderer.color = Color.white; //����(���� �̻� ǥ�� ��)�� �ʱ�ȭ

            if (noBarrierBlock) //��迡 ������ ������ �̵�
            {
                movedDistance += curSPD * Time.deltaTime;
                transform.position = path.path.GetPointAtDistance(movedDistance);
            }
        }
    }

    public void InitializeMonster(int _id, BaseMonster monster, int pathID, float scale)
    {
        //���̵�
        id = _id;

        //����
        baseMonster = monster;
        baseHP = baseMonster.hp * scale;
        curHP = baseHP;
        baseSPD = baseMonster.spd;

        //�̵� ����
        path = GameManager.instance.monsterPaths[pathID];
        movedDistance = 0.0f;
        noBarrierBlock = true;

        //�׷���
        spriteRenderer.sprite = GameManager.instance.monsterSprites[baseMonster.type];
    }
}
