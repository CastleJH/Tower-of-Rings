using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PathCreation;

public class Monster : MonoBehaviour
{
    public int id;  //생성 아이디(불렛이 올바른 타겟을 구분할 때 쓰임)

    //기본 스탯
    public BaseMonster baseMonster;

    //조정된 개인 스탯
    public float maxHP;
    public float curHP;
    public float baseSPD;       //링 이동 방해 효과 적용 전 속도
    public float curSPD;        //링 이동 방해 효과 적용 후 속도

    //이동 변수
    public PathCreator path;    //이동 경로
    public float movedDistance; //맵에서의 이동 거리

    //그래픽
    public Animator anim;
    public TextMesh hpText;     //HP 텍스트
    float prevX;                //직전 프레임의 x좌표(이 값에 의해 좌/우 방향이 flip됨)

    //기타 변수
    bool isInBattle;            //살아서 전투에 참여중인지 여부
    float snowTime;             //눈꽃의 슬로우 효과가 지속된 시간
    float snowEndTime;          //눈꽃의 슬로우 효과가 끝나는 시간
    float poisonDmg;            //맹독의 데미지
    float poisonTime;           //맹독의 데미지 쿨타임
    public bool isInBlizzard;   //눈보라 속에 있는지 여부
    float paralyzeTime;         //마비의 마비 효과가 지속된 시간
    float paralyzeEndTime;      //마비의 마비 효과가 끝나는 시간
    public bool barrierBlock;   //결계로부터 이동을 방해받지 않는지 여부
    public int curseStack;      //저주로부터 쌓인 스택
    public bool isInAmplify;    //증폭 범위 내에 있는지 여부
    public float amplifyInc;    //증폭량
    float skillCoolTime1;       //엘리트/보스 몬스터의 스킬 쿨타임
    float skillCoolTime2;       //엘리트/보스 몬스터의 스킬 쿨타임
    float skillUseTime;         //엘리트/보스 몬스터의 스킬 지속된 시간
    bool immuneDamage;          //데미지 면역 여부
    bool immuneInterrupt;       //이동방해 면역 여부
    int cloneID;                //복제술사의 복제된 몬스터 ID

    void OnEnable()
    {
        anim.SetTrigger("AddScene");    //애니메이션을 RunState로 돌린다.
    }

    void Update()
    {
        if (isInBattle)
        {
            //기본 속도를 정한다. 일반 몬스터가 아니라면 스킬을 써서 기본 속도를 바꾼다.
            baseSPD = baseMonster.baseSPD;      
            if (baseMonster.tier != 'n') UseMonsterSkill();

            //최종 속도를 정한다. 링의 이동 방해 효과등을 적용한다.
            curSPD = baseSPD;
            if (!immuneInterrupt)
            {
                //이동방해는 약한순->강한순으로 체크한다. 지속시간도 감소시키면서 마지막에 가장 강한 이동 방해 효과만 받도록 하기 위함임.
                if (isInBlizzard) curSPD = baseSPD * 0.7f; //눈보라

                if (snowTime < snowEndTime) //눈꽃
                {
                    curSPD = baseSPD * 0.5f;
                    snowTime += Time.deltaTime;
                }

                if (paralyzeTime < paralyzeEndTime) //마비
                {
                    curSPD = 0;
                    paralyzeTime += Time.deltaTime;
                }

                if (barrierBlock) curSPD = 0;  //결계
            }

            //맹독 중첩이 있으면 데미지를 받는다.
            if (poisonDmg > 0)
            {
                poisonTime += Time.deltaTime;
                if (poisonTime >= 1.0f)
                {
                    AE_DecreaseHP(poisonDmg, Color.green);
                    PlayParticleCollision(5, 0.0f);
                    poisonTime = 0.0f;
                }
            }

            //최종 이속으로 애니메이션 속도를 결정한다.
            anim.speed = curSPD;

            //최종 이속으로 몬스터 위치를 결정한다.
            prevX = transform.position.x;
            movedDistance += curSPD * Time.deltaTime;
            transform.position = path.path.GetPointAtDistance(movedDistance) + new Vector3(0.0f, 0.0f, id * 0.001f);

            //직전 위치와 x좌표를 비교하여 좌우반전한다.
            if (prevX > transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
                hpText.transform.localScale = new Vector3(0.1f, 0.1f, 1);
            }
            else if (prevX < transform.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                hpText.transform.localScale = new Vector3(-0.1f, 0.1f, 1);
            }
        }
    }

