using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amplifier : MonoBehaviour
{
    List<Monster> monsters; //증폭에 영향을 받는 몬스터들
    Ring parent;        //부모링
    float coolTime;     //파티클 쿨타임

    void Awake()
    {
        monsters = new List<Monster>();
    }

    void Update()
    {
        //if (Time.timeScale == 0) return;
        coolTime += Time.deltaTime;
        //증폭끼리 영역이 겹쳤을 때 하나가 삭제되면 일시적으로 isInAmplify가 false로 변경될 수 있다. 따라서 매 프레임 계속 true로 바꿔줘야 함.
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].isInAmplify = true;
        if (coolTime > 0.5f)    //0.5초마다 파티클 재생
        {
            coolTime = 0.0f;
            for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].PlayParticleCollision(parent.baseRing.id, 0.0f);
        }
        //부모링이 전투에서 제거되면 본인도 제거한다.
        if (!parent.gameObject.activeSelf) RemoveFromBattle();
    }

    //증폭을 초기화한다.
    public void InitializeAmplifier(Ring par)
    {
        monsters.Clear();
        parent = par;
        transform.position = new Vector3(par.transform.position.x, par.transform.position.y, -0.2f);
        transform.localScale = new Vector3(par.baseRing.range * 2, par.baseRing.range * 2, 1);
    }

    //증폭을 전투에서 제거한다. 제거하면서 영향을 받던 모든 몬스터들의 추가 피격 상태를 해제한다.
    public void RemoveFromBattle()
    {
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].isInAmplify = false;
        monsters.Clear();
        GameManager.instance.ReturnAmplifierToPool(this);
    }

    //증폭에 몬스터가 들어오면 추가 피격 상태로 만든다.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monsters.Add(monster);
        }
    }

    //몬스터가 증폭을 나가면 추가 피격 상태를 해제한다.
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monster.isInAmplify = false;
            monsters.Remove(monster);
        }
    }
}
