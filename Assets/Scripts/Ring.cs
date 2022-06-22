using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ring : MonoBehaviour
{
    public Ringstone ringBase;  //���� �⺻ ����

    //����
    public float curNumTarget;  //Ÿ�� ��
    public float curDMG;   //���� ������
    public float curSPD;   //���� ���� ��Ÿ��
    public float curEFF;   //���� ȿ�� ���� �ð�
    public float buffNumTarget; //Ÿ�� �� ��ȭ��(�״�� ���ϱ� �Ѵ�. 0.0f�� ����)
    public float buffDMG;       //������ ��ȭ��(�״�� ���ϱ� �Ѵ�. 1.0f�� ����)
    public float buffSPD;       //���� ��Ÿ�� ��ȭ��(�״�� ���ϱ� �Ѵ�. 1.0f�� ����. 0.2f���� �۾Ƶ� 0.2f�� ����)
    public float buffEFF;       //ȿ�� ���ӽð� ��ȭ��(�״�� ���ϱ��Ѵ�. 1.0f�� ����)

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
                    targets = targets.OrderByDescending(x => x.movedDistance).ToList();
                    for (int i = 0; i < numTarget; i++)
                    {
                        bullet = GameManager.instance.GetBulletFromPool(ringBase.id);
                        bullet.InitializeBullet(this, targets[i]);
                        bullet.gameObject.SetActive(true);
                    }
                    break;
                case 1: //����Ʈ�� ���� ���� �� ���� ��
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

    //�ҷ��� ���Ϳ� ����� ���� ȿ���� ���Ѵ�.
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

    //�ó����� �����Ѵ�.
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

        //�ٸ� ���� ���Ͽ�
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

        //�ٸ� �����κ���
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

    //�ó����� �����Ѵ�.
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