    //몬스터를 초기화한다.
    public void InitializeMonster(int _id, int pathID, float scale)
    {
        //아이디
        id = _id;

        //스탯
        maxHP = baseMonster.baseMaxHP * scale;
        curHP = maxHP;
        baseSPD = baseMonster.baseSPD;
        SetHPText();

        //이동 변수
        path = GameManager.instance.monsterPaths[pathID];
        movedDistance = 0.0f;

        //기타 변수
        isInBattle = true;
        snowEndTime = -1.0f;
        poisonDmg = 0.0f;
        poisonTime = 0.0f;
        isInBlizzard = false;
        paralyzeEndTime = -1.0f;
        barrierBlock = false;
        curseStack = 0;
        isInAmplify = false;
        amplifyInc = 0.0f;
        if (baseMonster.tier == 'n' || baseMonster.type == 23) skillCoolTime1 = 0.0f;
        else skillCoolTime1 = 5.0f;
        skillCoolTime2 = 0.0f;
        skillUseTime = 0.0f;
        immuneDamage = false;
        immuneInterrupt = false;
        cloneID = -1;
    }

    //파티클을 플레이한다.
    public void PlayParticleCollision(int id, float time)
    {
        //피격 파티클을 가져와 저장한다.
        ParticleChecker par = GameManager.instance.GetParticleFromPool(id);

        //플레이한다.
        par.PlayParticle(transform, time);
    }

    //파티클을 플레이한다.
    public void PlayParticleCollision(int id, float time, float scale)
    {
        //피격 파티클을 가져와 저장한다.
        ParticleChecker par = GameManager.instance.GetParticleFromPool(id);
        par.transform.localScale = Vector3.one * scale;

        //플레이한다.
        par.PlayParticle(transform, time);
    }

    //HP 텍스트를 갱신한다. 소수점을 버리고 표현하되, 남은 HP가 0초과 1이하인 경우는 1로 표현한다.
    public void SetHPText()
    {
        if (curHP > 1.0f) hpText.text = ((int)curHP).ToString();
        else if (curHP > 0.0f) hpText.text = "1";
        else hpText.text = "0";
    }
    
    //죽었는지 확인한다. 사망 애니메이션 후 잠시 뒤 게임에서 자신을 제거한다.
    void CheckDead()
    {
        if (curHP <= 0 && isInBattle)
        {
            isInBattle = false;
            RemoveFromBattle(true);
        }
    }

    //게임에서 제거한다. isDead = true면 사망으로 인한 효과를 낸다.
    public void RemoveFromBattle(bool isDead)
    {
        if (!gameObject.activeSelf) return;

        isInBattle = false;
        curHP = 0;
        BattleManager.instance.monsters.Remove(this);

        if (isDead) //사망으로 인해 제거하는 경우
        {
            if (baseMonster.tier == 'b') BattleManager.instance.isBossKilled = true;    //보스몬스터였으면 유물을 획득할 수 있게 플래그를 올린다.

            //사망 애니메이션을 보여준다.
            anim.ResetTrigger("AddScene");
            anim.speed = 0.8f;
            anim.SetTrigger("Die");

            //사망으로 인한 게임 내 효과를 낸다.
            ApplyDeadEffect();

            //몬스터를 풀에 잠시 뒤(애니메이션이 끝난 뒤) 되돌려준다.
            Invoke("InvokeReturnMonsterToPool", 1.0f);
        }
        else InvokeReturnMonsterToPool();   //아니면 그냥 바로 제거한다.
    }

