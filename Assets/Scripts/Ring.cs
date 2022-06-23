using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ring : MonoBehaviour
{
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
    int id2RemoveCount;     //��ȭ ���� �Ҹ� ī��Ʈ
    float id4Splash;        //���� ���� ���÷��� ������(����)

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
    public void PutRingIntoScene()
    {
        switch (ringBase.id)
        {
            case 2:
                id2RemoveCount = 20;
                break;
            case 4:
                id4Splash = 0.5f;
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
            audioSource.PlayOneShot(GameManager.instance.ringAttackAudios[ringBase.id]);
            Bullet bullet;
            switch (ringBase.id)
            {
                case 0: //����Ʈ�� ���� ���ʺ��� Ÿ�� ��ŭ ��
                case 3:
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
                case 2: //��ȭ ���� �Ҹ� ī����
                    if (id2RemoveCount-- == 0)
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
                default:
                    Debug.Log(string.Format("Not implemented yet. {0} TryShoot", ringBase.id.ToString()));
                    break;
            }
            shootCoolTime = 0.0f;
        }
    }

    //�ҷ��� ���Ϳ� ����� ���� ȿ���� ���Ѵ�.
    public void AttackEffect(Monster monster)
    {
        switch (ringBase.id)
        {
            case 0: //�ܼ� ����
            case 2:
                monster.AE_DecreaseHP(curATK, Color.red);
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
                monster.AE_Explosion(curATK, id4Splash);
                monster.PlayParticleCollision(ringBase.id, 0.0f);
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
        ChangeCurEFF(0.0f);

        //�ٸ� ���� ���Ͽ� ���� ȿ���� ����
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            ring = DeckManager.instance.rings[i];
            if (ring == this) continue;
            if (Vector2.Distance(ring.transform.position, transform.position) <= ringBase.range + ring.collider.radius)
            {
                switch (ringBase.id)
                {
                    case 0:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurATK(0.1f);
                        ring.ChangeCurATK(0.05f);
                        break;
                    case 1:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurNumTarget(1.0f);
                        ring.ChangeCurNumTarget(0.5f);
                        break;
                    case 3:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurEFF(0.5f);
                        ring.ChangeCurEFF(0.1f);
                        break;
                    case 4:
                        if (ring.ringBase.id == ringBase.id) ring.id4Splash += 0.05f;
                        ring.ChangeCurATK(0.05f);
                        break;
                    default: //�ƹ� ȿ�� ����
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
                    case 0:
                        if (ring.ringBase.id == ringBase.id) ChangeCurATK(0.1f);
                        ChangeCurATK(0.05f);
                        break;
                    case 1:
                        if (ring.ringBase.id == ringBase.id) ChangeCurNumTarget(1.0f);
                        ChangeCurNumTarget(0.5f);
                        break;
                    case 3:
                        if (ring.ringBase.id == ringBase.id) ChangeCurEFF(0.5f);
                        ChangeCurEFF(0.1f);
                        break;
                    case 4:
                        if (ring.ringBase.id == ringBase.id) id4Splash += 0.05f;
                        ChangeCurATK(0.05f);
                        break;
                    default: //�ƹ� ȿ�� ����
                        break;
                }
        }
    }

    //�ó����� �����Ѵ�.
    public void RemoveSynergy()
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
                    case 0:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurATK(-0.1f);
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 1:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurNumTarget(-1.0f);
                        ring.ChangeCurNumTarget(-0.5f);
                        break;
                    case 3:
                        if (ring.ringBase.id == ringBase.id) ring.ChangeCurEFF(-0.5f);
                        ring.ChangeCurEFF(-0.1f);
                        break;
                    case 4:
                        if (ring.ringBase.id == ringBase.id) ring.id4Splash -= 0.05f;
                        ring.ChangeCurATK(-0.05f);
                        break;
                    case 2: //�ƹ� ȿ�� ����
                        break;
                }
            }
        }
    }

    //��ġ ���� �������� Ȯ���Ѵ�.
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

    //ȿ�� ���ӽð��� �����Ѵ�.
    public void ChangeCurEFF(float buff)
    {
        buffEFF += buff;
        curEFF = ringBase.baseEFF + buffEFF;
        if (curEFF < 0) curEFF = 0;
    }
}
