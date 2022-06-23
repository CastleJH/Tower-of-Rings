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
    int poisonStack;

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
                TryShoot();
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
    public void PutRingIntoScene(int _number)
    {
        number = _number;
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
            audioSource.PlayOneShot(GameManager.instance.ringAttackAudios[ringBase.id]);
            Bullet bullet;
            switch (ringBase.id)
            {
                case 0: //리스트의 가장 앞쪽부터 타겟 만큼 쏨
                case 3:
                case 6:
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
                        DeckManager.instance.RemoveRing(this);
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
                    for (int i = 0; i < numTarget; i++)
                    {
                        int tar = Random.Range(0, targets.Count);
                        bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                        bullet.InitializeBullet(this, targets[tar]);
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
                default:
                    Debug.Log(string.Format("Not implemented yet. {0} TryShoot", ringBase.id.ToString()));
                    break;
            }
            shootCoolTime = 0.0f;
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
        ChangeCurEFF(0.0f);

        //다른 링에 대하여 나의 효과를 적용
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= ringBase.range + ring.collider.radius)
            {
                switch (ringBase.id)
                {
                    case 0:
                    case 6:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurATK(0.1f);
                        ring.ChangeCurATK(0.05f);
                        break;
                    case 1:
                    case 10:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurNumTarget(1.0f);
                        ring.ChangeCurNumTarget(0.5f);
                        break;
                    case 3:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurEFF(0.5f);
                        ring.ChangeCurEFF(0.1f);
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
                    case 0:
                    case 6:
                        if (ring.ringBase.id == ringBase.id) ChangeCurATK(0.1f);
                        ChangeCurATK(0.05f);
                        break;
                    case 1:
                    case 10:
                        if (ring.ringBase.id == ringBase.id) ChangeCurNumTarget(1.0f);
                        ChangeCurNumTarget(0.5f);
                        break;
                    case 3:
                        if (ring.ringBase.id == ringBase.id) ChangeCurEFF(0.5f);
                        ChangeCurEFF(0.1f);
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
                        break;
                    default: //구현 안됨
                        Debug.LogError("no synergy");
                        break;
                }
        }
    }

    //시너지를 제거한다.
    public void RemoveSynergy()
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
                    case 0:
                    case 6:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurATK(-0.1f);
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 1:
                    case 10:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurNumTarget(-1.0f);
                        ring.ChangeCurNumTarget(-0.5f);
                        break;
                    case 3:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurEFF(-0.5f);
                        ring.ChangeCurEFF(-0.1f);
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
                        break;
                    default: //구현 안됨
                        Debug.LogError("no synergy");
                        break;
                }
            }
        }
    }

    //배치 가능 범위인지 확인한다.
    public bool CheckArragePossible()
    {
        int ret = 3;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.75f);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Land") ret = 1;
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

    //효과 지속시간을 증감한다.
    public void ChangeCurEFF(float buff)
    {
        buffEFF += buff;
        curEFF = ringBase.baseEFF + buffEFF;
        if (curEFF < 0) curEFF = 0;
    }
}
