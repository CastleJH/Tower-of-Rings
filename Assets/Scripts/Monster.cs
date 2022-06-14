using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Monster : MonoBehaviour
{
    public int id;  //생성 아이디(불렛이 올바른 타겟을 구분할 때 쓰임)

    //기본 스탯
    public BaseMonster baseMonster;

    //개인 스탯
    public float baseHP;
    public float curHP;
    public float baseSPD;
    public float curSPD;

    //이동 변수
    public PathCreator path;    //이동 경로
    public float movedDistance; //맵에서의 이동 거리
    public bool noBarrierBlock; //결계로부터 이동을 방해받지 않는지 여부

    //그래픽
    SpriteRenderer spriteRenderer;  //몬스터 이미지

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (curHP > 0)  //살아있는 경우에만 동작할 수 있음.
        {
            curSPD = baseSPD;
            /*
            추후 구현: 이동 속도 조절
            */
            spriteRenderer.color = Color.white; //색깔(상태 이상 표시 용)을 초기화

            if (noBarrierBlock) //결계에 막히지 않으면 이동
            {
                movedDistance += curSPD * Time.deltaTime;
                transform.position = path.path.GetPointAtDistance(movedDistance);
            }
        }
    }

    public void InitializeMonster(int _id, BaseMonster monster, int pathID, float scale)
    {
        //아이디
        id = _id;

        //스탯
        baseMonster = monster;
        baseHP = baseMonster.hp * scale;
        curHP = baseHP;
        baseSPD = baseMonster.spd;

        //이동 변수
        path = GameManager.instance.monsterPaths[pathID];
        movedDistance = 0.0f;
        noBarrierBlock = true;

        //그래픽
        spriteRenderer.sprite = GameManager.instance.monsterSprites[baseMonster.type];
    }
}
