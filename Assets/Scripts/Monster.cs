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
    bool isInBattle;     //��Ƽ� ������ ���������� ����
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
    float skillCoolTime;    //����Ʈ/���� ������ ��ų ��Ÿ��
    float skillUseTime;     //����Ʈ/���� ������ ��ų ���ӵ� �ð�
    bool immuneDamage;
    bool immuneInterrupt;

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
            if (!IsNormalMonster()) UseMonsterSkill();
            curSPD = baseSPD;

            spriteRenderer.color = Color.white; //����(���� �̻� ǥ�� ��)�� �ʱ�ȭ

            if (!immuneInterrupt)
            {
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

                if (barrierBlock) curSPD = 0;
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

            movedDistance += curSPD * Time.deltaTime;
            transform.position = path.path.GetPointAtDistance(movedDistance);
            transform.Translate(0.0f, 0.0f, id * 0.001f);
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
        if (curHP <= 0 && isInBattle)
        {
            isInBattle = false;
            RemoveFromBattle(true);
        }
    }

    //���ӿ��� �����Ѵ�. isDead = true�� ������� ���� ȿ���� ����.
    public void RemoveFromBattle(bool isDead)
    {
        if (!gameObject.activeSelf) return;

        curHP = 0;  //HP�� 0���� �ٲ۴�.
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

    //��� ȿ���� �����Ѵ�. (��� ȹ�浵 ����)
    void ApplyDeadEffect()
    {
        //���� ���� ����� ���� ȿ���� �����Ѵ�.
        if (curseStack != 0) AE_CurseDead();

        //��ũ�� ���� ����� ī�����Ѵ�.
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

        //���� ���͸� 20��� ȹ���Ѵ�. �Ϲ� ���͸� 66%Ȯ���� 1, 33%Ȯ���� 2��� ȹ���Ѵ�.
        int g;
        if (IsNormalMonster()) g = Mathf.Clamp(Random.Range(0, 3), 1, 2);
        else g = 20;
        BattleManager.instance.goldGet += g;
    }

    //�Ϲ� ��������/����Ʈ �������� �˷��ش�.
    public bool IsNormalMonster()
    {
        if (baseMonster.type < 3) return true;
        else return false;
    }

    //����Ʈ/������ ��ų�� ����Ѵ�.
    void UseMonsterSkill()
    {
        Monster monster;
        switch (baseMonster.type)
        {
            case 3:
                baseSPD = baseMonster.spd * 1.0f + 0.5f * ((baseHP - curHP) / baseHP);
                break;
            case 4:
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
                    baseSPD = baseMonster.spd;
                    skillCoolTime += Time.deltaTime;
                    if (skillCoolTime > 5.0f)
                    {
                        skillCoolTime = 0.0f;
                        skillUseTime = 0.0001f;
                    }
                }
                break;
            case 5:
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
                break;
            case 6:
                skillCoolTime += Time.deltaTime;
                if (skillCoolTime >= 1.0f)
                {
                    skillCoolTime = 0.0f;

                    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);

                    for (int i = 0; i < colliders.Length; i++)
                        if (colliders[i].tag == "Monster")
                        {
                            monster = colliders[i].GetComponent<Monster>();
                            if (monster.curHP > 0 && monster != this) monster.AE_DecreaseHP(monster.baseHP * 0.05f, new Color32(0, 255, 0, 255));
                        }
                }
                break;
            case 7:
                for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
                {
                    monster = BattleManager.instance.monsters[i];
                    if (monster != this && Vector2.Distance(monster.transform.position, transform.position) < 1.5f)
                        monster.baseSPD = monster.baseMonster.spd * 1.2f;
                    else monster.baseSPD = monster.baseMonster.spd;
                }
                break;
            case 8:
                immuneInterrupt = true;
                break;
            case 9:
                skillCoolTime += Time.deltaTime;
                if (skillCoolTime >= 5.0f)
                {
                    skillCoolTime = 0.0f;

                    BattleManager.instance.ChangeCurrentRP((int)BattleManager.instance.rp * 0.8f);
                }
                break;
            case 10:
                break;
            case 11:
                break;
            case 12:
                break;
            case 13:
                break;
            case 14:
                break;
            case 15:
                break;
            case 16:
                break;
        }
    }

    //���� ����Ʈ: HP����
    public void AE_DecreaseHP(float dmg, Color32 color)
    {
        if (immuneDamage) dmg = 0;
        if (dmg != -1)
        {
            if (isInAmplify) dmg *= 1.2f;
            curHP -= dmg;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText((Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
            t.gameObject.SetActive(true);
        }
        else curHP = 0;
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
        if (!IsNormalMonster()) rate *= 0.5f;
        if (curHP - dmg < baseHP * rate)
        {
            AE_DecreaseHP(-1, Color.red);
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText("ó��!", transform.position, new Color32(70, 70, 70, 255));
            t.gameObject.SetActive(true);
        }
        else AE_DecreaseHP(dmg, new Color32(70, 70, 70, 255));
    }

    //���� ����Ʈ: ����
    public void AE_Grow(float dmg, Ring ring)
    {
        AE_DecreaseHP(dmg, new Color32(30, 180, 30, 255));
        if (curHP <= 0 && ring.growStack < 20)
        {
            ring.growStack++;
            ring.ChangeCurATK(0.1f);
        }
    }

    //���� ����Ʈ: õ��
    public void AE_Angel()
    {
        AE_DecreaseHP(curHP * 0.2f, Color.yellow);
        movedDistance = 0;
    }

    //���� ����Ʈ: ����
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

    //���� ����Ʈ: ���
    public void AE_InstantDeath(float dmg, float prob)
    {
        if (Random.Range(0.0f, 1.0f) < prob)
        {
            if (!IsNormalMonster()) AE_DecreaseHP(baseHP * prob * 0.5f, new Color32(80, 80, 80, 255));
            else
            {
                AE_DecreaseHP(-1, Color.black);
                DamageText t = GameManager.instance.GetDamageTextFromPool();
                t.InitializeDamageText("���!", transform.position, new Color32(80, 80, 80, 255));
                t.gameObject.SetActive(true);
            }
        }
        else AE_DecreaseHP(dmg, new Color32(80, 80, 80, 255));
    }
}
