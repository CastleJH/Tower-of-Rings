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
    public float baseSPD;       //링 방해 효과 적용 전 속도
    public float curSPD;        //링 방해 효과 적용 후 속도

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
                    poisonTime = 0.0f;
                }
            }

            movedDistance += curSPD * Time.deltaTime;
            anim.speed = curSPD;

            prevX = transform.position.x;
            transform.position = path.path.GetPointAtDistance(movedDistance);
            transform.Translate(0.0f, 0.0f, id * 0.001f);

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

        //이동 변수
        path = GameManager.instance.monsterPaths[pathID];
        movedDistance = 0.0f;

        //그래픽
        //CopySPUMDataFromBase();
        SetHPText();

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

        curHP = 0;  //HP를 0으로 바꾼다.
        BattleManager.instance.monsters.Remove(this);
        if (isDead)
        {
            if (baseMonster.tier == 'b') BattleManager.instance.isBossKilled = true;
            anim.ResetTrigger("AddScene");
            anim.speed = 0.8f;
            anim.SetTrigger("Die");
            ApplyDeadEffect();
            Invoke("InvokeReturnMonsterToPool", 1.0f);
        }
        else InvokeReturnMonsterToPool();
    }

    void InvokeReturnMonsterToPool()
    {
        GameManager.instance.ReturnMonsterToPool(this);
    }

    //사망 효과를 적용한다. (골드 획득도 포함)
    void ApplyDeadEffect()
    {
        //저주 링의 사망시 폭발 효과를 적용한다.
        if (curseStack != 0) AE_CurseDead();

        //네크로 링의 사망을 카운팅한다.
        if (DeckManager.instance.necroIdx != -1)
        {

            if (DeckManager.instance.necroCount < 20)
            {
                DeckManager.instance.necroCount++;
                if (DeckManager.instance.necroCount >= 20)
                {
                    DeckManager.instance.necroCount = 20;
                    UIManager.instance.SetBattleDeckRingRPText(DeckManager.instance.necroIdx, "20/20");
                    BattleManager.instance.ChangePlayerRP(0);
                }
                else UIManager.instance.SetBattleDeckRingRPText(DeckManager.instance.necroIdx, DeckManager.instance.necroCount.ToString() + "/20");
            }
        }
    }

    //엘리트/보스의 스킬을 사용한다.
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

    //공격 이펙트: HP감소
    public void AE_DecreaseHP(float dmg, Color32 color)
    {
        if (dmg >= 0 || color != Color.green)
        {
            if (immuneDamage) dmg = 0;
            if (dmg != -1)
            {
                if (isInAmplify) dmg *= 1.2f;
                curHP -= dmg;
                if (curHP > maxHP) curHP = maxHP;
                DamageText t = GameManager.instance.GetDamageTextFromPool();
                t.InitializeDamageText((Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
                t.gameObject.SetActive(true);
            }
            else curHP = 0;
        }
        else
        {
            curHP -= dmg;
            if (curHP > maxHP) curHP = maxHP;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText((-Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
            t.gameObject.SetActive(true);
        }
        SetHPText();
        CheckDead();
    }

    //공격 이펙트: 눈꽃
    public void AE_Snow(float dmg, float time)
    {
        AE_DecreaseHP(dmg, new Color32(0, 0, 180, 255));
        //이미 받고있는 눈꽃 링의 둔화 효과가 더 오래 간다면 적용 취소
        if (snowEndTime - snowTime > time) return;
        snowEndTime = time;
        snowTime = 0;
    }

    //공격 이펙트: 폭발
    public void AE_Explosion(float dmg, float splash)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);

        Monster monster;
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i].tag == "Monster")
            {
                monster = colliders[i].GetComponent<Monster>();
                if (monster.curHP > 0 && monster != this) monster.AE_DecreaseHP(dmg * splash, new Color32(150, 30, 30, 255));
                
            }
        AE_DecreaseHP(dmg, new Color32(150, 30, 30, 255));
    }

    //공격 이펙트: 맹독
    public void AE_Poison(float dmg)
    {
        poisonDmg += dmg;
    }

    //공격 이펙트: 마비
    public void AE_Paralyze(float dmg, float time)
    {
        AE_DecreaseHP(dmg, new Color32(150, 150, 0, 255));
        //이미 받고있는 마비 링의 마비 효과가 더 오래 간다면 적용 취소
        if (paralyzeEndTime - paralyzeTime > time) return;
        paralyzeEndTime = time;
        paralyzeTime = 0;
    }

    //공격 이펙트: 귀환(텔레포트)
    public void AE_Teleport(float dmg, float prob)
    {
        AE_DecreaseHP(dmg, new Color32(255, 0, 200, 255));
        if (Random.Range(0.0f, 1.0f) < prob)
        {
            movedDistance = 0.0f;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText("귀환!", transform.position, new Color32(255, 0, 200, 255));
            t.gameObject.SetActive(true);
        }
    }

    //공격 이펙트: 절단
    public void AE_Cut(float dmg)
    {
        if (baseMonster.tier == 'n') AE_DecreaseHP(curHP * (dmg * 0.01f), Color.red);
        else AE_DecreaseHP(curHP * (dmg * 0.005f), Color.red);
    }

    //공격 이펙트: 저주
    public void AE_Curse(int stack)
    {
        curseStack += stack;
    }

    //공격 이펙트: 저주의 사망 효과
    public void AE_CurseDead()
    {
        float dmg = curseStack;
        dmg = maxHP * dmg * 0.01f;

        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            BattleManager.instance.monsters[i].PlayParticleCollision(21, 0.0f);
            BattleManager.instance.monsters[i].AE_DecreaseHP(dmg, new Color32(50, 50, 50, 255));
        }
    }

    //공격 이펙트: 처형
    public void AE_Execution(float dmg, float rate)
    {
        if (baseMonster.tier != 'n') rate *= 0.5f;
        if (curHP - dmg < maxHP * rate)
        {
            AE_DecreaseHP(-1, Color.red);
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText("처형!", transform.position, new Color32(70, 70, 70, 255));
            t.gameObject.SetActive(true);
        }
        else AE_DecreaseHP(dmg, new Color32(70, 70, 70, 255));
    }

    //공격 이펙트: 성장
    public void AE_Grow(float dmg, Ring ring)
    {
        AE_DecreaseHP(dmg, new Color32(30, 180, 30, 255));
        if (curHP <= 0 && ring.growStack < 20)
        {
            ring.growStack++;
            ring.ChangeCurATK(0.1f);
        }
    }

    //공격 이펙트: 천사
    public void AE_Angel()
    {
        AE_DecreaseHP(curHP * 0.2f, Color.yellow);
        movedDistance = 0;
    }

    //공격 이펙트: 추적
    public void AE_Chase(float dmg, float radius)
    {
        Monster monster;
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (Vector2.Distance(transform.position, monster.transform.position) < radius)
                monster.AE_DecreaseHP(dmg, new Color32(100, 0, 0, 255));
        }
    }

    //공격 이펙트: 즉사
    public void AE_InstantDeath(float dmg, float prob)
    {
        if (Random.Range(0.0f, 1.0f) < prob)
        {
            if (baseMonster.tier != 'n') AE_DecreaseHP(maxHP * prob * 0.5f, new Color32(80, 80, 80, 255));
            else
            {
                AE_DecreaseHP(-1, Color.black);
                DamageText t = GameManager.instance.GetDamageTextFromPool();
                t.InitializeDamageText("즉사!", transform.position, new Color32(80, 80, 80, 255));
                t.gameObject.SetActive(true);
            }
        }
        else AE_DecreaseHP(dmg, new Color32(80, 80, 80, 255));
    }

    void Elite_Berserker()
    {
        baseSPD = baseMonster.baseSPD * (1.0f + (maxHP - curHP) / maxHP);
    }

    void Elite_Messenger()
    {
        if (skillUseTime != 0)
        {
            baseSPD = baseMonster.baseSPD * 2;
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 2.0f)
            {
                skillUseTime = 0.0f;
                skillCoolTime1 = 0.0f;
            }
        }
        else
        {
            skillCoolTime1 += Time.deltaTime;
            if (skillCoolTime1 > 5.0f)
            {
                skillCoolTime1 = 0.0f;
                skillUseTime = 0.0001f;
            }
        }
    }

    void Elite_Giant()
    {
        if (curHP < maxHP * 0.2f)
        {
            if (skillUseTime < 5.0f)
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

    void Elite_DarkPriest()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 1.0f)
        {
            skillCoolTime1 = 0.0f;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            Monster monster;

            for (int i = 0; i < colliders.Length; i++)
                if (colliders[i].tag == "Monster")
                {
                    monster = colliders[i].GetComponent<Monster>();
                    if (monster.curHP > 0 && monster != this) monster.AE_DecreaseHP(-monster.maxHP * 0.05f, Color.green);
                }
        }
    }

    void Elite_DevilCommander()
    {
        Monster monster;
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (monster != this && Vector2.Distance(monster.transform.position, transform.position) < 1.5f)
                monster.baseSPD = monster.baseMonster.baseSPD * 1.2f;
            else monster.baseSPD = monster.baseMonster.baseSPD;
        }
    }

    void Elite_Purifier()
    {
        immuneInterrupt = true;
    }

    void Elite_Predator()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 5.0f)
        {
            skillCoolTime1 = 0.0f;

            BattleManager.instance.ChangePlayerRP(-BattleManager.instance.rp * 0.2f);
        }
    }

    void Boss_Puppeteer()
    {
        Monster monster;
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            monster = GameManager.instance.GetMonsterFromPool(29);
            monster.gameObject.transform.position = new Vector2(100, 100);  //초기에는 멀리 떨어뜨려놓아야 path의 중간에서 글리치 하지 않음.
            monster.InitializeMonster(cloneID--, FloorManager.instance.curRoom.pathID, 1.0f);    //그외에는 일반 몬스터
            monster.movedDistance = movedDistance + 1.0f;
            monster.gameObject.SetActive(true);
            BattleManager.instance.monsters.Add(monster);
        }

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

    void Boss_CloneMage()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            Monster monster;
            float scale;
            if (FloorManager.instance.floor.floorNum == 7) scale = 4.0f;
            else scale = 0.5f * (FloorManager.instance.floor.floorNum + 1);

            monster = GameManager.instance.GetMonsterFromPool(23);
            monster.gameObject.transform.position = new Vector2(100, 100);  //초기에는 멀리 떨어뜨려놓아야 path의 중간에서 글리치 하지 않음.
            monster.InitializeMonster(cloneID--, FloorManager.instance.curRoom.pathID, scale);    //그외에는 일반 몬스터

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

    void Boss_Sealer()
    {
        if (skillUseTime != 0)
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
            if (skillCoolTime1 >= 10.0f)
            {
                skillUseTime = 0.001f;
                anim.SetTrigger("Attack");

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
            }
        }
    }

    void Boss_DarkWarlock()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)
        {
            int ringID;
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
            if (canDowngrade)
            {
                skillCoolTime1 = 0.0f;
                anim.SetTrigger("Attack");

                do ringID = DeckManager.instance.deck[Random.Range(0, DeckManager.instance.deck.Count)];
                while (GameManager.instance.baseRings[ringID].level == 1);
                GameManager.instance.baseRings[ringID].Downgrade();
                UIManager.instance.OpenBattleDeckPanel();
                BattleManager.instance.ringDowngrade.Add(ringID);
            }
        }
    }

    void Boss_Spacer()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

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

    void Boss_Executer()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.rings[Random.Range(0, DeckManager.instance.rings.Count)]);
        }
    }

    void Boss_King()
    {
        if (skillUseTime != 0)
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
            if (skillCoolTime1 >= 10.0f)
            {
                skillCoolTime1 = 0.0f;
                anim.SetTrigger("Attack");

                bool canDowngrade = false;
                do
                {
                    switch (Random.Range(0, 3))
                    {
                        case 0:
                            DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.rings[Random.Range(0, DeckManager.instance.rings.Count)]);
                            canDowngrade = true;
                            break;
                        case 1:
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
                        case 2:
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
        if (skillCoolTime2 >= 5.0f)
        {
            skillCoolTime2 = 0.0f;
            anim.SetTrigger("Attack");

            switch (Random.Range(0, 2))
            {
                case 0:
                    immuneInterrupt = true;
                    break;
                case 1:
                    immuneInterrupt = false;
                    AE_DecreaseHP((maxHP - curHP) * 0.05f, Color.green);
                    break;
            }
        }
    }

    /*private void CopySPUMDataFromBase()
    {
        SPUM_Prefabs _baseData = Resources.Load<SPUM_Prefabs>(string.Format("SPUM/SPUM_Units/Unit{0:D3}", baseMonster.type));
        if (_baseData == null) Debug.LogError(string.Format("SPUM named Unit{0:D3} does not exist in Assets/Resources/SPUM/SPUM_Units", baseMonster.type));
        else
        {
            SyncSprites(spumSprites._itemList, _baseData._spriteOBj._itemList);
            SyncSprites(spumSprites._eyeList, _baseData._spriteOBj._eyeList);
            SyncSprites(spumSprites._hairList, _baseData._spriteOBj._hairList);
            SyncSprites(spumSprites._bodyList, _baseData._spriteOBj._bodyList);
            SyncSprites(spumSprites._clothList, _baseData._spriteOBj._clothList);
            SyncSprites(spumSprites._armorList, _baseData._spriteOBj._armorList);
            SyncSprites(spumSprites._pantList, _baseData._spriteOBj._pantList);
            SyncSprites(spumSprites._weaponList, _baseData._spriteOBj._weaponList);
            SyncSprites(spumSprites._backList, _baseData._spriteOBj._backList);
        }
    }

    private void SyncSprites(List<SpriteRenderer> _target, List<SpriteRenderer> _base)
    {
        for (int i = _base.Count - 1; i >= 0; i--)
        {
            _target[i].sprite = _base[i].sprite;
            _target[i].color = _base[i].color;
        }
    }*/
}
