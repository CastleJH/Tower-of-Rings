using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int destroyID;   //��� �� Ÿ�����κ��� ���� �ҷ�����(�ùٸ� ������Ʈ Ǯ�� �ǵ����� ����)
    public Ring parent;     //�� �ҷ��� �߻��� �θ� ��
    Monster target;         //�� �ҷ��� ���Ͽ� �̵��� Ÿ��
    bool isInBattle;        //���� �ȿ� ��ȿ�ϰ� �����ϴ��� ����

    TrailRenderer trailRenderer;    //Ʈ����

    void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (isInBattle && parent.baseRing != null && target != null && target.curHP > 0 && target.movedDistance > 0.05f) Move();   //���� �ȿ� ��ȿ�ϰ� �����ϰ�, �θ��� �ְ�, Ÿ���� ��������� �̵�
        else RemoveFromBattle(); //����
    }

    //Ÿ���� ���� �̵��Ѵ�.
    void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, parent.baseRing.bulletSPD * Time.deltaTime);
    }

    //�ҷ��� �θ�&Ÿ���� �����ϸ� �ʱ�ȭ�Ѵ�.
    public void InitializeBullet(Ring _parent, Monster _target)
    {
        parent = _parent;
        target = _target;
        isInBattle = true;

        transform.position = new Vector3(parent.transform.position.x, parent.transform.position.y, 0);
    }

    //���ӿ��� �ڽ��� �����Ѵ�. ������Ʈ Ǯ�� �ǵ����� �� Ʈ������ ���� �ܻ��� ���ش�.
    void RemoveFromBattle()
    {
        trailRenderer.Clear();
        GameManager.instance.ReturnBulletToPool(this);
    }

    //�ùٸ� Ÿ�ٿ� �����ߴ��� Ȯ���Ѵ�.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Monster" && isInBattle)
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();
            if (monster == target)    //�ùٸ� Ÿ�ٿ� ������ ���
            {
                if (parent.baseRing != null) parent.AttackEffect(monster);  //�θ��� ��ȿ�ϴٸ� �����Ѵ�.
                isInBattle = false;
                RemoveFromBattle();     //�������� �����Ѵ�.
            }
        }
    }
}