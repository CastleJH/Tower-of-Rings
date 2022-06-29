using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    List<Monster> monsters;

    void Awake()
    {
        monsters = new List<Monster>();
    }

    public void InitializeBarrier(float lifeTime, Vector2 pos)
    {
        monsters.Clear();
        transform.position = new Vector3(pos.x, pos.y, -0.001f);
        Invoke("InvokeReturnBarrierToPool", lifeTime + 0.05f);
    }

    void InvokeReturnBarrierToPool()
    {
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].barrierBlock = false;
        monsters.Clear();
        GameManager.instance.ReturnBarrierToPool(this);
    }

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
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monster.barrierBlock = false;
            monsters.Remove(monster);
        }
    }
}