    //사망 효과를 적용한다.
    void ApplyDeadEffect()
    {
        //저주 링의 사망시 폭발 효과를 적용한다.
        if (curseStack != 0) AE_CurseDead();

        //사령술 링의 소환을 위한 카운팅을 한다.
        if (DeckManager.instance.necroIdx != -1)
        {
            if (DeckManager.instance.necroCount < 10)   //사령술 카운트가 10이 될때까지 한다.
            {
                DeckManager.instance.necroCount++;
                if (DeckManager.instance.necroCount >= 10)  //링 소환 덱의 텍스트를 갱신한다.
                {
                    DeckManager.instance.necroCount = 10;
                    UIManager.instance.SetBattleDeckRingRPText(DeckManager.instance.necroIdx, "10/10");
                    BattleManager.instance.ChangePlayerRP(0);
                }
                else UIManager.instance.SetBattleDeckRingRPText(DeckManager.instance.necroIdx, DeckManager.instance.necroCount.ToString() + "/10");
            }
        }

        GameManager.instance.MonsterCollectionProgressUp(baseMonster.type);
    }

    //몬스터의 스킬을 사용한다(엘리트/보스인 경우만 불린다).
    void UseMonsterSkill()
    {
        switch (baseMonster.type)
        {
            case 15:
                Elite_Berserker();
                break;
            case 16:
                Elite_Messenger();
                break;
            case 17:
                Elite_Giant();
                break;
            case 18:
                Elite_DarkPriest();
                break;
            case 19:
                Elite_DevilCommander();
                break;
            case 20:
                Elite_Purifier();
                break;
            case 21:
                Elite_Predator();
                break;
            case 22:
                Boss_Puppeteer();
                break;
            case 23:
                Boss_CloneMage();
                break;
            case 24:
                Boss_Sealer();
                break;
            case 25:
                Boss_DarkWarlock();
                break;
            case 26:
                Boss_Spacer();
                break;
            case 27:
                Boss_Executer();
                break;
            case 28:
                Boss_King();
                break;
        }
    }

