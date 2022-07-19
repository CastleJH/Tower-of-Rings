using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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

    //그래픽
    SpriteRenderer spriteRenderer;  //몬스터 이미지
    public TextMesh hpText;   //HP 텍스트

    //기타 변수
    bool isInBattle;     //살아서 전투에 참여중인지 여부
    float snowTime;         //눈꽃의 슬로우 효과가 지속된 시간
    float snowEndTime;      //눈꽃의 슬로우 효과가 끝나는 시간
    Dictionary<int, float> poisonDmg;   //맹독의 데미지
    Dictionary<int, float> poisonTime;  //맹독의 데미지 쿨타임
    public bool isInBlizzard; //눈보라 속에 있는지 여부
    float paralyzeTime;         //마비의 마비 효과가 지속된 시간
    float paralyzeEndTime;      //마비의 마비 효과가 끝나는 시간
    public bool barrierBlock; //결계로부터 이동을 방해받지 않는지 여부
    public int curseStack;     //저주로부터 쌓인 스택
    public bool isInAmplify;    //증폭 범위 내에 있는지 여부
    float skillCoolTime;    //엘리트/보스 몬스터의 스킬 쿨타임
    float skillUseTime;     //엘리트/보스 몬스터의 스킬 지속된 시간
    bool immuneDamage;
    bool immuneInterrupt;
    int cloneID;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        poisonTime = new Dictionary<int, float>();
        poisonDmg = new Dictionary<int, float>();
    }

    void Update()
    {
        if (isInBattle)
        {
            baseSPD = baseMonster.spd;
            if (!IsNormalMonster()) UseMonsterSkill();
            curSPD = baseSPD;

            spriteRenderer.color = Color.white; //색깔(상태 이상 표시 용)을 초기화

            if (!immuneInterrupt)
            {
                //이동방해는 약한순->강한순으로 적용되어야 한다.
                if (isInBlizzard) //눈보라 속이라면
                {
                    curSPD = baseSPD * 0.7f;
                    spriteRenderer.color = Color.cyan;
                }

                if (snowTime < snowEndTime) //눈꽃 링의 둔화가 적용중이라면
                {
                    curSPD = baseSPD * 0.5f;
                    snowTime += Time.deltaTime;
                    spriteRenderer.color = Color.cyan;
                }

                if (paralyzeTime < paralyzeEndTime) //마비 링의 마비가 적용중이라면
                {
                    curSPD = 0;
                    paralyzeTime += Time.deltaTime;
                    spriteRenderer.color = Color.yellow;
                }

                if (barrierBlock) curSPD = 0;
            }

            if (poisonDmg.Count > 0)  //맹독 중첩이 하나라도 있다면
            {
                spriteRenderer.color = Color.green;
                List<int> list = poisonDmg.Keys.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    int key = list[i];
                    poisonTime[key] += Time.deltaTime;
                    if (poisonTime[key] > 1.0f)
                    {
                        AE_DecreaseHP(poisonDmg[key], new Color32(0, 100, 0, 255));
                        PlayParticleCollision(5, 0.0f);
                        poisonTime[key] = 0.0f;
                    }
                }
            }

            if (curseStack != 0) spriteRenderer.color = Color.gray;     //저주 중첩이 하나라도 있다면

            movedDistance += curSPD * Time.deltaTime;
            transform.position = path.path.GetPointAtDistance(movedDistance);
            transform.Translate(0.0f, 0.0f, id * 0.001f);
        }
    }

    //몬스터를 초기화한다.
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

        //그래픽
        spriteRenderer.sprite = GameManager.instance.monsterSprites[baseMonster.type];
        spriteRenderer.color = Color.white; //색깔(상태 이상 표시 용)을 초기화
        SetHPText();

        //기타 변수
        isInBattle = true;
        snowEndTime = -1.0f;
        poisonDmg.Clear();
        poisonTime.Clear();
        isInBlizzard = false;
        paralyzeEndTime = -1.0f;
        barrierBlock = false;
        curseStack = 0;
        isInAmplify = false;
        skillCoolTime = 0.0f;
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
            ApplyDeadEffect();
            Invoke("InvokeReturnMonsterToPool", 0.5f);
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
                    BattleManager.instance.ChangeCurrentRP(BattleManager.instance.rp);
                }
                else UIManager.instance.SetBattleDeckRingRPText(DeckManager.instance.necroIdx, DeckManager.instance.necroCount.ToString() + "/20");
            }
        }

        //보스 몬스터면 20골드 획득한다. 일반 몬스터면 66%확률로 1, 33%확률로 2골드 획득한다.
        int g;
        if (IsNormalMonster()) g = Mathf.Clamp(Random.Range(0, 3), 1, 2);
        else g = 20;
        BattleManager.instance.goldGet += g;
    }

    //일반 몬스터인지/엘리트 몬스터인지 알려준다.
    public bool IsNormalMonster()
    {
        if (baseMonster.type < 3) return true;
        else return false;
    }

    //엘리트/보스의 스킬을 사용한다.
    void UseMonsterSkill()
    {
        switch (baseMonster.type)
        {
            case 3:
                Elite_Berserker();
                break;
            case 4:
                Elite_Messenger();
                break;
            case 5:
                Elite_Giant();
                break;
            case 6:
                Elite_DarkPriest();
                break;
            case 7:
                Elite_DevilCommander();
                break;
            case 8:
                Elite_Purifier();
                break;
            case 9:
                Elite_Predator();
                break;
            case 10:
                Boss_Puppeteer();
                break;
            case 11:
                Boss_Slime();
                break;
            case 12:
                Boss_Sealer();
                break;
            case 13:
                Boss_DarkWarlock();
                break;
            case 14:
                Boss_Executer();
                break;
            case 15:
                Boss_Spacer();
                break;
            case 16:
                Boss_King();
                break;
        }
    }

    //공격 이펙트: HP감소
    public void AE_DecreaseHP(float dmg, Color32 color)
    {
        if (immuneDamage) dmg = 0;
        if (dmg != -1)
        {
            if (isInAmplify) dmg *= 1.2f;
            curHP -= dmg;
            if (dmg < 0) dmg *= -1;
            if (curHP > baseHP) curHP = baseHP;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText((Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
            t.gameObject.SetActive(true);
        }
        else curHP = 0;
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
    public void AE_Poison(int ringNumber, float dmg)
    {

        if (poisonDmg.ContainsKey(ringNumber))
        {
            poisonDmg[ringNumber] += dmg;
        }
        else
        {
            poisonDmg.Add(ringNumber, dmg);
            poisonTime.Add(ringNumber, 1.0f);
        }
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
        if (IsNormalMonster()) AE_DecreaseHP(curHP * (dmg * 0.01f), Color.red);
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
        dmg = baseHP * dmg * 0.01f;

        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            BattleManager.instance.monsters[i].PlayParticleCollision(21, 0.0f);
            BattleManager.instance.monsters[i].AE_DecreaseHP(dmg, new Color32(50, 50, 50, 255));
        }
    }

    //공격 이펙트: 처형
    public void AE_Execution(float dmg, float rate)
    {
        if (!IsNormalMonster()) rate *= 0.5f;
        if (curHP - dmg < baseHP * rate)
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
            if (!IsNormalMonster()) AE_DecreaseHP(baseHP * prob * 0.5f, new Color32(80, 80, 80, 255));
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
        baseSPD = baseMonster.spd * (1.0f + (baseHP - curHP) / baseHP);
    }

    void Elite_Messenger()
    {
        if (skillUseTime != 0)
        {
            baseSPD = baseMonster.spd * 2;
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 2.0f)
            {
                skillUseTime = 0.0f;
                skillCoolTime = 0.0f;
            }
        }
        else
        {
            skillCoolTime += Time.deltaTime;
            if (skillCoolTime > 5.0f)
            {
                skillCoolTime = 0.0f;
                skillUseTime = 0.0001f;
            }
        }
    }

    void Elite_Giant()
    {
        if (curHP < baseHP * 0.2f)
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
        skillCoolTime += Time.deltaTime;
        if (skillCoolTime >= 1.0f)
        {
            skillCoolTime = 0.0f;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            Monster monster;

            for (int i = 0; i < colliders.Length; i++)
                if (colliders[i].tag == "Monster")
                {
                    monster = colliders[i].GetComponent<Monster>();
                    if (monster.curHP > 0 && monster != this) monster.AE_DecreaseHP(-monster.baseHP * 0.05f, new Color32(0, 255, 0, 255));
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
                monster.baseSPD = monster.baseMonster.spd * 1.2f;
            else monster.baseSPD = monster.baseMonster.spd;
        }
    }

    void Elite_Purifier()
    {
        immuneInterrupt = true;
    }

    void Elite_Predator()
    {
        skillCoolTime += Time.deltaTime;
        if (skillCoolTime >= 5.0f)
        {
            skillCoolTime = 0.0f;

            BattleManager.instance.ChangeCurrentRP((int)BattleManager.instance.rp * 0.8f);
        }
    }

    void Boss_Puppeteer()
    {
        Monster monster;
        skillCoolTime += Time.deltaTime;
        if (skillCoolTime >= 10.0f)
        {
            skillCoolTime = 0.0f;

            monster = GameManager.instance.GetMonsterFromPool();
            monster.gameObject.transform.position = new Vector2(100, 100);  //초기에는 멀리 떨어뜨려놓아야 path의 중간에서 글리치 하지 않음.
            monster.InitializeMonster(cloneID--, GameManager.instance.monsterDB[17], FloorManager.instance.curRoom.pathID, 1.0f);    //그외에는 일반 몬스터
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

    void Boss_Slime()
    {
        Monster monster;
        skillCoolTime += Time.deltaTime;
        if (skillCoolTime >= 10.0f)
        {
            skillCoolTime = 0.0f;

            float scale;
            if (FloorManager.instance.floor.floorNum == 7) scale = 4.0f;
            else scale = 0.5f * (FloorManager.instance.floor.floorNum + 1);

            monster = GameManager.instance.GetMonsterFromPool();
            monster.gameObject.transform.position = new Vector2(100, 100);  //초기에는 멀리 떨어뜨려놓아야 path의 중간에서 글리치 하지 않음.
            monster.InitializeMonster(cloneID--, GameManager.instance.monsterDB[11], FloorManager.instance.curRoom.pathID, scale);    //그외에는 일반 몬스터

            monster.movedDistance = movedDistance + 0.5f;
            monster.curHP *= 0.666f;
            SetHPText();

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
            baseSPD = baseMonster.spd * 2;
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 5.0f)
            {
                skillUseTime = 0.0f;
                skillCoolTime = 0.0f;
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) DeckManager.instance.rings[i].isSealed = false;
            }
        }
        else
        {
            skillCoolTime += Time.deltaTime;
            if (skillCoolTime >= 10.0f)
            {
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
            }
        }
    }

    void Boss_DarkWarlock()
    {
        skillCoolTime += Time.deltaTime;
        if (skillCoolTime >= 10.0f)
        {
            int ringID;
            bool canDowngrade = false;
            for (int i = 0; i < DeckManager.instance.deck.Count; i++)
            {
                ringID = DeckManager.instance.deck[i];
                if (GameManager.instance.ringstoneDB[ringID].level > 1)
                {
                    canDowngrade = true;
                    break;
                }
            }
            if (canDowngrade)
            {
                skillCoolTime = 0.0f;
                do ringID = DeckManager.instance.deck[Random.Range(0, DeckManager.instance.deck.Count)];
                while (GameManager.instance.ringstoneDB[ringID].level == 1);
                GameManager.instance.ringstoneDB[ringID].Downgrade();
                BattleManager.instance.ringDowngrade.Add(ringID);
            }
        }
    }

    void Boss_Executer()
    {

    }

    void Boss_Spacer()
    {

    }

    void Boss_King()
    {

    }
}
