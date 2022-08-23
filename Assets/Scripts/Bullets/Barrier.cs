using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    List<Monster> monsters; //결계에 영향을 받는 몬스터들

    void Awake()
    {
        monsters = new List<Monster>();
    }

    //결계를 초기화한다. 결계의 지속시간이 끝나면 스스로 사라지도록 한다.
    public void InitializeBarrier(float lifeTime, Vector2 pos)
    {
        monsters.Clear();
        transform.position = new Vector3(pos.x, pos.y, -0.1f);
        Invoke("InvokeRemoveFromBattle", lifeTime + 0.05f);
    }

    //결계를 없앤다. 결계에 영향을 받던 몬스터들의 이동 방해를 해제하고, 이 결계를 오브젝트 풀에 되돌린다.
    void InvokeRemoveFromBattle()
    {
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].barrierBlock = false;
        monsters.Clear();
        GameManager.instance.ReturnBarrierToPool(this);
    }

    //결계에 몬스터가 들어오면 이동 불가 상태로 만든다.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monsters.Add(monster);
            monster.barrierBlock = true;
        }
    }
}