    //피격 이펙트: HP감소 (만일 dmg가 음수이면 회복, 1987654321이면 즉사/처형. 즉사는 color.r == 0이고 처형은 255이다.)
    public void AE_DecreaseHP(float dmg, Color32 color)
    {
        if (dmg >= 0)
        {
            if (immuneDamage) dmg = 0;
            if (dmg != 1987654321)
            {
                if (isInAmplify) dmg *= (1.0f + amplifyInc);   //증폭 효과를 받는중이면 데미지를 올린다.
                curHP -= dmg;
                //데미지 미터기를 표시한다. 소수점 아래 둘째자리까지만 표현.
                DamageText t = GameManager.instance.GetDamageTextFromPool();
                t.InitializeDamageText((Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
                t.gameObject.SetActive(true);
            }
            else //즉사/처형인 경우
            {
                curHP = 0;
                DamageText t = GameManager.instance.GetDamageTextFromPool();
                if (color.r == 0) t.InitializeDamageText("즉사!", transform.position, new Color32(70, 70, 70, 255));
                else t.InitializeDamageText("처형!", transform.position, new Color32(70, 70, 70, 255));
            }
        }
        else //HP 회복인 경우
        {
            curHP -= dmg;
            if (curHP > maxHP) curHP = maxHP;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText((-Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
            t.gameObject.SetActive(true);
        }
        SetHPText();

        //사망 판정인지 확인한다.
        CheckDead();
    }

    //피격 이펙트: 눈꽃
    public void AE_Snow(float dmg, float time)
    {
        AE_DecreaseHP(dmg, new Color32(0, 0, 180, 255));
        //이미 받고있는 눈꽃 링의 둔화 효과가 더 오래 간다면 적용 취소
        if (snowEndTime - snowTime > time) return;
        snowEndTime = time;
        snowTime = 0;
    }

    //피격 이펙트: 폭발
    public void AE_Explosion(float dmg, float splash)
    {
        //자신 근처의 자신이 아닌 다른 모든 몬스터들에 스플래시 데미지
        Monster monster;
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (monster.isInBattle && Vector2.Distance(transform.position, monster.transform.position) < 1.5f && monster != this) 
                monster.AE_DecreaseHP(dmg * splash, new Color32(150, 30, 30, 255));
        }
        //자신에게 원래 데미지
        AE_DecreaseHP(dmg, new Color32(150, 30, 30, 255));
    }

    //피격 이펙트: 맹독
    public void AE_Poison(float dmg)
    {
        poisonDmg += dmg;
    }

    //피격 이펙트: 마비
    public void AE_Paralyze(float dmg, float time)
    {
        AE_DecreaseHP(dmg, new Color32(150, 150, 0, 255));
        //이미 받고있는 마비 링의 마비 효과가 더 오래 간다면 적용 취소
        if (paralyzeEndTime - paralyzeTime > time) return;
        paralyzeEndTime = time;
        paralyzeTime = 0;
    }

    //피격 이펙트: 귀환(텔레포트)
    public void AE_Teleport(float dmg, float prob)
    {
        AE_DecreaseHP(dmg, new Color32(255, 0, 200, 255));
        if (Random.Range(0.0f, 1.0f) < prob) //일정 확률로 이동 거리를 0으로 만들어버림
        {
            movedDistance = 0.0f;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText("귀환!", transform.position, new Color32(255, 0, 200, 255));
            t.gameObject.SetActive(true);
        }
    }

    //피격 이펙트: 절단
    public void AE_Cut(float dmg)
    {
        if (baseMonster.tier == 'n') AE_DecreaseHP(curHP * (dmg * 0.01f), Color.red);
        else AE_DecreaseHP(curHP * (dmg * 0.005f), Color.red);
    }

    //피격 이펙트: 저주
    public void AE_Curse(int stack)
    {
        curseStack += stack;
    }

    //피격 이펙트: 저주의 사망 효과
    public void AE_CurseDead()
    {
        float dmg = curseStack;
        dmg = maxHP * dmg * 0.01f;

        //모든 몬스터들에 자신에게 쌓인 저주 스택에 비례하여 피해를 입힘
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            BattleManager.instance.monsters[i].PlayParticleCollision(21, 0.0f);
            BattleManager.instance.monsters[i].AE_DecreaseHP(dmg, new Color32(50, 50, 50, 255));
        }
    }

    //피격 이펙트: 처형
    public void AE_Execution(float dmg, float rate)
    {
        if (baseMonster.tier != 'n') rate *= 0.5f;  //엘리트/보스 몬스터는 처형컷이 더 낮아짐
        if (curHP - dmg < maxHP * rate) AE_DecreaseHP(1987654321, Color.red); //이 공격으로 인해 일정 HP 미만이 되면 처형
        else AE_DecreaseHP(dmg, new Color32(70, 70, 70, 255));
    }

    //피격 이펙트: 성장
    public void AE_Grow(float dmg, Ring ring)
    {
        AE_DecreaseHP(dmg, new Color32(30, 180, 30, 255));
        if (curHP <= 0 && ring.growStack < 20) //최대 20까지 성장
        {
            ring.growStack++;
            ring.ChangeCurATK(0.1f);
        }
    }

    //피격 이펙트: 천사
    public void AE_Angel()
    {
        AE_DecreaseHP(curHP * 0.2f, Color.yellow);
        movedDistance = 0;  //모두 이동거리를 0으로 만듦
    }

    //피격 이펙트: 추적
    public void AE_Chase(float dmg, float radius)
    {
        Monster monster;
        //자신을 포함한 일정 반경 내 몬스터 모두에게 데미지주기
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (monster.isInBattle && Vector2.Distance(transform.position, monster.transform.position) < radius)
                monster.AE_DecreaseHP(dmg, new Color32(100, 0, 0, 255));
        }
    }

    //피격 이펙트: 즉사
    public void AE_InstantDeath(float dmg, float prob)
    {
        if (Random.Range(0.0f, 1.0f) < prob)    //일정 확률로 즉사(단 일반 몬스터가 아니면 HP비례 데미지입음)
        {
            if (baseMonster.tier != 'n') AE_DecreaseHP(maxHP * prob * 0.5f, new Color32(80, 80, 80, 255));
            else AE_DecreaseHP(1987654321, Color.black);
        }
        else AE_DecreaseHP(dmg, new Color32(80, 80, 80, 255));  //즉사 아니면 평범하게 데미지입음
    }

    //엘리트 몬스터 스킬: 광전사
    void Elite_Berserker()
    {
        baseSPD = baseMonster.baseSPD * (1.0f + (maxHP - curHP) / maxHP); //잃은 HP 비례 속도가 빨라짐
    }

    //엘리트 몬스터 스킬: 전령
    void Elite_Messenger()
    {
        if (skillUseTime != 0)  //스킬 사용중이라면 이속을 두배로 만든다.
        {
            baseSPD = baseMonster.baseSPD * 2;
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 2.0f) //2초간 스킬이 지속된다.
            {
                skillUseTime = 0.0f;
                skillCoolTime1 = 0.0f;
            }
        }
        else   //5초 쿨타임을 기다림
        {
            skillCoolTime1 += Time.deltaTime;
            if (skillCoolTime1 > 5.0f)
            {
                skillCoolTime1 = 0.0f;
                skillUseTime = 0.0001f;
            }
        }
    }

