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
    public TextMesh hpText;   //HP 텍스트
    public List<ParticleSystem> particles; //본인의 피격 파티클
    public List<int> particlesID;   //본인의 피격 파티클 종류

    //기타 변수
    float snowTime;         //눈꽃의 슬로우 효과가 지속된 시간
    float snowEndTime;      //눈꽃의 슬로우 효과가 끝나는 시간

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (curHP > 0)  //살아있는 경우에만 동작할 수 있음.
        {
            curSPD = baseSPD;

            spriteRenderer.color = Color.white; //색깔(상태 이상 표시 용)을 초기화

            //이동방해는 약한순->강한순으로 적용되어야 한다.
            if (snowTime < snowEndTime) //눈꽃 링의 둔화가 적용중이라면
            {
                curSPD = baseSPD * 0.5f;
                snowTime += Time.deltaTime;
                spriteRenderer.color = Color.cyan;
            }

            if (noBarrierBlock) //결계에 막히지 않으면 이동
            {
                movedDistance += curSPD * Time.deltaTime;
                transform.position = path.path.GetPointAtDistance(movedDistance);
                transform.Translate(0.0f, 0.0f, id * 0.001f);
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
        spriteRenderer.color = Color.white; //색깔(상태 이상 표시 용)을 초기화
        SetHPText();
        InvokeParticleOff();

        //기타 변수
        snowEndTime = -1.0f;
    }

    //파티클을 플레이한다.
    public void PlayParticleCollision(int id, float time)
    {
        //피격 파티클을 가져와 저장한다.
        ParticleSystem par = GameManager.instance.GetParticleFromPool(id);

        //지속 시간을 지정해야 한다면 설정한다.
        if (time != 0.0f)
        {
            ParticleSystem[] pars = par.GetComponentsInChildren<ParticleSystem>();
            par.Stop(true);

            for (int i = 0; i < pars.Length; i++)
            {
                var main = pars[i].main;
                main.duration = time;
            }
        }

        //저장한다.
        particles.Add(par);
        particlesID.Add(id);

        //위치를 설정한다.
        par.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.1f);
        par.gameObject.transform.parent = gameObject.transform;

        //플레이한다.
        par.gameObject.SetActive(true);
        par.Play();
    }

    //제거해야 하는 파티클들을 확인하고 제거한다.
    public void InvokeParticleOff()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            if (!particles[i].IsAlive(true) || !BattleManager.instance.isBattlePlaying)   //파티클 재생이 끝났거나 게임이 끝난 경우
            {
                //재생을 한번 더 멈추고 제거한다(게임 종료에 의해 제거해야 하는 것일 수 있으므로).
                particles[i].Stop(true);
                GameManager.instance.ReturnParticleToPool(particles[i], particlesID[i]);
                particles.RemoveAt(i);
                particlesID.RemoveAt(i);
            }
        }

        if (BattleManager.instance.isBattlePlaying && curHP > 0)  //게임이 아직 진행중이고 자신이 아직 HP가 남아있다면 다음에 이 함수를 또 부른다.
            Invoke("InvokeParticleOff", 1f);
    }

    //HP 텍스트를 갱신한다.
    private void SetHPText()
    {
        if (curHP > 1.0f) hpText.text = ((int)curHP).ToString();
        else if (curHP > 0.0f) hpText.text = "1";
        else hpText.text = "0";
    }
    
    //죽었는지 확인한다. 사망 애니메이션 후 잠시 뒤 게임에서 자신을 제거한다.
    void CheckDead()
    {
        if (curHP <= 0)
        {
            /*추가 구현 필요 - 사망 애니메이션*/
            RemoveFromScene(0.5f);
        }
    }

    //게임에서 time 초 뒤 제거한다.
    public void RemoveFromScene(float time)
    {
        if (time == 0.0f) InvokeReturnMonsterToPool();
        else Invoke("InvokeReturnMonsterToPool", time);
    }

    //실제로 게임에서 제거한다. 링에 의한 각종 사망 효과를 적용한다. 골드/RP도 증가시킨다.
    void InvokeReturnMonsterToPool()
    {
        if (!gameObject.activeSelf) return;

        BattleManager.instance.monsters.Remove(this);
        //BattleManager.instance.totalGetGold += 10;
        //BattleManager.instance.totalKilledMonster++;
        //BattleManager.instance.rp += BattleManager.instance.genRP;
        //UIManager.instance.SetIngameTotalRPText();

        //저주 링의 사망시 폭발 효과를 적용한다.
        //if (curseStack != 0) AE_CurseDead();
        //if (growRings.Count > 0) AE_GrowDead();
        //if (DeckManager.instance.isNecro) DeckManager.instance.necroCnt++;

        //파티클을 모두 제거한다.
        CancelInvoke();
        InvokeParticleOff();

        //HP를 0으로 바꾼다.
        curHP = 0;

        GameManager.instance.ReturnMonsterToPool(this);
    }

    //공격 이펙트: HP감소
    public void AE_DecreaseHP(float dmg, Color32 color)
    {
        if (dmg != -1)
        {
            curHP -= dmg;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText((Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
            t.gameObject.SetActive(true);
        }
        else
        {
            curHP = 0;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText("X", transform.position, Color.black);
            t.gameObject.SetActive(true);
        }
        SetHPText();
        CheckDead();
    }

    //공격 이펙트: 눈꽃
    public void AE_Snow(float time)
    {
        //이미 받고있는 눈꽃 링의 둔화 효과가 더 오래 간다면 적용 취소
        if (snowEndTime - snowTime > time) return;
        snowEndTime = time;
        snowTime = 0;
    }
}
