using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int destroyID;
    public Ring parent;
    public Monster target;
    bool isInBattle;

    TrailRenderer trailRenderer;

    void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (isInBattle && parent.baseRing != null && target != null && target.curHP > 0 && target.movedDistance > 0.05f) Move();   //�θ��� �ְ� Ÿ���� ��������� �̵�
        else RemoveFromBattle(0.0f); //����
    }

    //���� ���� �̵��Ѵ�.
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

    //���ӿ��� �ڽ��� time�� �� �����Ѵ�.
    void RemoveFromBattle(float time)
    {
        if (time == 0.0f) InvokeRemoveFromBattle();
        else Invoke("InvokeRemoveFromBattle", time);
    }

    //������ ���ӿ��� �����Ѵ�.
    void InvokeRemoveFromBattle()
    {
        trailRenderer.Clear();
        CancelInvoke();
        GameManager.instance.ReturnBulletToPool(this);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Monster" && isInBattle)
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();
            if (target == null) RemoveFromBattle(0.0f);
            else if (monster.id == target.id)    //�ùٸ� Ÿ�ٿ� ������ ���
            {
                //���� �� �ڽ��� �����Ѵ�.
                if (parent.baseRing != null) parent.AttackEffect(monster);
                isInBattle = false;
                RemoveFromBattle(0.0f);
            }
        }
    }
}