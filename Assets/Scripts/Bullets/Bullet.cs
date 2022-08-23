using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int destroyID;   //어느 링 타입으로부터 나온 불렛인지(올바른 오브젝트 풀에 되돌리기 위함)
    public Ring parent;     //이 불렛을 발사한 부모 링
    Monster target;         //이 불렛이 향하여 이동할 타겟
    bool isInBattle;        //전투 안에 유효하게 존재하는지 여부

    TrailRenderer trailRenderer;    //트레일

    void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (isInBattle && parent.baseRing != null && target != null && target.curHP > 0 && target.movedDistance > 0.05f) Move();   //전투 안에 유효하게 존재하고, 부모링이 있고, 타겟이 살아있으면 이동
        else RemoveFromBattle(); //제거
    }

    //타겟을 향해 이동한다.
    void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, parent.baseRing.bulletSPD * Time.deltaTime);
    }

    //불렛의 부모&타겟을 지정하며 초기화한다.
    public void InitializeBullet(Ring _parent, Monster _target)
    {
        parent = _parent;
        target = _target;
        isInBattle = true;

        transform.position = new Vector3(parent.transform.position.x, parent.transform.position.y, 0);
    }

    //게임에서 자신을 제거한다. 오브젝트 풀에 되돌리기 전 트레일을 꺼서 잔상을 없앤다.
    void RemoveFromBattle()
    {
        trailRenderer.Clear();
        GameManager.instance.ReturnBulletToPool(this);
    }

    //올바른 타겟에 도달했는지 확인한다.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Monster" && isInBattle)
        {
            Monster monster = collision.gameObject.GetComponent<Monster>();
            if (monster == target)    //올바른 타겟에 도달한 경우
            {
                if (parent.baseRing != null) parent.AttackEffect(monster);  //부모링이 유효하다면 공격한다.
                isInBattle = false;
                RemoveFromBattle();     //전투에서 제거한다.
            }
        }
    }
}