using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PathCreation;

public class Monster : MonoBehaviour
{
    public int id;  //���� ���̵�(�ҷ��� �ùٸ� Ÿ���� ������ �� ����)

    //�⺻ ����
    public BaseMonster baseMonster;

    //���� ����
    public float baseHP;
    public float curHP;
    public float baseSPD;
    public float curSPD;

    //�̵� ����
    public PathCreator path;    //�̵� ���
    public float movedDistance; //�ʿ����� �̵� �Ÿ�

    //�׷���
    SpriteRenderer spriteRenderer;  //���� �̹���
    public TextMesh hpText;   //HP �ؽ�Ʈ

    //��Ÿ ����
    float snowTime;         //������ ���ο� ȿ���� ���ӵ� �ð�
    float snowEndTime;      //������ ���ο� ȿ���� ������ �ð�
    Dictionary<int, float> poisonDmg;   //�͵��� ������
    Dictionary<int, float> poisonTime;  //�͵��� ������ ��Ÿ��
    public bool isInBlizzard; //������ �ӿ� �ִ��� ����
    float paralyzeTime;         //������ ���� ȿ���� ���ӵ� �ð�
    float paralyzeEndTime;      //������ ���� ȿ���� ������ �ð�
    public bool barrierBlock; //���κ��� �̵��� ���ع��� �ʴ��� ����
    public int curseStack;     //���ַκ��� ���� ����
    public bool isInAmplify;    //���� ���� ���� �ִ��� ����

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        poisonTime = new Dictionary<int, float>();
        poisonDmg = new Dictionary<int, float>();
    }

    void Update()
    {
        if (curHP > 0)  //����ִ� ��쿡�� ������ �� ����.
        {
            curSPD = baseSPD;

            spriteRenderer.color = Color.white; //����(���� �̻� ǥ�� ��)�� �ʱ�ȭ


            //�̵����ش� ���Ѽ�->���Ѽ����� ����Ǿ�� �Ѵ�.
            if (isInBlizzard) //������ ���̶��
            {
                curSPD = baseSPD * 0.7f;
                spriteRenderer.color = Color.cyan;
            }

            if (snowTime < snowEndTime) //���� ���� ��ȭ�� �������̶��
            {
                curSPD = baseSPD * 0.5f;
                snowTime += Time.deltaTime;
                spriteRenderer.color = Color.cyan;
            }

            if (paralyzeTime < paralyzeEndTime) //���� ���� ���� �������̶��
            {
                curSPD = 0;
                paralyzeTime += Time.deltaTime;
                spriteRenderer.color = Color.yellow;
            }

            if (poisonDmg.Count > 0)  //�͵� ��ø�� �ϳ��� �ִٸ�
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

            if (curseStack != 0) spriteRenderer.color = Color.gray;     //���� ��ø�� �ϳ��� �ִٸ�

            if (!barrierBlock) //��迡 ������ ������ �̵�
            {
                movedDistance += curSPD * Time.deltaTime;
                transform.position = path.path.GetPointAtDistance(movedDistance);
                transform.Translate(0.0f, 0.0f, id * 0.001f);
            }
        }
    }

    //���͸� �ʱ�ȭ�Ѵ�.
    public void InitializeMonster(int _id, BaseMonster monster, int pathID, float scale)
    {
        //���̵�
        id = _id;

        //����
        baseMonster = monster;
        baseHP = baseMonster.hp * scale;
        curHP = baseHP;
        baseSPD = baseMonster.spd;

        //�̵� ����
        path = GameManager.instance.monsterPaths[pathID];
        movedDistance = 0.0f;

        //�׷���
        spriteRenderer.sprite = GameManager.instance.monsterSprites[baseMonster.type];
        spriteRenderer.color = Color.white; //����(���� �̻� ǥ�� ��)�� �ʱ�ȭ
        SetHPText();

        //��Ÿ ����
        snowEndTime = -1.0f;
        poisonDmg.Clear();
        poisonTime.Clear();
        isInBlizzard = false;
        paralyzeEndTime = -1.0f;
        barrierBlock = false;
        curseStack = 0;
        isInAmplify = false;
    }

    //��ƼŬ�� �÷����Ѵ�.
    public void PlayParticleCollision(int id, float time)
    {
        //�ǰ� ��ƼŬ�� ������ �����Ѵ�.
        ParticleChecker par = GameManager.instance.GetParticleFromPool(id);

        //�÷����Ѵ�.
        par.PlayParticle(transform, time);
    }

    //HP �ؽ�Ʈ�� �����Ѵ�. �Ҽ����� ������ ǥ���ϵ�, ���� HP�� 0�ʰ� 1������ ���� 1�� ǥ���Ѵ�.
    private void SetHPText()
    {
        if (curHP > 1.0f) hpText.text = ((int)curHP).ToString();
        else if (curHP > 0.0f) hpText.text = "1";
        else hpText.text = "0";
    }
    
    //�׾����� Ȯ���Ѵ�. ��� �ִϸ��̼� �� ��� �� ���ӿ��� �ڽ��� �����Ѵ�.
    void CheckDead()
    {
        if (curHP <= 0)
        {
            /*�߰� ���� �ʿ� - ��� �ִϸ��̼�*/
            RemoveFromBattle(0.5f);
        }
    }

    //���ӿ��� time�� �� �����Ѵ�.
    public void RemoveFromBattle(float time)
    {
        if (time == 0.0f) InvokeRemoveFromBattle();
        else Invoke("InvokeRemoveFromBattle", time);
    }

    //������ ���ӿ��� �����Ѵ�. ���� ���� ���� ��� ȿ���� �����Ѵ�. ���/RP�� ������Ų��.
    void InvokeRemoveFromBattle()
    {
        if (!gameObject.activeSelf) return;

        BattleManager.instance.monsters.Remove(this);

        //���� ���� ����� ���� ȿ���� �����Ѵ�.
        if (curseStack != 0) AE_CurseDead();

        //BattleManager.instance.totalGetGold += 10;
        //BattleManager.instance.totalKilledMonster++;
        //BattleManager.instance.rp += BattleManager.instance.genRP;
        //UIManager.instance.SetIngameTotalRPText();


        //if (growRings.Count > 0) AE_GrowDead();
        //if (DeckManager.instance.isNecro) DeckManager.instance.necroCnt++;

        //HP�� 0���� �ٲ۴�.
        curHP = 0;

        GameManager.instance.ReturnMonsterToPool(this);
    }

    //�Ϲ� ��������/����Ʈ �������� �˷��ش�.
    public bool IsNormalMonster()
    {
        if (baseMonster.type < 3) return true;
        else return false;
    }

    //���� ����Ʈ: HP����
    public void AE_DecreaseHP(float dmg, Color32 color)
    {
        if (dmg != -1)
        {
            if (isInAmplify) dmg *= 1.2f;
            curHP -= dmg;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText((Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
            t.gameObject.SetActive(true);
        }
        else
        {
            curHP = 0;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText("X", transform.position, Color.black);
            t.gameObject.SetActive(true);
        }
        SetHPText();
        CheckDead();
    }

    //���� ����Ʈ: ����
    public void AE_Snow(float dmg, float time)
    {
        AE_DecreaseHP(dmg, new Color32(0, 0, 180, 255));
        //�̹� �ް��ִ� ���� ���� ��ȭ ȿ���� �� ���� ���ٸ� ���� ���
        if (snowEndTime - snowTime > time) return;
        snowEndTime = time;
        snowTime = 0;
    }

    //���� ����Ʈ: ����
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

    //���� ����Ʈ: �͵�
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

    //���� ����Ʈ: ����
    public void AE_Paralyze(float dmg, float time)
    {
        AE_DecreaseHP(dmg, new Color32(150, 150, 0, 255));
        //�̹� �ް��ִ� ���� ���� ���� ȿ���� �� ���� ���ٸ� ���� ���
        if (paralyzeEndTime - paralyzeTime > time) return;
        paralyzeEndTime = time;
        paralyzeTime = 0;
    }

    //���� ����Ʈ: ��ȯ(�ڷ���Ʈ)
    public void AE_Teleport(float dmg, float prob)
    {
        AE_DecreaseHP(dmg, new Color32(255, 0, 200, 255));
        if (Random.Range(0.0f, 1.0f) < prob)
        {
            movedDistance = 0.0f;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText("��ȯ!", transform.position, new Color32(255, 0, 200, 255));
            t.gameObject.SetActive(true);
        }
    }

    //���� ����Ʈ: ����
    public void AE_Cut(float dmg)
    {
        if (IsNormalMonster()) AE_DecreaseHP(curHP * (dmg * 0.01f), Color.red);
        else AE_DecreaseHP(curHP * (dmg * 0.005f), Color.red);
    }

    //���� ����Ʈ: ����
    public void AE_Curse(int stack)
    {
        curseStack += stack;
    }

    //���� ����Ʈ: ������ ��� ȿ��
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

    //���� ����Ʈ: ó��
    public void AE_Execution(float dmg, float rate)
    {
        if (baseMonster.type > 2) rate *= 0.5f;
        if (curHP < baseHP * rate) AE_DecreaseHP(-1, Color.red);
        else AE_DecreaseHP(dmg, new Color32(70, 70, 70, 255));
    }
}
