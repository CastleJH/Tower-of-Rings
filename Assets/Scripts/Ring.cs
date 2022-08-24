using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ring : MonoBehaviour
{
    int number;    //링 구분에 쓰임
    public BaseRing baseRing;  //링의 기본 정보

    //스탯
    public float curNumTarget;  //타겟 수
    public float curATK;        //현재 데미지
    public float curSPD;        //현재 공격 쿨타임
    public float curEFF;        //현재 효과 지속 시간
    public float buffNumTarget; //타겟 수 변화량(그대로 더하기 한다. 0.0f로 시작)
    public float buffATK;       //데미지 변화율(그대로 곱하기 한다. 1.0f로 시작)
    public float buffSPD;       //공격 쿨타임 변화율(그대로 곱하기 한다. 1.0f로 시작. 0.2f보다 작아도 0.2f를 곱함)
    public float buffEFF;       //효과 지속시간 변화율(그대로 곱하기 한다. 1.0f로 시작)
    public float buffEFFPlus;   //효과 지속시간 변화율(그대로 더하기 한다. 0.0f로 시작)

    //타겟
    public List<Monster> targets;   //공격범위 내의 몬스터들
    public List<Ring> nearRings;    //공격범위 내의 링들(시너지 적용 대상)

    //사운드
    public AudioSource audioSource; //공격 사운드

    //그래픽
    public SpriteRenderer spriteRenderer;   //링 자체 렌더러
    public SpriteRenderer rangeRenderer;    //공격 범위 표시 렌더러
    ParticleChecker particle;               //범용 파티클
    public Animator anim;

    //개별 링 변수
    int oxyRemoveCount;             //산화 링의 소멸 카운트
    float explosionSplash;          //폭발 링의 스플래쉬 데미지(비율)
    Blizzard blizzard;              //눈보라
    int curseStack;                 //저주 링의 공격 당 쌓는 스택
    Amplifier amplifier;            //증폭
    float executionRate;            //처형 기준 HP 비율
    public int growStack;           //성장링의 공격력 증가 스택
    float chaseAttackRadius;        //추격의 피해 범위

    //기타 공통 변수
    public bool isInBattle;         //전투 중인지 체크용
    public float shootCoolTime;     //발사 쿨타임 체크용
    public CircleCollider2D ringCollider; //자신의 콜라이더
    public bool isSealed;           //보스 몬스터로 인한 봉인 여부
    public Ring commanderNearest;   //가장 근처의 사령관 링
    public Monster commanderTarget; //가장 근처 사령관 링의 타겟

    void Awake()
    {
        targets = new List<Monster>();
        ringCollider = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        if (isInBattle)
        {
            if (!isSealed)  //봉인되지 않았다면
            {
                shootCoolTime += Time.deltaTime;
                if (shootCoolTime > curSPD)     //공격 쿨타임이 되었다면
                {
                    switch (baseRing.id)
                    {
                        case 7:     //RP 생산
                            GenerateRP(curATK);
                            anim.SetTrigger("isShoot");
                            shootCoolTime = 0.0f;
                            break;
                        case 10:    //HP 회복
                            GameManager.instance.ChangePlayerCurHP(1);
                            PlayParticle(10);
                            anim.SetTrigger("isShoot");
                            shootCoolTime = 0.0f;
                            break;
                        case 11:    //아무것도 하지 않는다.
                        case 17:
                        case 23:
                            break;
                        case 22:    //제자리에서 폭발
                            BombardAttack();
                            break;
                        case 32:    //3개인 경우만 공격
                            if (DeckManager.instance.sleepActivated == 3) TryShoot();
                            break;
                        default:    //공격
                            TryShoot();
                            break;
                    }
                }
            }
        }
        else
        {
            if (DeckManager.instance.genRing != this)   //전투중도 아니고 생성 중인 링도 아닌데 scene에 있는거면 반드시 제거
                GameManager.instance.ReturnRingToPool(this);
        }
    }

    //링 배치 단계를 위해 id에 해당하는 링으로 변한다(일단 베이스랑 그래픽 변수만 올바르게 바꾼다).
    public void InitializeRing(int id)
    {
        //베이스 정보 얻기
        baseRing = GameManager.instance.baseRings[id];

        //그래픽
        transform.localScale = Vector3.one;
        spriteRenderer.sprite = GameManager.instance.ringSprites[id];
        spriteRenderer.transform.localScale = Vector3.one;
        rangeRenderer.transform.localScale = new Vector2(baseRing.range * 2, baseRing.range * 2);
        rangeRenderer.color = new Color(0, 0, 0, 0);
        transform.localScale = Vector3.one;

        //콜라이더 끄기
        ringCollider.enabled = false;
    }

    //실제로 게임에 넣는다.
    public void PutIntoBattle(int _number)
    {
        number = _number;

        //각 링 별로 필요한 변수만 초기화한다.
        switch (baseRing.id)    
        {
            case 2:
                oxyRemoveCount = 20;
                break;
            case 4:
                explosionSplash = 0.5f;
                break;
            case 11:
                blizzard = GameManager.instance.GetBlizzardFromPool();
                blizzard.InitializeBlizzard(this);
                blizzard.gameObject.SetActive(true);
                break;
            case 21:
                curseStack = 1;
                break;
            case 23:
                amplifier = GameManager.instance.GetAmplifierFromPool();
                amplifier.InitializeAmplifier(this);
                amplifier.gameObject.SetActive(true);
                break;
            case 24:
                executionRate = 0.1f;
                break;
            case 26:
                growStack = 0;
                break;
            case 28:
                chaseAttackRadius = 1.5f;
                break;
            default:
                break;
        }

        //기타 공통 변수를 초기화한다.
        isInBattle = true;
        shootCoolTime = baseRing.baseSPD - 0.2f;
        rangeRenderer.color = new Color(0, 0, 0, 0);
        ringCollider.enabled = true;
        isSealed = false;
        commanderNearest = null;
        commanderTarget = null;

        //버프 관련 변수를 초기화한다.
        buffNumTarget = 0.0f;
        buffATK = 1.0f;
        buffSPD = 1.0f;
        buffEFF = 1.0f;
        buffEFFPlus = 0.0f;

        //귀족의 인장 유물이 있으면 5번째 링마다 공격력을 증감해준다.
        if (GameManager.instance.baseRelics[17].have && (number + 1) % 5 == 0)
        {
            if (GameManager.instance.baseRelics[17].isPure) buffATK = 1.1f;
            else buffATK = 0.8f;
        }

        //시너지를 적용한다.
        ApplySynergy();
    }

    //불렛을 발사할 수 있다면 발사한다.
    public void TryShoot()
    {
        GetTargets();
        int numTarget = Mathf.Min(targets.Count, (int)curNumTarget);    //범위 내 몬스터 수 or 나의 최대 타겟 수 중 더 작은 것
        if (numTarget != 0)
        {
            if (baseRing.id != 19) audioSource.PlayOneShot(GameManager.instance.ringAttackAudios[baseRing.id]);
            Bullet bullet;
            switch (baseRing.id)
            {
                case 0: //리스트의 가장 앞쪽부터 타겟 만큼 쏨
                case 3:
                case 6:
                case 8:
                case 9:
                case 13:
                case 16:
                case 25:
                case 27:
                case 30:
                case 31:
                case 32:
                    targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                    for (int i = 0; i < numTarget; i++)
                    {
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                    }
                    break;
                case 1: //리스트의 가장 앞쪽 한 개만 쏨
                case 4:
                    float maxDist = 0.0f;
                    int mIdx = -1;
                    for (int i = 0; i < targets.Count; i++)
                        if (maxDist < targets[i].movedDistance)
                        {
                            maxDist = targets[i].movedDistance;
                            mIdx = i;
                        }
                    if (mIdx == -1) return;
                    bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                    bullet.InitializeBullet(this, targets[mIdx]);
                    bullet.gameObject.SetActive(true);
                    break;
                case 2: //산화 링의 소멸 카운팅
                    if (oxyRemoveCount-- == 0)
                    {
                        DeckManager.instance.RemoveRingFromBattle(this);
                        return;
                    }
                    targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                    for (int i = 0; i < numTarget; i++)
                    {
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                    }
                    break;
                case 5: //랜덤한 갯수만큼 공격
                case 14:
                case 20:
                case 24:
                case 26:
                case 29:
                    if (commanderNearest != null && commanderNearest.commanderTarget != null && Random.Range(0.0f, 1.0f) < commanderNearest.curEFF) //사령관 대상인 경우
                    {
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, commanderNearest.commanderTarget);
                        bullet.gameObject.SetActive(true);
                        numTarget--;
                        if (targets.Contains(commanderNearest.commanderTarget))
                            targets.Remove(commanderNearest.commanderTarget); //앞으로의 공격대상에서 해당 대상은 뺀다.

                    }
                    for (int i = 0; i < numTarget; i++)
                    {
                        int tar = Random.Range(0, targets.Count);
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, targets[tar]);
                        bullet.gameObject.SetActive(true);
                        targets.RemoveAt(tar);
                    }
                    break;
                case 12: //HP 높은 순으로 타겟만큼 공격
                case 21:
                    targets = targets.OrderByDescending(x => x.curHP).ToList();
                    for (int i = 0; i < numTarget; i++)
                    {
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                    }
                    break;
                case 15: //엘리트/보스 먼저 공격
                    targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                    for (int i = 0; i < targets.Count && numTarget != 0; i++)
                    {
                        if (targets[i].baseMonster.tier == 'n') continue;
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                        numTarget--;
                    }
                    for (int i = 0; i < targets.Count && numTarget != 0; i++)
                    {
                        if (targets[i].baseMonster.tier != 'n') continue;
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                        numTarget--;
                    }
                    break;
                case 18: //랜덤한 갯수만큼 결계 생성
                    Barrier barrier;
                    if (Random.Range(0.0f, 1.0f) < 0.8f && commanderNearest != null && commanderNearest.commanderTarget != null) //사령관 대상인 경우
                    {
                        barrier = GameManager.instance.GetBarrierFromPool();
                        barrier.InitializeBarrier(curEFF, commanderNearest.commanderTarget.transform.position);
                        barrier.gameObject.SetActive(true);
                        if (targets.Contains(commanderNearest.commanderTarget))
                        {
                            targets.Remove(commanderNearest.commanderTarget); //앞으로의 공격대상에서 해당 대상은 뺀다.
                            if (numTarget == targets.Count) numTarget--;
                        }
                    }
                    for (int i = 0; i < numTarget; i++)
                    {
                        int tar = Random.Range(0, targets.Count);
                        barrier = GameManager.instance.GetBarrierFromPool();
                        barrier.InitializeBarrier(curEFF, targets[tar].transform.position);
                        barrier.gameObject.SetActive(true);
                        targets.RemoveAt(tar);
                    }
                    break;
                case 19: //사령관의 대상 고르기
                    commanderTarget = null;
                    for (int i = 0; i < targets.Count; i++)
                        if (commanderTarget == null || commanderTarget.movedDistance < targets[i].movedDistance)
                            commanderTarget = targets[i];
                    break;
                case 28: //주변에 가장 몬스터가 많이 있는 몬스터 고르기
                    int maxID = -1;
                    int maxHave = 0;
                    targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                    for (int j = 0; j < targets.Count; j++)
                    {
                        int tmpHave = 0;
                        for (int k = j - 1; k >= 0; k--)
                            if (Vector2.Distance(targets[k].transform.position, targets[j].transform.position) < chaseAttackRadius) tmpHave++;
                            else break;
                        for (int k = j; k < targets.Count; k++)
                            if (Vector2.Distance(targets[k].transform.position, targets[j].transform.position) < chaseAttackRadius) tmpHave++;
                            else break;
                        if (tmpHave > maxHave)
                        {
                            maxHave = tmpHave;
                            maxID = j;
                        }
                    }
                    bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                    bullet.InitializeBullet(this, targets[maxID]);
                    bullet.gameObject.SetActive(true);
                    break;
                case 7: //발사 안함
                case 10:
                case 11:
                case 17:
                case 22:
                case 23:
                    break;
                default:
                    Debug.Log(string.Format("Not implemented yet. {0} TryShoot", baseRing.id.ToString()));
                    break;
            }
            shootCoolTime = 0.0f;
            if (baseRing.id != 19) anim.SetTrigger("isShoot");
        }
    }

    //불렛이 몬스터에 닿았을 때의 효과를 정한다.
    public void AttackEffect(Monster monster)
    {
        switch (baseRing.id)
        {
            case 0: //단순 공격
            case 2:
            case 9:
            case 12:
            case 14:
            case 15:
            case 25:
            case 27:
            case 30:
            case 31:
            case 32:
                monster.AE_DecreaseHP(curATK, new Color32(100, 0, 0, 255));
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 1:
                GetTargets();
                targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                int numTarget = Mathf.Min(targets.Count, (int)curNumTarget) - 1;
                for (int i = 0; i < targets.Count && numTarget != 0; i++)
                    if (targets[i] != monster)
                    {
                        targets[i].AE_DecreaseHP(curATK, Color.yellow);
                        targets[i].PlayParticleCollision(baseRing.id, 0.0f);
                        numTarget--;
                    }
                monster.AE_DecreaseHP(curATK, Color.yellow);
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 3:
                monster.AE_Snow(curATK, curEFF);
                monster.PlayParticleCollision(baseRing.id, curEFF);
                break;
            case 4:
                monster.AE_Explosion(curATK, explosionSplash);
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 5:
                monster.AE_Poison(curATK);
                break;
            case 6:
                monster.AE_DecreaseHP(Random.Range(0.0f, curATK), new Color32(255, 0, 100, 255));
                break;
            case 8:
                monster.AE_DecreaseHP(curATK, new Color32(100, 0, 0, 255));
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                GenerateRP(curATK);
                break;
            case 13:
                monster.AE_Paralyze(curATK, curEFF);
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 16:
                monster.AE_Teleport(curATK, curEFF);
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 20:
                monster.AE_Cut(curATK);
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 21:
                monster.AE_Curse(curseStack);
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 24:
                monster.AE_Execution(curATK, executionRate);
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 26:
                monster.AE_Grow(curATK, this);
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 28:
                monster.AE_Chase(curATK * (1.0f + (curNumTarget * 0.5f)), chaseAttackRadius);
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 29:
                monster.AE_InstantDeath(curATK, curEFF);
                monster.PlayParticleCollision(baseRing.id, 0.0f);
                break;
            case 10:
            case 17:
            case 18:    //아무것도 없음
            case 19:
            case 22:
                break;
            default:
                Debug.Log(string.Format("Not implemented yet. {0} AttackEffect", baseRing.id.ToString()));
                break;
        }
    }

    //모든 링들과 시너지를 주고받는다. 반드시 자신의 생성 시점 "단 한 번"만 불려야 한다.
    void ApplySynergy()
    {
        Ring ring;

        ChangeCurNumTarget(0.0f);
        ChangeCurATK(0.0f);
        ChangeCurSPD(0.0f);
        ChangeCurEFF(0.0f, '*');

        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= baseRing.range + ring.ringCollider.radius) //다른 링에 대하여 나의 효과를 적용
                ApplyMySynergyToOther(ring, true);
            if (Vector2.Distance(ring.transform.position, transform.position) <= ring.baseRing.range + ringCollider.radius) //다른 링으로부터 나에게 효과를 적용
                ring.ApplyMySynergyToOther(this, true);
        }
    }

    //내 시너지를 다른 ring에게 적용한다. isPlus == true면 시너지를 주는 것이고, false면 적용했던 시너지를 제거하는 것이다.
    public void ApplyMySynergyToOther(Ring ring, bool isPlus)
    {
        float mult = isPlus ? 1.0f : -1.0f;
        switch (baseRing.id)
        {
            case 0: //공 10 공 5
            case 6:
            case 7:
            case 11:
            case 15:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurATK(mult * 0.1f);
                ring.ChangeCurATK(mult * 0.05f);
                break;
            case 1: //타 1 타 0.5
            case 20:
            case 26:
            case 30:
            case 31:
            case 32:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurNumTarget(mult * 1.0f);
                ring.ChangeCurNumTarget(mult * 0.5f);
                break;
            case 8: //속 -15 속 -8
            case 14:
            case 25:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurSPD(mult * -0.15f);
                ring.ChangeCurSPD(mult * -0.08f);
                break;
            case 3: //효 +0.5 효 +*0.05
            case 18:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(mult * 0.5f, '+');
                ring.ChangeCurEFF(mult * 0.05f, '*');
                break;
            case 4:
                if (ring.baseRing.id == baseRing.id) ring.explosionSplash += mult * 0.05f;
                ring.ChangeCurATK(mult * 0.05f);
                break;
            case 5:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurATK(mult * 0.5f);
                ring.ChangeCurSPD(mult * -0.08f);
                break;
            case 9:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurATK(mult * 0.2f);
                ring.ChangeCurATK(mult * 0.05f);
                break;
            case 12:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurATK(mult * 0.15f);
                ring.ChangeCurATK(mult * 0.05f);
                break;
            case 13:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(mult * 0.1f, '+');
                ring.ChangeCurEFF(mult * 0.05f, '*');
                break;
            case 16:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(mult * 0.01f, '+');
                ring.ChangeCurEFF(mult * 0.05f, '*');
                break;
            case 21:
                if (ring.baseRing.id == baseRing.id) ring.curseStack += (int)mult * 1;
                ring.ChangeCurSPD(mult * -0.08f);
                break;
            case 24:
                if (ring.baseRing.id == baseRing.id) ring.executionRate += mult * 0.02f;
                ring.ChangeCurSPD(mult * -0.08f);
                break;
            case 28:
                if (ring.baseRing.id == baseRing.id) ring.chaseAttackRadius += mult * 0.15f;
                ring.ChangeCurATK(mult * 0.05f);
                break;
            case 29:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(mult * 0.02f, '+');
                ring.ChangeCurEFF(mult * 0.05f, '*');
                break;
            case 2: //효과 없음
            case 10:
            case 17:
            case 19:
            case 22:
            case 23:
            case 27:
                break;
            default: //구현 안된 것이 있으면 안된다.
                Debug.LogError("no synergy");
                break;
        }
    }

    //전투에서 제거시 일어나는 변화를 적용한다.
    public void ApplyRemoveEffect()
    {
        //내가 적용했던 시너지들을 제거한다.
        Ring ring;
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= baseRing.range + ring.ringCollider.radius)
                ApplyMySynergyToOther(ring, false);
        }

        //링 별로 제거 효과를 적용한다.
        switch (baseRing.id)
        {
            case 11:
                blizzard.RemoveFromBattle();
                blizzard = null;
                break;
            case 23:
                amplifier.RemoveFromBattle();
                amplifier = null;
                break;
            case 32:
                DeckManager.instance.sleepActivated--;
                break;
            default:
                break;
        }
    }

    //배치 가능한지 확인한다(RP가 충분한지, 근처에 겹치는 물체가 없는지). 배치 가능하면 true, 불가능하면 false 반환.
    public bool CanBePlaced()
    {
        //RP가 충분한지 확인한다.
        int deckIdx = DeckManager.instance.deck.IndexOf(baseRing.id);
        int rpCost;
        if (!int.TryParse(UIManager.instance.battleDeckRingRPText[deckIdx].text, out rpCost)) rpCost = 0;
        if (rpCost > BattleManager.instance.rp)
        {
            UIManager.instance.SetBattleArrangeFail("RP가 부족합니다.");
            rangeRenderer.color = new Color32(255, 0, 0, 50);
            return false;
        }
        
        //근처에 겹치는 물체가 있는지 확인한다.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.7f);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Monster Path")
            {
                UIManager.instance.SetBattleArrangeFail("올바르지 않은 위치입니다.");
                rangeRenderer.color = new Color32(255, 0, 0, 50);
                return false;
            }
            else if (colliders[i].tag == "Ring")
            {
                UIManager.instance.SetBattleArrangeFail("다른 링과 너무 가깝습니다.");
                rangeRenderer.color = new Color32(255, 0, 0, 50);
                return false;
            }
        }

        //범위 표시기의 색깔을 초록색으로 바꾼다.
        rangeRenderer.color = new Color32(0, 255, 0, 50);
        UIManager.instance.SetBattleArrangeFail(null);
        return true;
    }

    //공격 타겟들을 얻는다.
    public void GetTargets()
    {
        targets.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, baseRing.range);

        Monster monster;
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i].tag == "Monster")
            {
                monster = colliders[i].GetComponent<Monster>();
                if (monster.curHP > 0) targets.Add(monster);
            }
    }

    //타겟 개수를 증감한다.
    public void ChangeCurNumTarget(float buff)
    {
        buffNumTarget += buff;
        curNumTarget = baseRing.baseNumTarget + buffNumTarget;
        if (curNumTarget < 0) curNumTarget = 0;
    }

    //공격력을 증감한다.
    public void ChangeCurATK(float buff)
    {
        buffATK += buff;
        if (buffATK > 0) curATK = baseRing.baseATK * buffATK;   //공격력이 음수가 될 수는 없다.
        else curATK = 0;
    }

    //공격 쿨타임을 증감한다.
    public void ChangeCurSPD(float buff)
    {
        buffSPD += buff;
        if (buffSPD > 0.2f) curSPD = baseRing.baseSPD * buffSPD;   //공격 쿨타임이 기본의 20퍼센트 미만이 될 수는 없다.
        else curSPD = baseRing.baseSPD * 0.2f;
    }

    //효과 지속시간/확률을 증감한다. (mulOrPlus가 '*'이면 곱연산, '+'이면 합연산이다)
    public void ChangeCurEFF(float buff, char mulOrPlus)
    {
        if (mulOrPlus == '*') buffEFF += buff;
        else buffEFFPlus += buff;
        curEFF = baseRing.baseEFF * buffEFF + buffEFFPlus;
        if (curEFF < 0) curEFF = 0; //효과 지속시간이 음수가 될 수는 없다.
    }

    //RP를 생산한다.
    void GenerateRP(float genRP)
    {
        //RP를 생산하고 UI를 업데이트 한다.
        BattleManager.instance.ChangePlayerRP(genRP);

        //현재 링 위치에 파티클을 플레이한다.
        PlayParticle(7);
    }

    //폭격 링의 공격. 링 범위 내 모든 적을 공격한다. 
    void BombardAttack()
    {
        GetTargets();
        for (int i = targets.Count - 1; i >= 0; i--) targets[i].AE_DecreaseHP(curATK, new Color32(180, 0, 0, 255));

        //현재 링 위치에 파티클을 플레이한다.
        PlayParticle(baseRing.id);

        //게임에서 바로 제거한다.
        DeckManager.instance.RemoveRingFromBattle(this);
    }

    //현재 링 위치에서 파티클을 재생한다.
    void PlayParticle(int parID)
    {
        if (particle != null) particle.StopParticle(); //이미 있던 파티클은 멈춘다(이후 ParticleChecker에서 알아서 오브젝트 풀에 돌려준다).

        //파티클을 생성한다.
        particle = GameManager.instance.GetParticleFromPool(parID);

        //플레이한다.
        particle.PlayParticle(transform, 0.0f);
    }
}
