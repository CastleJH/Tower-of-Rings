using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int destroyID;
    public Ring parent;
    public Monster target;

    TrailRenderer trailRenderer;

    void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (parent.ringBase != null && target.curHP > 0 && target.movedDistance > 0.05f)
        {
            Move();   //�θ��� �ְ� Ÿ���� ��������� �̵�
        }
        else if (parent.ringBase != null)   //Ÿ�ٸ� �����Ŷ�� �ٽ� �߻�
        {
            parent.shootCoolTime = parent.curSPD - 0.1f;
            RemoveFromScene(0.0f);
        }
        else
        {
            RemoveFromScene(0.0f); //����
        }
    }

    //���� ���� �̵��Ѵ�.
    void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, parent.ringBase.bulletSPD * Time.deltaTime);
    }

    //�ҷ��� �θ�&Ÿ���� �����ϸ� �ʱ�ȭ�Ѵ�.
    public void InitializeBullet(Ring _parent, Monster _target)
    {
        parent = _parent;
        target = _target;
        transform.position = new Vector3(parent.transform.position.x, parent.transform.position.y, 0);
    }

    //���ӿ��� �ڽ��� time�� �� �����Ѵ�.
    void RemoveFromScene(float time)
    {
        if (time == 0.0f) InvokeReturnBulletToPool();
        else Invoke("InvokeReturnBulletToPool", time);
    }

    void InvokeReturnBulletToPool()
    {
        trailRenderer.Clear();
        CancelInvoke();
        GameManager.instance.ReturnBulletToPool(this);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Monster")
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();
            if (monster.id == target.id)    //�ùٸ� Ÿ�ٿ� ������ ���
            {
                //���� �� �ڽ��� �����Ѵ�.
                if (parent.ringBase != null) parent.AttackEffect(monster);
                RemoveFromScene(0.0f);
            }
        }
    }
}