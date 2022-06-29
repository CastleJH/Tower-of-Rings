using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ring : MonoBehaviour
{
    int number;    //링 구분에 쓰임
    public Ringstone ringBase;  //링의 기본 정보

    //스탯
    public float curNumTarget;  //타겟 수
    public float curATK;   //현재 데미지
    public float curSPD;   //현재 공격 쿨타임
    public float curEFF;   //현재 효과 지속 시간
    public float buffNumTarget; //타겟 수 변화량(그대로 더하기 한다. 0.0f로 시작)
    public float buffATK;       //데미지 변화율(그대로 곱하기 한다. 1.0f로 시작)
    public float buffSPD;       //공격 쿨타임 변화율(그대로 곱하기 한다. 1.0f로 시작. 0.2f보다 작아도 0.2f를 곱함)
    public float buffEFF;       //효과 지속시간 변화율(그대로 더하기 한다. 0.0f로 시작)

    //타겟
    public List<Monster> targets; //공격범위 내의 몬스터들
    public List<Ring> nearRings; //공격범위 내의 링들(시너지 적용 대상)

    //사운드
    public AudioSource audioSource;     //공격 사운드

    //그래픽
    public SpriteRenderer spriteRenderer;   //링 자체 렌더러
    public SpriteRenderer rangeRenderer;    //공격 범위 표시 렌더러
    public Animator anim;

    //기타 변수들
    public bool isInBattle;         //전투 중인지 체크용
    public float shootCoolTime;    //발사 쿨타임 체크용
    public CircleCollider2D collider; //자신의 콜라이더
    int oxyRemoveCount;     //산화 링의 소멸 카운트
    float explosionSplash;        //폭발 링의 스플래쉬 데미지(비율)
    int poisonStack;            //맹독 링의 공격 당 쌓는 스택
    ParticleSystem rpGenerationParticle; //RP 생산시 링 위치에서 재생할 파티클
    Blizzard blizzard;
    public Monster commanderTarget; //사령관 링의 타겟
    public Ring commanderNearest;   //가장 근처의 사령관 링

    void Awake()
    {
        targets = new List<Monster>();
        collider = GetComponent<CircleCollider2D>(); 
    }

    void Update()
    {
        if (isInBattle)
        {
            shootCoolTime += Time.deltaTime;
            if (shootCoolTime > curSPD)
            {
                switch (ringBase.id)
                {
                    case 7:
                        GenerateRP(curATK);
                        anim.SetTrigger("isShoot");
                        shootCoolTime = 0.0f;
                        break;
                    case 11:
                        break;
                    default:
                        TryShoot();
                        break;
                }
            }
        }
    }

    //링 배치 단계에서 보여질 모습을 바꾸는 것이 주 목적이다. id에 해당하는 링으로 변한다.
    public void InitializeRing(int id)
    {
        //베이스 정보 얻기
        ringBase = GameManager.instance.ringstoneDB[id];

        //그래픽
        spriteRenderer.sprite = GameManager.instance.ringSprites[id];
        rangeRenderer.transform.localScale = new Vector2(ringBase.range * 2, ringBase.range * 2);
        rangeRenderer.color = new Color(0, 0, 0, 0);
        transform.localScale = Vector3.one;

        //콜라이더 끄기
        collider.enabled = false;
    }

    //실제로 게임에 넣는다.
    public void PutIntoBattle(int _number)
    {
        number = _number;
        
        commanderTarget = null;
        commanderNearest = null;

        switch (ringBase.id)
        {
            case 2:
                oxyRemoveCount = 20;
                break;
            case 4:
                explosionSplash = 0.5f;
                break;
            case 5:
                poisonStack = 1;
                break;
            case 11:
                blizzard = GameManager.instance.GetBlizzardFromPool();
                blizzard.InitializeBlizzard(this);
                blizzard.gameObject.SetActive(true);
                break;
            case 19:
                Ring dRing;
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--)
                {
                    dRing = DeckManager.instance.rings[i];
                    if (dRing.commanderNearest == null || Vector2.Distance(dRing.transform.position, transform.position) <= Vector2.Distance(dRing.transform.position, dRing.commanderNearest.transform.position)) //첫 사령관 링이거나 기존 사령관 링보다 더 가까운지 확인
                        dRing.commanderNearest = this;
                }
                break;
            default:
                break;
        }

        isInBattle = true;
        shootCoolTime = ringBase.baseSPD - 0.2f;
        rangeRenderer.color = new Color(0, 0, 0, 0);
        collider.enabled = true;

        ApplySynergy();
    }

    //불렛을 발사할 수 있다면 발사한다.
    public void TryShoot()
    {
        GetTargets();
        int numTarget = Mathf.Min(targets.Count, (int)curNumTarget);
        if (numTarget != 0)
        {
            if (ringBase.id != 19) audioSource.PlayOneShot(GameManager.instance.ringAttackAudios[ringBase.id]);
            Bullet bullet;
            switch (ringBase.id)
            {
                case 0: //리스트의 가장 앞쪽부터 타겟 만큼 쏨
                case 3:
                case 6:
                case 8:
                case 9:
                    targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                    for (int i = 0; i < numTarget; i++)
                    {
                        bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
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
                    bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                    bullet.InitializeBullet(this, targets[mIdx]);
                    bullet.gameObject.SetActive(true);
                    break;
                case 2: //산화 링의 소멸 카운팅
                    if (oxyRemoveCount-- == 0)
                    {
                        DeckManager.instance.RemoveRingFromBattle(this);
                        break;
                    }
                    targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                    for (int i = 0; i < numTarget; i++)
                    {
                        bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                    }
                    break;
                case 5: //랜덤한 갯수만큼 공격
                    if (Random.Range(0.0f, 1.0f) < 0.8f && commanderNearest != null && commanderNearest.commanderTarget != null) //사령관 대상인 경우
                    {
                        bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                        bullet.InitializeBullet(this, commanderNearest.commanderTarget);
                        bullet.gameObject.SetActive(true);
                        if (targets.Contains(commanderNearest.commanderTarget))
                        {
                            targets.Remove(commanderNearest.commanderTarget); //앞으로의 공격대상에서 해당 대상은 뺀다.
                            if (numTarget == targets.Count) numTarget--;
                        }
                    }
                    for (int i = 0; i < numTarget; i++)
                    {
                        int tar = Random.Range(0, targets.Count);
                        bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                        bullet.InitializeBullet(this, targets[tar]);
                        bullet.gameObject.SetActive(true);
                        targets.RemoveAt(tar);
                    }
                    break;
                case 10: //HP 낮은 순으로 타겟만큼 공격
                    targets = targets.OrderBy(x => x.curHP).ToList();
                    for (int i = 0; i < numTarget; i++)
                    {
                        bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
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
                case 7: //발사 안함
                case 11:
                    break;
                default:
                    Debug.Log(string.Format("Not implemented yet. {0} TryShoot", ringBase.id.ToString()));
                    break;
            }
            shootCoolTime = 0.0f;
            if (ringBase.id != 19) anim.SetTrigger("isShoot");
        }
    }

    //불렛이 몬스터에 닿았을 때의 효과를 정한다.
    public void AttackEffect(Monster monster)
    {
        switch (ringBase.id)
        {
            case 0: //단순 공격
            case 2:
            case 9:
            case 10:
                monster.AE_DecreaseHP(curATK, new Color32(100, 0, 0, 255));
                monster.PlayParticleCollision(ringBase.id, 0.0f);
                break;
            case 1:
                GetTargets();
                targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                int numTarget = Mathf.Min(targets.Count, (int)curNumTarget) - 1;
                for (int i = 0; i < targets.Count && numTarget != 0; i++)
                    if (targets[i] != monster)
                    {
                        targets[i].AE_DecreaseHP(curATK, Color.yellow);
                        targets[i].PlayParticleCollision(ringBase.id, 0.0f);
                        numTarget--;
                    }
                monster.AE_DecreaseHP(curATK, Color.yellow);
                monster.PlayParticleCollision(ringBase.id, 0.0f);
                break;
            case 3:
                monster.AE_Snow(curATK, curEFF);
                monster.PlayParticleCollision(ringBase.id, curEFF);
                break;
            case 4:
                monster.AE_Explosion(curATK, explosionSplash);
                monster.PlayParticleCollision(ringBase.id, 0.0f);
                break;
            case 5:
                monster.AE_Poison(number, curATK * poisonStack);
                break;
            case 6:
                monster.AE_DecreaseHP(Random.Range(0.0f, curATK), new Color32(255, 0, 100, 255));
                break;
            case 8:
                monster.AE_DecreaseHP(curATK, new Color32(100, 0, 0, 255));
                monster.PlayParticleCollision(ringBase.id, 0.0f);
                GenerateRP(curATK);
                break;
            case 18:    //아무것도 없음
            case 19:    
                break;
            default:
                Debug.Log(string.Format("Not implemented yet. {0} AttackEffect", ringBase.id.ToString()));
                break;
        }
    }

    //시너지를 적용한다.
    void ApplySynergy()
    {
        Ring ring;

        buffNumTarget = 0.0f;
        buffATK = 1.0f;
        buffSPD = 1.0f;
        buffEFF = 0.0f;
        
        ChangeCurNumTarget(0.0f);
        ChangeCurATK(0.0f);
        ChangeCurSPD(0.0f);
        ChangeCurEFF(0.0f, true);

        //다른 링에 대하여 나의 효과를 적용
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= ringBase.range + ring.collider.radius)
            {
                switch (ringBase.id)
                {
                    case 0: //공 10 공 5
                    case 6:
                    case 7:
                    case 11:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurATK(0.1f);
                        ring.ChangeCurATK(0.05f);
                        break;
                    case 1: //타 1 타 0.5
                    case 10:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurNumTarget(1.0f);
                        ring.ChangeCurNumTarget(0.5f);
                        break;
                    case 8: //속 -15 속 -8
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurSPD(-0.15f);
                        ring.ChangeCurSPD(-0.08f);
                        break;
                    case 3: //효 +0.5 효 +*0.05
                    case 18:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurEFF(0.5f, false);
                        ring.ChangeCurEFF(0.05f, true);
                        break;
                    case 4:
                        if (ring.ringBase.id == ringBase.id) ring.explosionSplash += 0.05f;
                        ring.ChangeCurATK(0.05f);
                        break;
                    case 5:
                        if (ring.ringBase.id == ringBase.id) ring.poisonStack++;
                        ring.ChangeCurSPD(-0.08f);
                        break;
                    case 9:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurATK(0.2f);
                        ring.ChangeCurATK(0.05f);
                        break;
                    case 2: //효과 없음
                    case 19:
                        break;
                    default: //구현 안됨
                        Debug.LogError("no synergy");
                        break;
                }
            }
        }
        

        //다른 링으로부터 나에게 효과를 적용
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= ring.ringBase.range + collider.radius)
                switch (ring.ringBase.id)
                {
                    case 0: //공 10 공 5
                    case 6:
                    case 7:
                    case 11:
                        if (ring.ringBase.id == ringBase.id) ChangeCurATK(0.1f);
                        ChangeCurATK(0.05f);
                        break;
                    case 1: //타 1 타 0.5
                    case 10:
                        if (ring.ringBase.id == ringBase.id) ChangeCurNumTarget(1.0f);
                        ChangeCurNumTarget(0.5f);
                        break;
                    case 8: //속 -15 속 -8
                        if (ring.ringBase.id == ringBase.id) ChangeCurSPD(-0.15f);
                        ChangeCurSPD(-0.08f);
                        break;
                    case 3: //효 +0.5 효 +*0.05
                    case 18:
                        if (ring.ringBase.id == ringBase.id) ChangeCurEFF(0.5f, false);
                        ChangeCurEFF(0.05f, true);
                        break;
                    case 4:
                        if (ring.ringBase.id == ringBase.id) explosionSplash += 0.05f;
                        ChangeCurATK(0.05f);
                        break;
                    case 5:
                        if (ring.ringBase.id == ringBase.id) poisonStack++;
                        ChangeCurSPD(-0.08f);
                        break;
                    case 9:
                        if (ring.ringBase.id == ringBase.id) ChangeCurATK(0.2f);
                        ChangeCurATK(0.05f);
                        break;
                    case 2: //효과 없음
                    case 19:
                        break;
                    default: //구현 안됨
                        Debug.LogError("no synergy");
                        break;
                }
        }
    }

    //시너지를 제거한다.
    void RemoveSynergy()
    {
        Ring ring;

        //내가 적용했던 시너지들만 제거
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= ringBase.range + ring.collider.radius)
            {
                switch (ringBase.id)
                {
                    case 0: //공 -10 공 -5
                    case 6:
                    case 7:
                    case 11:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurATK(-0.1f);
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 1: //타 -1 타 -0.5
                    case 10:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurNumTarget(-1.0f);
                        ring.ChangeCurNumTarget(-0.5f);
                        break;
                    case 8: //속 +15 속 +8
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurSPD(0.15f);
                        ring.ChangeCurSPD(0.08f);
                        break;
                    case 3: //효 -0.5 효 -*0.05
                    case 18:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurEFF(-0.5f, false);
                        ring.ChangeCurEFF(-0.05f, true);
                        break;
                    case 4:
                        if (ring.ringBase.id == ringBase.id) ring.explosionSplash -= 0.05f;
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 5:
                        if (ring.ringBase.id == ringBase.id) ring.poisonStack--;
                        ring.ChangeCurSPD(0.08f);
                        break;
                    case 9:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurATK(-0.2f);
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 2: //효과 없음
                    case 19:
                        break;
                    default: //구현 안됨
                        Debug.LogError("no synergy");
                        break;
                }
            }
        }
    }


    //전투에서 제거한다.
    public void RemoveFromBattle()
    {
        RemoveSynergy();
        DeckManager.instance.rings.Remove(this);
        switch (ringBase.id)
        {
            case 11:
                blizzard.RemoveBlizzard();
                blizzard = null;
                break;
            case 19:
                Ring dRing;
                List<Ring> commanderList = new List<Ring>();
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) //모든 링에 대하여 이 링이 가장 가까운 사령관 링이었던 경우 삭제하고, 같은 사령관 링이면 저장한다.
                {
                    dRing = DeckManager.instance.rings[i];
                    if (dRing.commanderNearest == this) dRing.commanderNearest = null;
                    if (dRing.ringBase.id == ringBase.id) commanderList.Add(dRing);
                }
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) //모든 링에 대하여 새롭게 사령관 링을 찾아준다.
                {
                    dRing = DeckManager.instance.rings[i];
                    for (int j = commanderList.Count - 1; j >= 0; j--) 
                        if (dRing.commanderNearest == null || Vector2.Distance(dRing.transform.position, commanderList[j].transform.position) <= Vector2.Distance(dRing.transform.position, dRing.commanderNearest.transform.position)) //기존 사령관 링보다 더 가까운지 확인
                            dRing.commanderNearest = commanderList[j];
                }
                break;
            default:
                break;
        }
    }

    //배치 가능 범위인지 확인한다.
    public bool CheckArrangePossible()
    {
        int ret = 3;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.75f);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Land" || colliders[i].tag == "Barrier") ret = 1;
            else
            {
                if (colliders[i].tag == "Ring") ret = 2;
                else ret = 3;
                break;
            }
        }
        if (ret == 1)
        {
            rangeRenderer.color = new Color32(0, 255, 0, 50);
            UIManager.instance.SetBattleArrangeFail(null);
            return true;
        }
        else
        {
            if (ret == 2) UIManager.instance.SetBattleArrangeFail("다른 링과 너무 가깝습니다.");
            else UIManager.instance.SetBattleArrangeFail("올바르지 않은 위치입니다.");
            rangeRenderer.color = new Color32(255, 0, 0, 50);
            return false;
        }
    }

    //공격 타겟들을 얻는다.
    public void GetTargets()
    {
        targets.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ringBase.range);

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
        curNumTarget = ringBase.baseNumTarget + buffNumTarget;
        if (curNumTarget < 0) curNumTarget = 0;
    }

    //공격력을 증감한다.
    public void ChangeCurATK(float buff)
    {
        buffATK += buff;
        if (buffATK > 0) curATK = ringBase.baseATK * buffATK;
        else curATK = 0;
    }

    //공격 쿨타임을 증감한다.
    public void ChangeCurSPD(float buff)
    {
        buffSPD += buff;
        if (buffSPD >= 0.2f) curSPD = ringBase.baseSPD * buffSPD;
        else curSPD = ringBase.baseSPD * 0.2f;
    }

    //효과 지속시간/확률을 증감한다. (isMul인경우 베이스*buff, 아니면 buff자체를 더해준다)
    public void ChangeCurEFF(float buff, bool isMul)
    {
        if (isMul) buffEFF += ringBase.baseEFF * buff;
        else buffEFF += buff;
        curEFF = ringBase.baseEFF + buffEFF;
        if (curEFF < 0) curEFF = 0;
    }

    void GenerateRP(float genRP)
    {
        if (rpGenerationParticle != null)   //이미 있던 파티클은 돌려준다.
        {
            rpGenerationParticle.Stop();
            GameManager.instance.ReturnParticleToPool(rpGenerationParticle, 7);
        }

        //RP를 생산하고 UI를 업데이트 한다.
        BattleManager.instance.ChangeCurrentRP(BattleManager.instance.rp + genRP);

        //파티클을 생성한다.
        rpGenerationParticle = GameManager.instance.GetParticleFromPool(7);

        //위치를 설정한다.
        rpGenerationParticle.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);

        //플레이한다.
        rpGenerationParticle.gameObject.SetActive(true);
        rpGenerationParticle.Play();
    }
}
