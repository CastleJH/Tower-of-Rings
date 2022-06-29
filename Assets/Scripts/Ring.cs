using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ring : MonoBehaviour
{
    int number;    //�� ���п� ����
    public Ringstone ringBase;  //���� �⺻ ����

    //����
    public float curNumTarget;  //Ÿ�� ��
    public float curATK;   //���� ������
    public float curSPD;   //���� ���� ��Ÿ��
    public float curEFF;   //���� ȿ�� ���� �ð�
    public float buffNumTarget; //Ÿ�� �� ��ȭ��(�״�� ���ϱ� �Ѵ�. 0.0f�� ����)
    public float buffATK;       //������ ��ȭ��(�״�� ���ϱ� �Ѵ�. 1.0f�� ����)
    public float buffSPD;       //���� ��Ÿ�� ��ȭ��(�״�� ���ϱ� �Ѵ�. 1.0f�� ����. 0.2f���� �۾Ƶ� 0.2f�� ����)
    public float buffEFF;       //ȿ�� ���ӽð� ��ȭ��(�״�� ���ϱ� �Ѵ�. 0.0f�� ����)

    //Ÿ��
    public List<Monster> targets; //���ݹ��� ���� ���͵�
    public List<Ring> nearRings; //���ݹ��� ���� ����(�ó��� ���� ���)

    //����
    public AudioSource audioSource;     //���� ����

    //�׷���
    public SpriteRenderer spriteRenderer;   //�� ��ü ������
    public SpriteRenderer rangeRenderer;    //���� ���� ǥ�� ������
    public Animator anim;

    //��Ÿ ������
    public bool isInBattle;         //���� ������ üũ��
    public float shootCoolTime;    //�߻� ��Ÿ�� üũ��
    public CircleCollider2D collider; //�ڽ��� �ݶ��̴�
    int oxyRemoveCount;     //��ȭ ���� �Ҹ� ī��Ʈ
    float explosionSplash;        //���� ���� ���÷��� ������(����)
    int poisonStack;            //�͵� ���� ���� �� �״� ����
    ParticleSystem rpGenerationParticle; //RP ����� �� ��ġ���� ����� ��ƼŬ
    Blizzard blizzard;
    public Monster commanderTarget; //��ɰ� ���� Ÿ��
    public Ring commanderNearest;   //���� ��ó�� ��ɰ� ��

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

    //�� ��ġ �ܰ迡�� ������ ����� �ٲٴ� ���� �� �����̴�. id�� �ش��ϴ� ������ ���Ѵ�.
    public void InitializeRing(int id)
    {
        //���̽� ���� ���
        ringBase = GameManager.instance.ringstoneDB[id];

        //�׷���
        spriteRenderer.sprite = GameManager.instance.ringSprites[id];
        rangeRenderer.transform.localScale = new Vector2(ringBase.range * 2, ringBase.range * 2);
        rangeRenderer.color = new Color(0, 0, 0, 0);
        transform.localScale = Vector3.one;

        //�ݶ��̴� ����
        collider.enabled = false;
    }

    //������ ���ӿ� �ִ´�.
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
                    if (dRing.commanderNearest == null || Vector2.Distance(dRing.transform.position, transform.position) <= Vector2.Distance(dRing.transform.position, dRing.commanderNearest.transform.position)) //ù ��ɰ� ���̰ų� ���� ��ɰ� ������ �� ������� Ȯ��
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

    //�ҷ��� �߻��� �� �ִٸ� �߻��Ѵ�.
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
                case 0: //����Ʈ�� ���� ���ʺ��� Ÿ�� ��ŭ ��
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
                case 1: //����Ʈ�� ���� ���� �� ���� ��
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
                case 2: //��ȭ ���� �Ҹ� ī����
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
                case 5: //������ ������ŭ ����
                    if (Random.Range(0.0f, 1.0f) < 0.8f && commanderNearest != null && commanderNearest.commanderTarget != null) //��ɰ� ����� ���
                    {
                        bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                        bullet.InitializeBullet(this, commanderNearest.commanderTarget);
                        bullet.gameObject.SetActive(true);
                        if (targets.Contains(commanderNearest.commanderTarget))
                        {
                            targets.Remove(commanderNearest.commanderTarget); //�������� ���ݴ�󿡼� �ش� ����� ����.
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
                case 10: //HP ���� ������ Ÿ�ٸ�ŭ ����
                    targets = targets.OrderBy(x => x.curHP).ToList();
                    for (int i = 0; i < numTarget; i++)
                    {
                        bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                    }
                    break;
                case 18: //������ ������ŭ ��� ����
                    Barrier barrier;
                    if (Random.Range(0.0f, 1.0f) < 0.8f && commanderNearest != null && commanderNearest.commanderTarget != null) //��ɰ� ����� ���
                    {
                        barrier = GameManager.instance.GetBarrierFromPool();
                        barrier.InitializeBarrier(curEFF, commanderNearest.commanderTarget.transform.position);
                        barrier.gameObject.SetActive(true);
                        if (targets.Contains(commanderNearest.commanderTarget))
                        {
                            targets.Remove(commanderNearest.commanderTarget); //�������� ���ݴ�󿡼� �ش� ����� ����.
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
                case 19: //��ɰ��� ��� ����
                    commanderTarget = null;
                    for (int i = 0; i < targets.Count; i++)
                        if (commanderTarget == null || commanderTarget.movedDistance < targets[i].movedDistance)
                            commanderTarget = targets[i];
                    break;
                case 7: //�߻� ����
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

    //�ҷ��� ���Ϳ� ����� ���� ȿ���� ���Ѵ�.
    public void AttackEffect(Monster monster)
    {
        switch (ringBase.id)
        {
            case 0: //�ܼ� ����
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
            case 18:    //�ƹ��͵� ����
            case 19:    
                break;
            default:
                Debug.Log(string.Format("Not implemented yet. {0} AttackEffect", ringBase.id.ToString()));
                break;
        }
    }

    //�ó����� �����Ѵ�.
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

        //�ٸ� ���� ���Ͽ� ���� ȿ���� ����
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= ringBase.range + ring.collider.radius)
            {
                switch (ringBase.id)
                {
                    case 0: //�� 10 �� 5
                    case 6:
                    case 7:
                    case 11:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurATK(0.1f);
                        ring.ChangeCurATK(0.05f);
                        break;
                    case 1: //Ÿ 1 Ÿ 0.5
                    case 10:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurNumTarget(1.0f);
                        ring.ChangeCurNumTarget(0.5f);
                        break;
                    case 8: //�� -15 �� -8
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurSPD(-0.15f);
                        ring.ChangeCurSPD(-0.08f);
                        break;
                    case 3: //ȿ +0.5 ȿ +*0.05
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
                    case 2: //ȿ�� ����
                    case 19:
                        break;
                    default: //���� �ȵ�
                        Debug.LogError("no synergy");
                        break;
                }
            }
        }
        

        //�ٸ� �����κ��� ������ ȿ���� ����
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= ring.ringBase.range + collider.radius)
                switch (ring.ringBase.id)
                {
                    case 0: //�� 10 �� 5
                    case 6:
                    case 7:
                    case 11:
                        if (ring.ringBase.id == ringBase.id) ChangeCurATK(0.1f);
                        ChangeCurATK(0.05f);
                        break;
                    case 1: //Ÿ 1 Ÿ 0.5
                    case 10:
                        if (ring.ringBase.id == ringBase.id) ChangeCurNumTarget(1.0f);
                        ChangeCurNumTarget(0.5f);
                        break;
                    case 8: //�� -15 �� -8
                        if (ring.ringBase.id == ringBase.id) ChangeCurSPD(-0.15f);
                        ChangeCurSPD(-0.08f);
                        break;
                    case 3: //ȿ +0.5 ȿ +*0.05
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
                    case 2: //ȿ�� ����
                    case 19:
                        break;
                    default: //���� �ȵ�
                        Debug.LogError("no synergy");
                        break;
                }
        }
    }

    //�ó����� �����Ѵ�.
    void RemoveSynergy()
    {
        Ring ring;

        //���� �����ߴ� �ó����鸸 ����
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= ringBase.range + ring.collider.radius)
            {
                switch (ringBase.id)
                {
                    case 0: //�� -10 �� -5
                    case 6:
                    case 7:
                    case 11:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurATK(-0.1f);
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 1: //Ÿ -1 Ÿ -0.5
                    case 10:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurNumTarget(-1.0f);
                        ring.ChangeCurNumTarget(-0.5f);
                        break;
                    case 8: //�� +15 �� +8
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurSPD(0.15f);
                        ring.ChangeCurSPD(0.08f);
                        break;
                    case 3: //ȿ -0.5 ȿ -*0.05
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
                    case 2: //ȿ�� ����
                    case 19:
                        break;
                    default: //���� �ȵ�
                        Debug.LogError("no synergy");
                        break;
                }
            }
        }
    }


    //�������� �����Ѵ�.
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
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) //��� ���� ���Ͽ� �� ���� ���� ����� ��ɰ� ���̾��� ��� �����ϰ�, ���� ��ɰ� ���̸� �����Ѵ�.
                {
                    dRing = DeckManager.instance.rings[i];
                    if (dRing.commanderNearest == this) dRing.commanderNearest = null;
                    if (dRing.ringBase.id == ringBase.id) commanderList.Add(dRing);
                }
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) //��� ���� ���Ͽ� ���Ӱ� ��ɰ� ���� ã���ش�.
                {
                    dRing = DeckManager.instance.rings[i];
                    for (int j = commanderList.Count - 1; j >= 0; j--) 
                        if (dRing.commanderNearest == null || Vector2.Distance(dRing.transform.position, commanderList[j].transform.position) <= Vector2.Distance(dRing.transform.position, dRing.commanderNearest.transform.position)) //���� ��ɰ� ������ �� ������� Ȯ��
                            dRing.commanderNearest = commanderList[j];
                }
                break;
            default:
                break;
        }
    }

    //��ġ ���� �������� Ȯ���Ѵ�.
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
            if (ret == 2) UIManager.instance.SetBattleArrangeFail("�ٸ� ���� �ʹ� �������ϴ�.");
            else UIManager.instance.SetBattleArrangeFail("�ùٸ��� ���� ��ġ�Դϴ�.");
            rangeRenderer.color = new Color32(255, 0, 0, 50);
            return false;
        }
    }

    //���� Ÿ�ٵ��� ��´�.
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

    //Ÿ�� ������ �����Ѵ�.
    public void ChangeCurNumTarget(float buff)
    {
        buffNumTarget += buff;
        curNumTarget = ringBase.baseNumTarget + buffNumTarget;
        if (curNumTarget < 0) curNumTarget = 0;
    }

    //���ݷ��� �����Ѵ�.
    public void ChangeCurATK(float buff)
    {
        buffATK += buff;
        if (buffATK > 0) curATK = ringBase.baseATK * buffATK;
        else curATK = 0;
    }

    //���� ��Ÿ���� �����Ѵ�.
    public void ChangeCurSPD(float buff)
    {
        buffSPD += buff;
        if (buffSPD >= 0.2f) curSPD = ringBase.baseSPD * buffSPD;
        else curSPD = ringBase.baseSPD * 0.2f;
    }

    //ȿ�� ���ӽð�/Ȯ���� �����Ѵ�. (isMul�ΰ�� ���̽�*buff, �ƴϸ� buff��ü�� �����ش�)
    public void ChangeCurEFF(float buff, bool isMul)
    {
        if (isMul) buffEFF += ringBase.baseEFF * buff;
        else buffEFF += buff;
        curEFF = ringBase.baseEFF + buffEFF;
        if (curEFF < 0) curEFF = 0;
    }

    void GenerateRP(float genRP)
    {
        if (rpGenerationParticle != null)   //�̹� �ִ� ��ƼŬ�� �����ش�.
        {
            rpGenerationParticle.Stop();
            GameManager.instance.ReturnParticleToPool(rpGenerationParticle, 7);
        }

        //RP�� �����ϰ� UI�� ������Ʈ �Ѵ�.
        BattleManager.instance.ChangeCurrentRP(BattleManager.instance.rp + genRP);

        //��ƼŬ�� �����Ѵ�.
        rpGenerationParticle = GameManager.instance.GetParticleFromPool(7);

        //��ġ�� �����Ѵ�.
        rpGenerationParticle.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);

        //�÷����Ѵ�.
        rpGenerationParticle.gameObject.SetActive(true);
        rpGenerationParticle.Play();
    }
}
