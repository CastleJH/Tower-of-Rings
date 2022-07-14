using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blizzard : MonoBehaviour
{
    List<Monster> monsters; //눈보라에 영향을 받는 몬스터들
    Ring parent;        //부모링
    float coolTime;     //공격 쿨타임

    void Awake()
    {
        monsters = new List<Monster>();
    }

    void Update()
    {
        coolTime += Time.deltaTime;
        //눈보라끼리 영역이 겹쳤을 때 하나가 삭제되면 일시적으로 isInBlizzard가 false로 변경될 수 있다. 따라서 매 프레임 계속 true로 바꿔줘야 함.
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].isInBlizzard = true; 
        if (coolTime > 1.0f)    //1초마다 공격
        {
            coolTime = 0.0f;
            for (int i = monsters.Count - 1; i>= 0; i--) monsters[i].AE_DecreaseHP(parent.curATK, new Color32(50, 50, 255, 255));
        }
        //부모링이 전투에서 제거되면 본인도 제거한다.
        if (!parent.gameObject.activeSelf) RemoveFromBattle();
    }

    //눈보라를 초기화한다.
    public void InitializeBlizzard(Ring par)
    {
        monsters.Clear();
        parent = par;
        transform.position = new Vector3(par.transform.position.x, par.transform.position.y, -0.002f);
        transform.localScale = new Vector3(par.ringBase.range * 2, par.ringBase.range * 2, 1);
        coolTime = 0.0f;
    }

    //눈보라를 전투에서 제거한다. 제거하면서 영향을 받던 모든 몬스터들의 둔화를 해제한다.
    public void RemoveFromBattle()
    {
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].isInBlizzard = false;
        monsters.Clear();
        GameManager.instance.ReturnBlizzardToPool(this);
    }

    //눈보라에 몬스터가 들어오면 둔화 상태로 만든다.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monsters.Add(monster);
        }
    }

    //몬스터가 눈보라를 나가면 둔화 상태를 해제한다.
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
