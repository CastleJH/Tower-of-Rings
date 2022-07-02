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
        if (parent.ringBase != null && target != null && target.curHP > 0 && target.movedDistance > 0.05f) Move();   //부모링이 있고 타겟이 살아있으면 이동
        else RemoveFromBattle(0.0f); //제거
    }

    //적을 향해 이동한다.
    void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, parent.ringBase.bulletSPD * Time.deltaTime);
    }

    //불렛의 부모&타겟을 지정하며 초기화한다.
    public void InitializeBullet(Ring _parent, Monster _target)
    {
        parent = _parent;
        target = _target;
        transform.position = new Vector3(parent.transform.position.x, parent.transform.position.y, 0);
    }

    //게임에서 자신을 time초 뒤 제거한다.
    void RemoveFromBattle(float time)
    {
        if (time == 0.0f) InvokeRemoveFromBattle();
        else Invoke("InvokeRemoveFromBattle", time);
    }

    //실제로 게임에서 제거한다.
    void InvokeRemoveFromBattle()
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
            if (target == null) RemoveFromBattle(0.0f);
            else if (monster.id == target.id)    //올바른 타겟에 도달한 경우
            {
                //공격 후 자신을 제거한다.
                if (parent.ringBase != null) parent.AttackEffect(monster);
                RemoveFromBattle(0.0f);
            }
        }
    }
}