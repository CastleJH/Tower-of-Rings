using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    List<Monster> monsters; //��迡 ������ �޴� ���͵�

    void Awake()
    {
        monsters = new List<Monster>();
    }

    //��踦 �ʱ�ȭ�Ѵ�. ����� ���ӽð��� ������ ������ ��������� �Ѵ�.
    public void InitializeBarrier(float lifeTime, Vector2 pos)
    {
        monsters.Clear();
        transform.position = new Vector3(pos.x, pos.y, -0.001f);
        Invoke("InvokeRemoveFromBattle", lifeTime + 0.05f);
    }

    //��踦 ���ش�. ��迡 ������ �޴� ���͵��� �̵� ���θ� �ٽ� �����Ӱ� �ϰ�, ������Ʈ Ǯ�� �ǵ�����.
    void InvokeRemoveFromBattle()
    {
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].barrierBlock = false;
        monsters.Clear();
        GameManager.instance.ReturnBarrierToPool(this);
    }

    //��迡 ���Ͱ� ������ �̵� �Ұ� ���·� �����.
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monsters.Add(monster);
            monster.barrierBlock = true;
        }
    }

    /*
    //��迡 ���Ͱ� ������ �̵� �Ұ� ���·� �����.
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monster.barrierBlock = false;
            monsters.Remove(monster);
        }
    }
    */
}