    //엘리트 몬스터 스킬: 철벽거인
    void Elite_Giant()
    {
        if (curHP < maxHP * 0.2f) //최대 HP 20% 미만이 되면
        {
            if (skillUseTime < 10.0f) //10초간 모든 피해&이동 방해 효과 면역
            {
                immuneDamage = true;
                immuneInterrupt = true;
                skillUseTime += Time.deltaTime;
            }
            else
            {
                immuneDamage = false;
                immuneInterrupt = false;
            }
        }
    }

    //엘리트 몬스터 스킬: 암흑신관
    void Elite_DarkPriest()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 1.0f) //1초마다
        {
            skillCoolTime1 = 0.0f;

            //자신을 제외한 일정 반경 내 몬스터 모두의 HP를 회복
            Monster monster;
            for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
            {
                monster = BattleManager.instance.monsters[i];
                if (monster.isInBattle && Vector2.Distance(transform.position, monster.transform.position) < 3.0f && monster != this)
                    monster.AE_DecreaseHP(-monster.maxHP * 0.05f, Color.green);
            }
        }
    }

    //엘리트 몬스터 스킬: 악마 지휘관
    void Elite_DevilCommander()
    {
        Monster monster;
        //자신을 제외한 일정 반경 내 몬스터 모두의 이속을 증가
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (monster.isInBattle && Vector2.Distance(transform.position, monster.transform.position) < 3.0f && monster != this)
                monster.baseSPD = monster.baseMonster.baseSPD * 1.2f;
            else monster.baseSPD = monster.baseMonster.baseSPD;
        }
    }

    //엘리트 몬스터 스킬: 정화자
    void Elite_Purifier()
    {
        immuneInterrupt = true;
    }

    //엘리트 몬스터 스킬: 포식자
    void Elite_Predator()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 5.0f)  //5초마다
        {
            skillCoolTime1 = 0.0f;

            BattleManager.instance.ChangePlayerRP(-BattleManager.instance.rp * 0.2f); //플레이어 RP 감소
        }
    }

    //보스 몬스터 스킬: 인형술사
    void Boss_Puppeteer()
    {
        Monster monster;
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)    //10초마다
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            //인형을 소환한다. 초기에는 생성한 인형을 멀리 떨어뜨려놓아야 path의 중간에서 글리치 하지 않는다. 인형의 스탯을 정한 후에는 자신의 조금 앞쪽에다 위치시킨다.
            monster = GameManager.instance.GetMonsterFromPool(29);
            monster.gameObject.transform.position = new Vector2(100, 100);  //
            monster.InitializeMonster(cloneID--, FloorManager.instance.curRoom.pathID, 1.0f);
            monster.movedDistance = movedDistance + 1.0f;
            monster.gameObject.SetActive(true);
            BattleManager.instance.monsters.Add(monster);
        }

        //만일 살아있는 인형이 있다면 데미지 면역이다.
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (monster.id < 0)
            {
                immuneDamage = true;
                return;
            }
        }
        immuneDamage = false;
    }

    //보스 몬스터 스킬: 분신술사
    void Boss_CloneMage()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)    //10초마다
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            //HP를 2/3으로 하는 분신 두 개로 나뉜다.
            Monster monster;
            float scale = 0.5f * (FloorManager.instance.floor.floorNum + 1) * 2.0f;

            monster = GameManager.instance.GetMonsterFromPool(23);
            monster.gameObject.transform.position = new Vector2(100, 100);
            monster.InitializeMonster(cloneID--, FloorManager.instance.curRoom.pathID, scale);

            monster.movedDistance = movedDistance + 0.5f;
            monster.curHP = curHP * 0.666f;
            monster.SetHPText();

            monster.gameObject.SetActive(true);
            BattleManager.instance.monsters.Add(monster);

            movedDistance -= 0.5f;
            curHP *= 0.666f;
            SetHPText();
        }
    }

    //보스 몬스터 스킬: 봉인술사
    void Boss_Sealer()
    {
        if (skillUseTime != 0)  //스킬 사용중이라면 5초 후에 봉인을 푼다.
        {
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 5.0f)
            {
                skillUseTime = 0.0f;
                skillCoolTime1 = 0.0f;
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) DeckManager.instance.rings[i].isSealed = false;
            }
        }
        else
        {
            skillCoolTime1 += Time.deltaTime;
            if (skillCoolTime1 >= 10.0f)    //10초 쿨타임을 기다려서 봉인을 건다.
            {
                skillUseTime = 0.001f;
                anim.SetTrigger("Attack");

                int targetSealNum = DeckManager.instance.rings.Count / 2;   //전체 링의 절반만큼 랜덤하게 봉인건다.
                int sealNum = 0;
                int tarIdx;
                while (sealNum < targetSealNum)
                {
                    do tarIdx = Random.Range(0, DeckManager.instance.rings.Count);
                    while (DeckManager.instance.rings[tarIdx].isSealed);
                    DeckManager.instance.rings[tarIdx].isSealed = true;
                    sealNum++;
                }
            }
        }
    }

    //보스 몬스터 스킬: 저주술사
    void Boss_DarkWarlock()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)    //10초마다
        {
            int ringID;

            //다운그레이드 할 수 있는 링이 있는지 확인한다.
            bool canDowngrade = false;
            for (int i = 0; i < DeckManager.instance.deck.Count; i++)
            {
                ringID = DeckManager.instance.deck[i];
                if (GameManager.instance.baseRings[ringID].level > 1)
                {
                    canDowngrade = true;
                    break;
                }
            }

            //다운그레이드가 가능하다면
            if (canDowngrade)
            {
                skillCoolTime1 = 0.0f;
                anim.SetTrigger("Attack");

                //랜덤한 링 하나를 다운그레이드하고, 링 생성 덱을 갱신한 뒤(업그레이드 표시를 바꾸기 위해), 다운그레이드 내역을 기록한다(전투 종료 후 50퍼 확률로 각각 복구하기 위해).
                do ringID = DeckManager.instance.deck[Random.Range(0, DeckManager.instance.deck.Count)];
                while (GameManager.instance.baseRings[ringID].level == 1);
                GameManager.instance.baseRings[ringID].Downgrade();
                UIManager.instance.OpenBattleDeckPanel();
                BattleManager.instance.ringDowngrade.Add(ringID);
            }
        }
    }

    //보스 몬스터 스킬: 공간분열자
    void Boss_Spacer()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)    //10초마다
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            //모든 일반몬스터들끼리 자리를 바꾼다.
            Monster monster1, monster2;
            int monsterNum = BattleManager.instance.monsters.Count;
            float tmp;
            for (int i = monsterNum - 1; i >= 0; i--)
            {
                monster1 = BattleManager.instance.monsters[i];
                monster2 = BattleManager.instance.monsters[Random.Range(0, monsterNum)];
                if (monster1.baseMonster.tier != 'n' || monster2.baseMonster.tier != 'n') continue;
                tmp = monster1.movedDistance;
                monster1.movedDistance = monster2.movedDistance;
                monster2.movedDistance = tmp;
            }
        }
    }

    //보스 몬스터 스킬: 처형인
    void Boss_Executer()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)    //10초마다
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            //랜덤한 링을 제거한다.
            DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.rings[Random.Range(0, DeckManager.instance.rings.Count)]);
        }
    }

    //보스 몬스터 스킬: 타락한 왕
    void Boss_King()
    {
        if (skillUseTime != 0)  //스킬(봉인) 사용중이면
        {
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 5.0f)    //봉인을 취소한다.
            {
                skillUseTime = 0.0f;
                skillCoolTime1 = 0.0f;
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) DeckManager.instance.rings[i].isSealed = false;
            }
        }
        else 
        {
            skillCoolTime1 += Time.deltaTime;
            if (skillCoolTime1 >= 10.0f) //10초마다 강력한 스킬을 사용한다.
            {
                skillCoolTime1 = 0.0f;
                anim.SetTrigger("Attack");

                bool canDowngrade = false;
                do
                {
                    switch (Random.Range(0, 3)) //다음 셋 중 하나를 성공할 때까지 적용한다.
                    {
                        case 0: //링 하나를 랜덤하게 제거한다.
                            DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.rings[Random.Range(0, DeckManager.instance.rings.Count)]);
                            canDowngrade = true;
                            break;
                        case 1: //링 하나를 랜덤하게 다운그레이드한다.
                            int ringID;
                            for (int i = 0; i < DeckManager.instance.deck.Count; i++)
                            {
                                ringID = DeckManager.instance.deck[i];
                                if (GameManager.instance.baseRings[ringID].level > 1)
                                {
                                    canDowngrade = true;
                                    break;
                                }
                            }
                            if (canDowngrade)
                            {
                                skillCoolTime1 = 0.0f;
                                do ringID = DeckManager.instance.deck[Random.Range(0, DeckManager.instance.deck.Count)];
                                while (GameManager.instance.baseRings[ringID].level == 1);
                                GameManager.instance.baseRings[ringID].Downgrade();
                                UIManager.instance.OpenBattleDeckPanel();
                                BattleManager.instance.ringDowngrade.Add(ringID);
                            }
                            break;
                        case 2: //링 절반을 봉인한다.
                            skillUseTime = 0.001f;

                            int targetSealNum = DeckManager.instance.rings.Count / 2;
                            int sealNum = 0;
                            int tarIdx;
                            while (sealNum < targetSealNum)
                            {
                                do tarIdx = Random.Range(0, DeckManager.instance.rings.Count);
                                while (DeckManager.instance.rings[tarIdx].isSealed);
                                DeckManager.instance.rings[tarIdx].isSealed = true;
                                sealNum++;
                            }
                            canDowngrade = true;
                            break;
                    }
                } while (!canDowngrade);
            }
        }

        skillCoolTime2 += Time.deltaTime;
        if (skillCoolTime2 >= 5.0f)     //5초마다 일반 스킬을 사용한다.
        {
            skillCoolTime2 = 0.0f;
            anim.SetTrigger("Attack");

            switch (Random.Range(0, 2))
            {
                case 0: //이동 방해 효과에 면역이다.
                    immuneInterrupt = true;
                    break;
                case 1: //잃은 HP의 일부를 회복한다.
                    immuneInterrupt = false;
                    AE_DecreaseHP(-(maxHP - curHP) * 0.05f, Color.green);
                    break;
            }
        }
    }

    //몬스터를 오브젝트 풀에 되돌릴 때 Invoke로 돌리는 경우 쓰인다.
    void InvokeReturnMonsterToPool()
    {
        GameManager.instance.ReturnMonsterToPool(this);
    }
}
