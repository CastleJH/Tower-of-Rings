using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ring : MonoBehaviour
{
    public Ringstone ringBase;  //링의 기본 정보

    //스탯
    public float curNumTarget;  //타겟 수
    public float curDMG;   //현재 데미지
    public float curSPD;   //현재 공격 쿨타임
    public float curEFF;   //현재 효과 지속 시간
    public float buffNumTarget; //타겟 수 변화량(그대로 더하기 한다. 0.0f로 시작)
    public float buffDMG;       //데미지 변화율(그대로 곱하기 한다. 1.0f로 시작)
    public float buffSPD;       //공격 쿨타임 변화율(그대로 곱하기 한다. 1.0f로 시작. 0.2f보다 작아도 0.2f를 곱함)
    public float buffEFF;       //효과 지속시간 변화율(그대로 곱하기한다. 1.0f로 시작)

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
    public void PutRingIntoScene()
    {
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
                    targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                    for (int i = 0; i < numTarget; i++)
                    {
                        bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                    }
                    break;
                case 1: //리스트의 가장 앞쪽 한 개만 쏨
                    float maxDist = 0.0f;
                    int idx = -1;
                    for (int i = 0; i < targets.Count; i++)
                        if (maxDist < targets[i].movedDistance)
                        {
                            maxDist = targets[i].movedDistance;
                            idx = i;
                        }
                    bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                    bullet.InitializeBullet(this, targets[idx]);
                    bullet.gameObject.SetActive(true);
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
            case 0:
                monster.AE_DecreaseHP(curDMG, Color.red);
                monster.PlayParticleCollision(ringBase.id);
                break;
            case 1:
                GetTargets();
                targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                int numTarget = Mathf.Min(targets.Count, (int)curNumTarget) - 1;
                Debug.Log("num: " + numTarget.ToString());
                for (int i = 0; i < targets.Count && numTarget != 0; i++)
                    if (targets[i] != monster)
                    {
                        Debug.Log("1");
                        targets[i].AE_DecreaseHP(curDMG, Color.blue);
                        targets[i].PlayParticleCollision(ringBase.id);
                        numTarget--;
                    }
                    else
                        Debug.Log("0");
                monster.AE_DecreaseHP(curDMG, Color.blue);
                monster.PlayParticleCollision(ringBase.id);
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
        buffDMG = 1.0f;
        buffSPD = 1.0f;
        buffEFF = 1.0f;
        
        ChangeCurNumTarget(0.0f);
        ChangeCurDMG(0.0f);
        ChangeCurSPD(0.0f);
        ChangeCurEFF(0.0f);

        GetNearRings();

        //다른 링에 대하여
        switch (ringBase.id)
        {
            case 0:
                for (int i = 0; i < nearRings.Count; i++)
                {
                    ring = nearRings[i];
                    ring.ChangeCurDMG(0.05f);
                    if (ring.ringBase.id == ringBase.id) ring.ChangeCurDMG(0.1f);
                }
                break;
            case 1:
                for (int i = 0; i < nearRings.Count; i++)
                {
                    ring = nearRings[i];
                    ring.ChangeCurNumTarget(0.5f);
                    if (ring.ringBase.id == ringBase.id) ring.ChangeCurNumTarget(1.0f);
                }
                break;
        }

        //다른 링으로부터
        for (int i = 0; i < nearRings.Count; i++)
            switch (nearRings[i].ringBase.id)
            {
                case 0:
                    ChangeCurDMG(0.05f);
                    if (nearRings[i].ringBase.id == ringBase.id) ChangeCurDMG(0.1f);
                    break;
                case 1:
                    ChangeCurNumTarget(0.5f);
                    if (nearRings[i].ringBase.id == ringBase.id) ChangeCurNumTarget(1.0f);
                    break;
            }
    }

    //시너지를 제거한다.
    public void RemoveSynergy()
    {
        Ring ring;

        GetNearRings();

        switch (ringBase.id)
        {
            case 0:
                for (int i = 0; i < nearRings.Count; i++)
                {
                    ring = nearRings[i];
                    ring.ChangeCurDMG(-0.05f);
                    if (ring.ringBase.id == ringBase.id) ring.ChangeCurDMG(-0.1f);
                }
                break;
            case 1:
                for (int i = 0; i < nearRings.Count; i++)
                {
                    ring = nearRings[i];
                    ring.ChangeCurNumTarget(-0.5f);
                    if (ring.ringBase.id == ringBase.id) ring.ChangeCurNumTarget(-1.0f);
                }
                break;
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

    public void GetNearRings()
    {
        nearRings.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ringBase.range);

        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i].tag == "Ring" && colliders[i].gameObject != gameObject)
                nearRings.Add(colliders[i].GetComponent<Ring>());
    }
    public void ChangeCurNumTarget(float buff)
    {
        buffNumTarget += buff;
        curNumTarget = ringBase.baseNumTarget + buffNumTarget;
    }

    public void ChangeCurDMG(float buff)
    {
        buffDMG += buff;
        curDMG = ringBase.baseDMG * buffDMG;
    }

    public void ChangeCurSPD(float buff)
    {
        buffSPD += buff;
        if (buffSPD >= 0.2f) curSPD = ringBase.baseSPD * buffSPD;
        else curSPD = ringBase.baseSPD * 0.2f;
    }

    public void ChangeCurEFF(float buff)
    {
        buffEFF += buff;
        curEFF = ringBase.baseEFF * buffEFF;
    }
}
