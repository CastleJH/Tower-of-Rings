using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ring : MonoBehaviour
{
    int number;    //�� ���п� ����
    public BaseRing baseRing;  //���� �⺻ ����

    //����
    public float curNumTarget;  //Ÿ�� ��
    public float curATK;   //���� ������
    public float curSPD;   //���� ���� ��Ÿ��
    public float curEFF;   //���� ȿ�� ���� �ð�
    public float buffNumTarget; //Ÿ�� �� ��ȭ��(�״�� ���ϱ� �Ѵ�. 0.0f�� ����)
    public float buffATK;       //������ ��ȭ��(�״�� ���ϱ� �Ѵ�. 1.0f�� ����)
    public float buffSPD;       //���� ��Ÿ�� ��ȭ��(�״�� ���ϱ� �Ѵ�. 1.0f�� ����. 0.2f���� �۾Ƶ� 0.2f�� ����)
    public float buffEFF;       //ȿ�� ���ӽð� ��ȭ��(�״�� ���ϱ� �Ѵ�. 1.0f�� ����)
    public float buffEFFPlus;       //ȿ�� ���ӽð� ��ȭ��(�״�� ���ϱ� �Ѵ�. 0.0f�� ����)

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
    public CircleCollider2D ringCollider; //�ڽ��� �ݶ��̴�
    public bool isSealed;           //���� ���ͷ� ���� ���� ����
    int oxyRemoveCount;          //��ȭ ���� �Ҹ� ī��Ʈ
    float explosionSplash;        //���� ���� ���÷��� ������(����)
    int poisonStack;            //�͵� ���� ���� �� �״� ����
    ParticleChecker rpGenerationParticle; //RP ����� �� ��ġ���� ����� ��ƼŬ
    Blizzard blizzard;          //������
    public Monster commanderTarget; //��ɰ� ���� Ÿ��
    public Ring commanderNearest;   //���� ��ó�� ��ɰ� ��
    int curseStack;            //���� ���� ���� �� �״� ����
    Amplifier amplifier;        //����
    float executionRate;      //ó�� ���� HP ����
    public int growStack;        //���帵�� ���ݷ� ���� ����
    float chaseAttackRadius;         //�߰��� ���� ����

    void Awake()
    {
        targets = new List<Monster>();
        ringCollider = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        //if (Time.timeScale == 0) return;
        if (isInBattle)
        {
            if (!isSealed)
            {
                shootCoolTime += Time.deltaTime;
                if (shootCoolTime > curSPD)     //���� ��Ÿ���� �Ǿ��ٸ�
                {
                    switch (baseRing.id)
                    {
                        case 7:
                            GenerateRP(curATK);
                            anim.SetTrigger("isShoot");
                            shootCoolTime = 0.0f;
                            break;
                        case 10:
                            GameManager.instance.ChangePlayerCurHP(1);
                            anim.SetTrigger("isShoot");
                            shootCoolTime = 0.0f;
                            break;
                        case 11:
                        case 17:
                        case 23:
                            break;
                        case 22:
                            BombardAttack();
                            break;
                        case 32:
                            if (DeckManager.instance.sleepActivated == 3) TryShoot();
                            break;
                        default:
                            TryShoot();
                            break;
                    }
                }
            }
        }
        else
        {
            if (DeckManager.instance.genRing != this)
                GameManager.instance.ReturnRingToPool(this);
        }
    }

    //�� ��ġ �ܰ踦 ����, id�� �ش��ϴ� ������ ���Ѵ�.
    public void InitializeRing(int id)
    {
        //���̽� ���� ���
        baseRing = GameManager.instance.baseRings[id];

        //�׷���
        transform.localScale = Vector3.one;
        spriteRenderer.sprite = GameManager.instance.ringSprites[id];
        rangeRenderer.transform.localScale = new Vector2(baseRing.range * 2, baseRing.range * 2);
        rangeRenderer.color = new Color(0, 0, 0, 0);
        transform.localScale = Vector3.one;

        //�ݶ��̴� ����
        ringCollider.enabled = false;
    }

    //������ ���ӿ� �ִ´�.
    public void PutIntoBattle(int _number)
    {
        number = _number;

        commanderTarget = null;
        commanderNearest = null;

        switch (baseRing.id)
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

        isInBattle = true;
        shootCoolTime = baseRing.baseSPD - 0.2f;
        rangeRenderer.color = new Color(0, 0, 0, 0);
        ringCollider.enabled = true;
        isSealed = false;

        ApplySynergy();
    }

    //�ҷ��� �߻��� �� �ִٸ� �߻��Ѵ�.
    public void TryShoot()
    {
        GetTargets();
        int numTarget = Mathf.Min(targets.Count, (int)curNumTarget);    //���� �� ���� �� or ���� �ִ� Ÿ�� �� �� �� ���� ��
        if (numTarget != 0)
        {
            if (baseRing.id != 19) audioSource.PlayOneShot(GameManager.instance.ringAttackAudios[baseRing.id]);
            Bullet bullet;
            switch (baseRing.id)
            {
                case 0: //����Ʈ�� ���� ���ʺ��� Ÿ�� ��ŭ ��
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
                    if (mIdx == -1) return;
                    bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                    bullet.InitializeBullet(this, targets[mIdx]);
                    bullet.gameObject.SetActive(true);
                    break;
                case 2: //��ȭ ���� �Ҹ� ī����
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
                case 5: //������ ������ŭ ����
                case 14:
                case 20:
                case 24:
                case 26:
                case 29:
                    if (commanderNearest != null && commanderNearest.commanderTarget != null && Random.Range(0.0f, 1.0f) < commanderNearest.curEFF) //��ɰ� ����� ���
                    {
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, commanderNearest.commanderTarget);
                        bullet.gameObject.SetActive(true);
                        numTarget--;
                        if (targets.Contains(commanderNearest.commanderTarget))
                            targets.Remove(commanderNearest.commanderTarget); //�������� ���ݴ�󿡼� �ش� ����� ����.

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
                case 12: //HP ���� ������ Ÿ�ٸ�ŭ ����
                case 21:
                    targets = targets.OrderByDescending(x => x.curHP).ToList();
                    for (int i = 0; i < numTarget; i++)
                    {
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                    }
                    break;
                case 15: //����Ʈ/���� ���� ����
                    targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                    for (int i = 0; i < targets.Count && numTarget != 0; i++)
                    {
                        if (targets[i].IsNormalMonster()) continue;
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                        numTarget--;
                    }
                    for (int i = 0; i < targets.Count && numTarget != 0; i++)
                    {
                        if (!targets[i].IsNormalMonster()) continue;
                        bullet = GameManager.instance.GetBulletFromPool(baseRing.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                        numTarget--;
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
                case 28: //�ֺ��� ���� ���Ͱ� ���� �ִ� ���� ����
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
                case 7: //�߻� ����
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

    //�ҷ��� ���Ϳ� ����� ���� ȿ���� ���Ѵ�.
    public void AttackEffect(Monster monster)
    {
        switch (baseRing.id)
        {
            case 0: //�ܼ� ����
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
                monster.AE_Poison(number, curATK * poisonStack);
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
            case 18:    //�ƹ��͵� ����
            case 19:
            case 22:
                break;
            default:
                Debug.Log(string.Format("Not implemented yet. {0} AttackEffect", baseRing.id.ToString()));
                break;
        }
    }

    public void GiveSynergy(Ring ring)
    {
        switch (baseRing.id)
        {
            case 0: //�� 10 �� 5
            case 6:
            case 7:
            case 11:
            case 15:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurATK(0.1f);
                ring.ChangeCurATK(0.05f);
                break;
            case 1: //Ÿ 1 Ÿ 0.5
            case 20:
            case 26:
            case 30:
            case 31:
            case 32:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurNumTarget(1.0f);
                ring.ChangeCurNumTarget(0.5f);
                break;
            case 8: //�� -15 �� -8
            case 14:
            case 25:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurSPD(-0.15f);
                ring.ChangeCurSPD(-0.08f);
                break;
            case 3: //ȿ +0.5 ȿ +*0.05
            case 18:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(0.5f, '+');
                ring.ChangeCurEFF(0.05f, '*');
                break;
            case 4:
                if (ring.baseRing.id == baseRing.id) ring.explosionSplash += 0.05f;
                ring.ChangeCurATK(0.05f);
                break;
            case 5:
                if (ring.baseRing.id == baseRing.id) ring.poisonStack++;
                ring.ChangeCurSPD(-0.08f);
                break;
            case 9:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurATK(0.2f);
                ring.ChangeCurATK(0.05f);
                break;
            case 12:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurATK(0.15f);
                ring.ChangeCurATK(0.05f);
                break;
            case 13:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(0.1f, '+');
                ring.ChangeCurEFF(0.05f, '*');
                break;
            case 16:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(0.01f, '+');
                ring.ChangeCurEFF(0.05f, '*');
                break;
            case 21:
                if (ring.baseRing.id == baseRing.id) ring.curseStack++;
                ring.ChangeCurSPD(-0.08f);
                break;
            case 24:
                if (ring.baseRing.id == baseRing.id) ring.executionRate += 0.02f;
                ring.ChangeCurSPD(-0.08f);
                break;
            case 28:
                if (ring.baseRing.id == baseRing.id) ring.chaseAttackRadius += 0.15f;
                ring.ChangeCurATK(0.05f);
                break;
            case 29:
                if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(0.02f, '+');
                ring.ChangeCurEFF(0.05f, '*');
                break;
            case 2: //ȿ�� ����
            case 10:
            case 17:
            case 19:
            case 22:
            case 23:
            case 27:
                break;
            default: //���� �ȵ�
                Debug.LogError("no synergy");
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
        buffEFF = 1.0f;
        buffEFFPlus = 0.0f;


        ChangeCurNumTarget(0.0f);
        ChangeCurATK(0.0f);
        ChangeCurSPD(0.0f);
        ChangeCurEFF(0.0f, '*');

        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= baseRing.range + ring.ringCollider.radius) //�ٸ� ���� ���Ͽ� ���� ȿ���� ����
                GiveSynergy(ring);
            if (Vector2.Distance(ring.transform.position, transform.position) <= ring.baseRing.range + ringCollider.radius) //�ٸ� �����κ��� ������ ȿ���� ����
                ring.GiveSynergy(this);
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
            if (Vector2.Distance(ring.transform.position, transform.position) <= baseRing.range + ring.ringCollider.radius)
            {
                switch (baseRing.id)
                {
                    case 0: //�� -10 �� -5
                    case 6:
                    case 7:
                    case 11:
                    case 15:
                        if (ring.baseRing.id == baseRing.id) ring.ChangeCurATK(-0.1f);
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 1: //Ÿ -1 Ÿ -0.5
                    case 20:
                    case 26:
                    case 30:
                    case 31:
                    case 32:
                        if (ring.baseRing.id == baseRing.id) ring.ChangeCurNumTarget(-1.0f);
                        ring.ChangeCurNumTarget(-0.5f);
                        break;
                    case 8: //�� +15 �� +8
                    case 14:
                    case 25:
                        if (ring.baseRing.id == baseRing.id) ring.ChangeCurSPD(0.15f);
                        ring.ChangeCurSPD(0.08f);
                        break;
                    case 3: //ȿ -0.5 ȿ -*0.05
                    case 18:
                        if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(-0.5f, '+');
                        ring.ChangeCurEFF(-0.05f, '*');
                        break;
                    case 4:
                        if (ring.baseRing.id == baseRing.id) ring.explosionSplash -= 0.05f;
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 5:
                        if (ring.baseRing.id == baseRing.id) ring.poisonStack--;
                        ring.ChangeCurSPD(0.08f);
                        break;
                    case 9:
                        if (ring.baseRing.id == baseRing.id) ring.ChangeCurATK(-0.2f);
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 12:
                        if (ring.baseRing.id == baseRing.id) ring.ChangeCurATK(-0.15f);
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 13:
                        if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(-0.1f, '+');
                        ring.ChangeCurEFF(-0.05f, '*');
                        break;
                    case 16:
                        if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(-0.01f, '+');
                        ring.ChangeCurEFF(-0.05f, '*');
                        break;
                    case 21:
                        if (ring.baseRing.id == baseRing.id) ring.curseStack--;
                        ring.ChangeCurSPD(0.08f);
                        break;
                    case 24:
                        if (ring.baseRing.id == baseRing.id) ring.executionRate -= 0.02f;
                        ring.ChangeCurSPD(0.08f);
                        break;
                    case 28:
                        if (ring.baseRing.id == baseRing.id) ring.chaseAttackRadius -= 0.15f;
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 29:
                        if (ring.baseRing.id == baseRing.id) ring.ChangeCurEFF(-0.02f, '+');
                        ring.ChangeCurEFF(-0.05f, '*');
                        break;
                    case 2: //ȿ�� ����
                    case 10:
                    case 17:
                    case 19:
                    case 22:
                    case 23:
                    case 27:
                        break;
                    default: //���� �ȵ�
                        Debug.LogError("no synergy");
                        break;
                }
            }
        }
    }


    //�������� ���Ž� �Ͼ�� ��ȭ�� �����Ѵ�.
    public void ApplyRemoveEffect()
    {
        RemoveSynergy();
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

    //��ġ ���� �������� Ȯ���Ѵ�.
    public bool CanBePlaced()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.75f);

        int deckIdx = DeckManager.instance.deck.IndexOf(baseRing.id);
        int rpCost;
        if (!int.TryParse(UIManager.instance.battleDeckRingRPText[deckIdx].text, out rpCost)) rpCost = 0;
        if (rpCost > BattleManager.instance.rp)
        {
            UIManager.instance.SetBattleArrangeFail("RP�� �����մϴ�.");
            rangeRenderer.color = new Color32(255, 0, 0, 50);
            return false;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Monster Path")
            {
                UIManager.instance.SetBattleArrangeFail("�ùٸ��� ���� ��ġ�Դϴ�.");
                rangeRenderer.color = new Color32(255, 0, 0, 50);
                return false;
            }
            else if (colliders[i].tag == "Ring")
            {
                UIManager.instance.SetBattleArrangeFail("�ٸ� ���� �ʹ� �������ϴ�.");
                rangeRenderer.color = new Color32(255, 0, 0, 50);
                return false;
            }
        }

        rangeRenderer.color = new Color32(0, 255, 0, 50);
        UIManager.instance.SetBattleArrangeFail(null);
        return true;
    }

    //���� Ÿ�ٵ��� ��´�.
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

    //Ÿ�� ������ �����Ѵ�.
    public void ChangeCurNumTarget(float buff)
    {
        buffNumTarget += buff;
        curNumTarget = baseRing.baseNumTarget + buffNumTarget;
        if (curNumTarget < 0) curNumTarget = 0;
    }

    //���ݷ��� �����Ѵ�.
    public void ChangeCurATK(float buff)
    {
        buffATK += buff;
        if (buffATK > 0) curATK = baseRing.baseATK * buffATK;
        else curATK = 0;
    }

    //���� ��Ÿ���� �����Ѵ�.
    public void ChangeCurSPD(float buff)
    {
        buffSPD += buff;
        if (buffSPD >= 0.2f) curSPD = baseRing.baseSPD * buffSPD;
        else curSPD = baseRing.baseSPD * 0.2f;
    }

    //ȿ�� ���ӽð�/Ȯ���� �����Ѵ�. (isMul�ΰ�� ���̽�*buff, �ƴϸ� buff��ü�� �����ش�)
    public void ChangeCurEFF(float buff, char mulOrPlus)
    {
        if (mulOrPlus == '*') buffEFF += buff;
        else buffEFFPlus += buff;
        curEFF = baseRing.baseEFF * buffEFF + buffEFFPlus;
        if (curEFF < 0) curEFF = 0;
    }

    //RP�� �����Ѵ�.
    void GenerateRP(float genRP)
    {
        if (rpGenerationParticle != null)   //�̹� �ִ� ��ƼŬ�� �����ش�.
        {
            rpGenerationParticle.StopParticle();
        }

        //RP�� �����ϰ� UI�� ������Ʈ �Ѵ�.
        BattleManager.instance.ChangePlayerRP(genRP);

        //��ƼŬ�� �����Ѵ�.
        rpGenerationParticle = GameManager.instance.GetParticleFromPool(7);

        //�÷����Ѵ�.
        rpGenerationParticle.PlayParticle(transform, 0.0f);
    }

    //���� �� ��� ���� �����Ѵ�. 
    void BombardAttack()
    {
        GetTargets();
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            targets[i].PlayParticleCollision(baseRing.id, 0.0f);
            targets[i].AE_DecreaseHP(curATK, new Color32(180, 0, 0, 255));
        }
        DeckManager.instance.RemoveRingFromBattle(this);
    }
}
