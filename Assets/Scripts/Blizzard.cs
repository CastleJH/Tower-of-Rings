using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blizzard : MonoBehaviour
{
    List<Monster> monsters;
    Ring parent;
    float coolTime;

    void Awake()
    {
        monsters = new List<Monster>();
    }

    void Update()
    {
        coolTime += Time.deltaTime;
        for (int i = monsters.Count - 1; i >= 0; i--)
        {
            monsters[i].isInBlizzard = true; 
        }
        if (coolTime > 1.0f)
        {
            coolTime = 0.0f;
            for (int i = monsters.Count - 1; i>= 0; i--)
            {
                monsters[i].AE_DecreaseHP(parent.curATK, new Color32(50, 50, 255, 255));
            }
        }
        if (!parent.gameObject.activeSelf)
        {
            RemoveBlizzard();
        }
    }

    public void InitializeBlizzard(Ring par)
    {
        monsters.Clear();
        parent = par;
        transform.position = new Vector3(par.transform.position.x, par.transform.position.y, -0.002f);
        coolTime = 0.0f;
    }

    public void RemoveBlizzard()
    {
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].isInBlizzard = false;
        monsters.Clear();
        GameManager.instance.ReturnBlizzardToPool(this);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monsters.Add(monster);
            monster.isInBlizzard = true;
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monster.isInBlizzard = false;
            monsters.Remove(monster);
        }
    }
}
