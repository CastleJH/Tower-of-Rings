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
    //SpriteRenderer spriteRenderer;  //���� �̹���
    //public SPUM_SpriteList spumSprites;
    public Animator anim;
    public TextMesh hpText;   //HP �ؽ�Ʈ
    float prevX;

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
    float skillCoolTime1;    //����Ʈ/���� ������ ��ų ��Ÿ��
    float skillCoolTime2;
    float skillUseTime;     //����Ʈ/���� ������ ��ų ���ӵ� �ð�
    bool immuneDamage;
    bool immuneInterrupt;
    int cloneID;

    void Awake()
    {        
        poisonTime = new Dictionary<int, float>();
        poisonDmg = new Dictionary<int, float>();
    }

    void OnEnable()
    {
        anim.SetTrigger("AddScene");
    }

    void Update()
    {
        if (isInBattle)
        {
            baseSPD = baseMonster.spd;
            if (!IsNormalMonster()) UseMonsterSkill();
            curSPD = baseSPD;

            //spriteRenderer.color = Color.white; //����(���� �̻� ǥ�� ��)�� �ʱ�ȭ

            if (!immuneInterrupt)
            {
                //�̵����ش� ���Ѽ�->���Ѽ����� ����Ǿ�� �Ѵ�.
                if (isInBlizzard) //������ ���̶��
                {
                    curSPD = baseSPD * 0.7f;
                    //spriteRenderer.color = Color.cyan;
                }

                if (snowTime < snowEndTime) //���� ���� ��ȭ�� �������̶��
                {
                    curSPD = baseSPD * 0.5f;
                    snowTime += Time.deltaTime;
                    //spriteRenderer.color = Color.cyan;
                }

                if (paralyzeTime < paralyzeEndTime) //���� ���� ���� �������̶��
                {
                    curSPD = 0;
                    paralyzeTime += Time.deltaTime;
                    //spriteRenderer.color = Color.yellow;
                }

                if (barrierBlock) curSPD = 0;
            }

            if (poisonDmg.Count > 0)  //�͵� ��ø�� �ϳ��� �ִٸ�
            {
                //spriteRenderer.color = Color.green;
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

            //if (curseStack != 0) spriteRenderer.color = Color.gray;     //���� ��ø�� �ϳ��� �ִٸ�

            movedDistance += curSPD * Time.deltaTime;
            anim.speed = curSPD;

            prevX = transform.position.x;
            transform.position = path.path.GetPointAtDistance(movedDistance);
            transform.Translate(0.0f, 0.0f, id * 0.001f);

            if (prevX > transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
                hpText.transform.localScale = new Vector3(0.1f, 0.1f, 1);
            }
            else
            {
                transform.localScale = new Vector3(-1, 1, 1);
                hpText.transform.localScale = new Vector3(-0.1f, 0.1f, 1);
            }
        }
    }

    //���͸� �ʱ�ȭ�Ѵ�.
    public void InitializeMonster(int _id, int pathID, float scale)
    {
        //���̵�
        id = _id;

        //����
        baseHP = baseMonster.hp * scale;
        curHP = baseHP;
        baseSPD = baseMonster.spd;

        //�̵� ����
        path = GameManager.instance.monsterPaths[pathID];
        movedDistance = 0.0f;

        //�׷���
        //CopySPUMDataFromBase();
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
        if (IsNormalMonster() || baseMonster.type == 23) skillCoolTime1 = 0.0f;
        else skillCoolTime1 = 5.0f;
        skillCoolTime2 = 0.0f;
        skillUseTime = 0.0f;
        immuneDamage = false;
        immuneInterrupt = false;
        cloneID = -1;
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
    public void SetHPText()
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
            anim.ResetTrigger("AddScene");
            anim.speed = 0.8f;
            anim.SetTrigger("Die");
            ApplyDeadEffect();
            Invoke("InvokeReturnMonsterToPool", 1.0f);
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
        if (baseMonster.type < 15) return true;
        else return false;
    }

    //����Ʈ/������ ��ų�� ����Ѵ�.
    void UseMonsterSkill()
    {
        switch (baseMonster.type)
        {
            case 15:
                Elite_Berserker();
                break;
            case 16:
                Elite_Messenger();
                break;
            case 17:
                Elite_Giant();
                break;
            case 18:
                Elite_DarkPriest();
                break;
            case 19:
                Elite_DevilCommander();
                break;
            case 20:
                Elite_Purifier();
                break;
            case 21:
                Elite_Predator();
                break;
            case 22:
                Boss_Puppeteer();
                break;
            case 23:
                Boss_CloneMage();
                break;
            case 24:
                Boss_Sealer();
                break;
            case 25:
                Boss_DarkWarlock();
                break;
            case 26:
                Boss_Spacer();
                break;
            case 27:
                Boss_Executer();
                break;
            case 28:
                Boss_King();
                break;
        }
    }

    //���� ����Ʈ: HP����
    public void AE_DecreaseHP(float dmg, Color32 color)
    {
        if (dmg >= 0 || color != Color.green)
        {
            if (immuneDamage) dmg = 0;
            if (dmg != -1)
            {
                if (isInAmplify) dmg *= 1.2f;
                curHP -= dmg;
                if (curHP > baseHP) curHP = baseHP;
                DamageText t = GameManager.instance.GetDamageTextFromPool();
                t.InitializeDamageText((Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
                t.gameObject.SetActive(true);
            }
            else curHP = 0;
        }
        else
        {
            curHP -= dmg;
            if (curHP > baseHP) curHP = baseHP;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText((-Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
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

    void Elite_Berserker()
    {
        baseSPD = baseMonster.spd * (1.0f + (baseHP - curHP) / baseHP);
    }

    void Elite_Messenger()
    {
        if (skillUseTime != 0)
        {
            baseSPD = baseMonster.spd * 2;
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 2.0f)
            {
                skillUseTime = 0.0f;
                skillCoolTime1 = 0.0f;
            }
        }
        else
        {
            skillCoolTime1 += Time.deltaTime;
            if (skillCoolTime1 > 5.0f)
            {
                skillCoolTime1 = 0.0f;
                skillUseTime = 0.0001f;
            }
        }
    }

    void Elite_Giant()
    {
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
    }

    void Elite_DarkPriest()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 1.0f)
        {
            skillCoolTime1 = 0.0f;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            Monster monster;

            for (int i = 0; i < colliders.Length; i++)
                if (colliders[i].tag == "Monster")
                {
                    monster = colliders[i].GetComponent<Monster>();
                    if (monster.curHP > 0 && monster != this) monster.AE_DecreaseHP(-monster.baseHP * 0.05f, Color.green);
                }
        }
    }

    void Elite_DevilCommander()
    {
        Monster monster;
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (monster != this && Vector2.Distance(monster.transform.position, transform.position) < 1.5f)
                monster.baseSPD = monster.baseMonster.spd * 1.2f;
            else monster.baseSPD = monster.baseMonster.spd;
        }
    }

    void Elite_Purifier()
    {
        immuneInterrupt = true;
    }

    void Elite_Predator()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 5.0f)
        {
            skillCoolTime1 = 0.0f;

            BattleManager.instance.ChangeCurrentRP((int)BattleManager.instance.rp * 0.8f);
        }
    }

    void Boss_Puppeteer()
    {
        Monster monster;
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            monster = GameManager.instance.GetMonsterFromPool(29);
            monster.gameObject.transform.position = new Vector2(100, 100);  //�ʱ⿡�� �ָ� ����߷����ƾ� path�� �߰����� �۸�ġ ���� ����.
            monster.InitializeMonster(cloneID--, FloorManager.instance.curRoom.pathID, 1.0f);    //�׿ܿ��� �Ϲ� ����
            monster.movedDistance = movedDistance + 1.0f;
            monster.gameObject.SetActive(true);
            BattleManager.instance.monsters.Add(monster);
        }

        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (monster.id < 0)
            {
                immuneDamage = true;
                return;
            }
        }
        immuneDamage = false;
    }

    void Boss_CloneMage()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            Monster monster;
            float scale;
            if (FloorManager.instance.floor.floorNum == 7) scale = 4.0f;
            else scale = 0.5f * (FloorManager.instance.floor.floorNum + 1);

            monster = GameManager.instance.GetMonsterFromPool(23);
            monster.gameObject.transform.position = new Vector2(100, 100);  //�ʱ⿡�� �ָ� ����߷����ƾ� path�� �߰����� �۸�ġ ���� ����.
            monster.InitializeMonster(cloneID--, FloorManager.instance.curRoom.pathID, scale);    //�׿ܿ��� �Ϲ� ����

            monster.movedDistance = movedDistance + 0.5f;
            monster.curHP = curHP * 0.666f;
            monster.SetHPText();

            monster.gameObject.SetActive(true);
            BattleManager.instance.monsters.Add(monster);

            movedDistance -= 0.5f;
            curHP *= 0.666f;
            SetHPText();
        }
    }

    void Boss_Sealer()
    {
        if (skillUseTime != 0)
        {
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 5.0f)
            {
                skillUseTime = 0.0f;
                skillCoolTime1 = 0.0f;
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) DeckManager.instance.rings[i].isSealed = false;
            }
        }
        else
        {
            skillCoolTime1 += Time.deltaTime;
            if (skillCoolTime1 >= 10.0f)
            {
                skillUseTime = 0.001f;
                anim.SetTrigger("Attack");

                int targetSealNum = DeckManager.instance.rings.Count / 2;
                int sealNum = 0;
                int tarIdx;
                while (sealNum < targetSealNum)
                {
                    do tarIdx = Random.Range(0, DeckManager.instance.rings.Count);
                    while (DeckManager.instance.rings[tarIdx].isSealed);
                    DeckManager.instance.rings[tarIdx].isSealed = true;
                    sealNum++;
                }
            }
        }
    }

    void Boss_DarkWarlock()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)
        {
            int ringID;
            bool canDowngrade = false;
            for (int i = 0; i < DeckManager.instance.deck.Count; i++)
            {
                ringID = DeckManager.instance.deck[i];
                if (GameManager.instance.ringDB[ringID].level > 1)
                {
                    canDowngrade = true;
                    break;
                }
            }
            if (canDowngrade)
            {
                skillCoolTime1 = 0.0f;
                anim.SetTrigger("Attack");

                do ringID = DeckManager.instance.deck[Random.Range(0, DeckManager.instance.deck.Count)];
                while (GameManager.instance.ringDB[ringID].level == 1);
                GameManager.instance.ringDB[ringID].Downgrade();
                BattleManager.instance.ringDowngrade.Add(ringID);
            }
        }
    }

    void Boss_Spacer()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            Monster monster1, monster2;
            int monsterNum = BattleManager.instance.monsters.Count;
            float tmp;
            for (int i = monsterNum - 1; i >= 0; i--)
            {
                monster1 = BattleManager.instance.monsters[i];
                monster2 = BattleManager.instance.monsters[Random.Range(0, monsterNum)];
                if (!monster1.IsNormalMonster() || !monster2.IsNormalMonster()) continue;
                tmp = monster1.movedDistance;
                monster1.movedDistance = monster2.movedDistance;
                monster2.movedDistance = tmp;
            }
        }
    }

    void Boss_Executer()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.rings[Random.Range(0, DeckManager.instance.rings.Count)]);
        }
    }

    void Boss_King()
    {
        if (skillUseTime != 0)
        {
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 5.0f)
            {
                skillUseTime = 0.0f;
                skillCoolTime1 = 0.0f;
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) DeckManager.instance.rings[i].isSealed = false;
            }
        }
        else 
        {
            skillCoolTime1 += Time.deltaTime;
            if (skillCoolTime1 >= 10.0f)
            {
                skillCoolTime1 = 0.0f;
                anim.SetTrigger("Attack");

                bool canDowngrade = false;
                do
                {
                    switch (Random.Range(0, 3))
                    {
                        case 0:
                            DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.rings[Random.Range(0, DeckManager.instance.rings.Count)]);
                            canDowngrade = true;
                            break;
                        case 1:
                            int ringID;
                            for (int i = 0; i < DeckManager.instance.deck.Count; i++)
                            {
                                ringID = DeckManager.instance.deck[i];
                                if (GameManager.instance.ringDB[ringID].level > 1)
                                {
                                    canDowngrade = true;
                                    break;
                                }
                            }
                            if (canDowngrade)
                            {
                                skillCoolTime1 = 0.0f;
                                do ringID = DeckManager.instance.deck[Random.Range(0, DeckManager.instance.deck.Count)];
                                while (GameManager.instance.ringDB[ringID].level == 1);
                                GameManager.instance.ringDB[ringID].Downgrade();
                                BattleManager.instance.ringDowngrade.Add(ringID);
                            }
                            break;
                        case 2:
                            skillUseTime = 0.001f;

                            int targetSealNum = DeckManager.instance.rings.Count / 2;
                            int sealNum = 0;
                            int tarIdx;
                            while (sealNum < targetSealNum)
                            {
                                do tarIdx = Random.Range(0, DeckManager.instance.rings.Count);
                                while (DeckManager.instance.rings[tarIdx].isSealed);
                                DeckManager.instance.rings[tarIdx].isSealed = true;
                                sealNum++;
                            }
                            canDowngrade = true;
                            break;
                    }
                } while (!canDowngrade);
            }
        }

        skillCoolTime2 += Time.deltaTime;
        if (skillCoolTime2 >= 5.0f)
        {
            skillCoolTime2 = 0.0f;
            anim.SetTrigger("Attack");

            switch (Random.Range(0, 2))
            {
                case 0:
                    immuneInterrupt = true;
                    break;
                case 1:
                    immuneInterrupt = false;
                    AE_DecreaseHP((baseHP - curHP) * 0.05f, Color.green);
                    break;
            }
        }
    }

    /*private void CopySPUMDataFromBase()
    {
        SPUM_Prefabs _baseData = Resources.Load<SPUM_Prefabs>(string.Format("SPUM/SPUM_Units/Unit{0:D3}", baseMonster.type));
        if (_baseData == null) Debug.LogError(string.Format("SPUM named Unit{0:D3} does not exist in Assets/Resources/SPUM/SPUM_Units", baseMonster.type));
        else
        {
            SyncSprites(spumSprites._itemList, _baseData._spriteOBj._itemList);
            SyncSprites(spumSprites._eyeList, _baseData._spriteOBj._eyeList);
            SyncSprites(spumSprites._hairList, _baseData._spriteOBj._hairList);
            SyncSprites(spumSprites._bodyList, _baseData._spriteOBj._bodyList);
            SyncSprites(spumSprites._clothList, _baseData._spriteOBj._clothList);
            SyncSprites(spumSprites._armorList, _baseData._spriteOBj._armorList);
            SyncSprites(spumSprites._pantList, _baseData._spriteOBj._pantList);
            SyncSprites(spumSprites._weaponList, _baseData._spriteOBj._weaponList);
            SyncSprites(spumSprites._backList, _baseData._spriteOBj._backList);
        }
    }

    private void SyncSprites(List<SpriteRenderer> _target, List<SpriteRenderer> _base)
    {
        for (int i = _base.Count - 1; i >= 0; i--)
        {
            _target[i].sprite = _base[i].sprite;
            _target[i].color = _base[i].color;
        }
    }*/
}
