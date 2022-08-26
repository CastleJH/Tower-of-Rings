using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PathCreation;

public class Monster : MonoBehaviour
{
    public int id;  //���� ���̵�(�ҷ��� �ùٸ� Ÿ���� ������ �� ����)

    //�⺻ ����
    public BaseMonster baseMonster;

    //������ ���� ����
    public float maxHP;
    public float curHP;
    public float baseSPD;       //�� �̵� ���� ȿ�� ���� �� �ӵ�
    public float curSPD;        //�� �̵� ���� ȿ�� ���� �� �ӵ�

    //�̵� ����
    public PathCreator path;    //�̵� ���
    public float movedDistance; //�ʿ����� �̵� �Ÿ�

    //�׷���
    public Animator anim;
    public TextMesh hpText;     //HP �ؽ�Ʈ
    float prevX;                //���� �������� x��ǥ(�� ���� ���� ��/�� ������ flip��)

    //��Ÿ ����
    bool isInBattle;            //��Ƽ� ������ ���������� ����
    float snowTime;             //������ ���ο� ȿ���� ���ӵ� �ð�
    float snowEndTime;          //������ ���ο� ȿ���� ������ �ð�
    float poisonDmg;            //�͵��� ������
    float poisonTime;           //�͵��� ������ ��Ÿ��
    public bool isInBlizzard;   //������ �ӿ� �ִ��� ����
    float paralyzeTime;         //������ ���� ȿ���� ���ӵ� �ð�
    float paralyzeEndTime;      //������ ���� ȿ���� ������ �ð�
    public bool barrierBlock;   //���κ��� �̵��� ���ع��� �ʴ��� ����
    public int curseStack;      //���ַκ��� ���� ����
    public bool isInAmplify;    //���� ���� ���� �ִ��� ����
    public float amplifyInc;    //������
    float skillCoolTime1;       //����Ʈ/���� ������ ��ų ��Ÿ��
    float skillCoolTime2;       //����Ʈ/���� ������ ��ų ��Ÿ��
    float skillUseTime;         //����Ʈ/���� ������ ��ų ���ӵ� �ð�
    bool immuneDamage;          //������ �鿪 ����
    bool immuneInterrupt;       //�̵����� �鿪 ����
    int cloneID;                //���������� ������ ���� ID

    void OnEnable()
    {
        anim.SetTrigger("AddScene");    //�ִϸ��̼��� RunState�� ������.
    }

    void Update()
    {
        if (isInBattle)
        {
            //�⺻ �ӵ��� ���Ѵ�. �Ϲ� ���Ͱ� �ƴ϶�� ��ų�� �Ἥ �⺻ �ӵ��� �ٲ۴�.
            baseSPD = baseMonster.baseSPD;      
            if (baseMonster.tier != 'n') UseMonsterSkill();

            //���� �ӵ��� ���Ѵ�. ���� �̵� ���� ȿ������ �����Ѵ�.
            curSPD = baseSPD;
            if (!immuneInterrupt)
            {
                //�̵����ش� ���Ѽ�->���Ѽ����� üũ�Ѵ�. ���ӽð��� ���ҽ�Ű�鼭 �������� ���� ���� �̵� ���� ȿ���� �޵��� �ϱ� ������.
                if (isInBlizzard) curSPD = baseSPD * 0.7f; //������

                if (snowTime < snowEndTime) //����
                {
                    curSPD = baseSPD * 0.5f;
                    snowTime += Time.deltaTime;
                }

                if (paralyzeTime < paralyzeEndTime) //����
                {
                    curSPD = 0;
                    paralyzeTime += Time.deltaTime;
                }

                if (barrierBlock) curSPD = 0;  //���
            }

            //�͵� ��ø�� ������ �������� �޴´�.
            if (poisonDmg > 0)
            {
                poisonTime += Time.deltaTime;
                if (poisonTime >= 1.0f)
                {
                    AE_DecreaseHP(poisonDmg, Color.green);
                    PlayParticleCollision(5, 0.0f);
                    poisonTime = 0.0f;
                }
            }

            //���� �̼����� �ִϸ��̼� �ӵ��� �����Ѵ�.
            anim.speed = curSPD;

            //���� �̼����� ���� ��ġ�� �����Ѵ�.
            prevX = transform.position.x;
            movedDistance += curSPD * Time.deltaTime;
            transform.position = path.path.GetPointAtDistance(movedDistance) + new Vector3(0.0f, 0.0f, id * 0.001f);

            //���� ��ġ�� x��ǥ�� ���Ͽ� �¿�����Ѵ�.
            if (prevX > transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
                hpText.transform.localScale = new Vector3(0.1f, 0.1f, 1);
            }
            else if (prevX < transform.position.x)
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
        maxHP = baseMonster.baseMaxHP * scale;
        curHP = maxHP;
        baseSPD = baseMonster.baseSPD;
        SetHPText();

        //�̵� ����
        path = GameManager.instance.monsterPaths[pathID];
        movedDistance = 0.0f;

        //��Ÿ ����
        isInBattle = true;
        snowEndTime = -1.0f;
        poisonDmg = 0.0f;
        poisonTime = 0.0f;
        isInBlizzard = false;
        paralyzeEndTime = -1.0f;
        barrierBlock = false;
        curseStack = 0;
        isInAmplify = false;
        amplifyInc = 0.0f;
        if (baseMonster.tier == 'n' || baseMonster.type == 23) skillCoolTime1 = 0.0f;
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

    //��ƼŬ�� �÷����Ѵ�.
    public void PlayParticleCollision(int id, float time, float scale)
    {
        //�ǰ� ��ƼŬ�� ������ �����Ѵ�.
        ParticleChecker par = GameManager.instance.GetParticleFromPool(id);
        par.transform.localScale = Vector3.one * scale;

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

        isInBattle = false;
        curHP = 0;
        BattleManager.instance.monsters.Remove(this);

        if (isDead) //������� ���� �����ϴ� ���
        {
            if (baseMonster.tier == 'b') BattleManager.instance.isBossKilled = true;    //�������Ϳ����� ������ ȹ���� �� �ְ� �÷��׸� �ø���.

            //��� �ִϸ��̼��� �����ش�.
            anim.ResetTrigger("AddScene");
            anim.speed = 0.8f;
            anim.SetTrigger("Die");

            //������� ���� ���� �� ȿ���� ����.
            ApplyDeadEffect();

            //���͸� Ǯ�� ��� ��(�ִϸ��̼��� ���� ��) �ǵ����ش�.
            Invoke("InvokeReturnMonsterToPool", 1.0f);
        }
        else InvokeReturnMonsterToPool();   //�ƴϸ� �׳� �ٷ� �����Ѵ�.
    }

    //��� ȿ���� �����Ѵ�.
    void ApplyDeadEffect()
    {
        //���� ���� ����� ���� ȿ���� �����Ѵ�.
        if (curseStack != 0) AE_CurseDead();

        //��ɼ� ���� ��ȯ�� ���� ī������ �Ѵ�.
        if (DeckManager.instance.necroIdx != -1)
        {
            if (DeckManager.instance.necroCount < 10)   //��ɼ� ī��Ʈ�� 10�� �ɶ����� �Ѵ�.
            {
                DeckManager.instance.necroCount++;
                if (DeckManager.instance.necroCount >= 10)  //�� ��ȯ ���� �ؽ�Ʈ�� �����Ѵ�.
                {
                    DeckManager.instance.necroCount = 10;
                    UIManager.instance.SetBattleDeckRingRPText(DeckManager.instance.necroIdx, "10/10");
                    BattleManager.instance.ChangePlayerRP(0);
                }
                else UIManager.instance.SetBattleDeckRingRPText(DeckManager.instance.necroIdx, DeckManager.instance.necroCount.ToString() + "/10");
            }
        }
    }

    //������ ��ų�� ����Ѵ�(����Ʈ/������ ��츸 �Ҹ���).
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

    //�ǰ� ����Ʈ: HP���� (���� dmg�� �����̸� ȸ��, 1987654321�̸� ���/ó��. ���� color.r == 0�̰� ó���� 255�̴�.)
    public void AE_DecreaseHP(float dmg, Color32 color)
    {
        if (dmg >= 0)
        {
            if (immuneDamage) dmg = 0;
            if (dmg != 1987654321)
            {
                if (isInAmplify) dmg *= (1.0f + amplifyInc);   //���� ȿ���� �޴����̸� �������� �ø���.
                curHP -= dmg;
                //������ ���ͱ⸦ ǥ���Ѵ�. �Ҽ��� �Ʒ� ��°�ڸ������� ǥ��.
                DamageText t = GameManager.instance.GetDamageTextFromPool();
                t.InitializeDamageText((Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
                t.gameObject.SetActive(true);
            }
            else //���/ó���� ���
            {
                curHP = 0;
                DamageText t = GameManager.instance.GetDamageTextFromPool();
                if (color.r == 0) t.InitializeDamageText("���!", transform.position, new Color32(70, 70, 70, 255));
                else t.InitializeDamageText("ó��!", transform.position, new Color32(70, 70, 70, 255));
            }
        }
        else //HP ȸ���� ���
        {
            curHP -= dmg;
            if (curHP > maxHP) curHP = maxHP;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText((-Mathf.Round(dmg * 100) * 0.01f).ToString(), transform.position, color);
            t.gameObject.SetActive(true);
        }
        SetHPText();

        //��� �������� Ȯ���Ѵ�.
        CheckDead();
    }

    //�ǰ� ����Ʈ: ����
    public void AE_Snow(float dmg, float time)
    {
        AE_DecreaseHP(dmg, new Color32(0, 0, 180, 255));
        //�̹� �ް��ִ� ���� ���� ��ȭ ȿ���� �� ���� ���ٸ� ���� ���
        if (snowEndTime - snowTime > time) return;
        snowEndTime = time;
        snowTime = 0;
    }

    //�ǰ� ����Ʈ: ����
    public void AE_Explosion(float dmg, float splash)
    {
        //�ڽ� ��ó�� �ڽ��� �ƴ� �ٸ� ��� ���͵鿡 ���÷��� ������
        Monster monster;
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (monster.isInBattle && Vector2.Distance(transform.position, monster.transform.position) < 1.5f && monster != this) 
                monster.AE_DecreaseHP(dmg * splash, new Color32(150, 30, 30, 255));
        }
        //�ڽſ��� ���� ������
        AE_DecreaseHP(dmg, new Color32(150, 30, 30, 255));
    }

    //�ǰ� ����Ʈ: �͵�
    public void AE_Poison(float dmg)
    {
        poisonDmg += dmg;
    }

    //�ǰ� ����Ʈ: ����
    public void AE_Paralyze(float dmg, float time)
    {
        AE_DecreaseHP(dmg, new Color32(150, 150, 0, 255));
        //�̹� �ް��ִ� ���� ���� ���� ȿ���� �� ���� ���ٸ� ���� ���
        if (paralyzeEndTime - paralyzeTime > time) return;
        paralyzeEndTime = time;
        paralyzeTime = 0;
    }

    //�ǰ� ����Ʈ: ��ȯ(�ڷ���Ʈ)
    public void AE_Teleport(float dmg, float prob)
    {
        AE_DecreaseHP(dmg, new Color32(255, 0, 200, 255));
        if (Random.Range(0.0f, 1.0f) < prob) //���� Ȯ���� �̵� �Ÿ��� 0���� ��������
        {
            movedDistance = 0.0f;
            DamageText t = GameManager.instance.GetDamageTextFromPool();
            t.InitializeDamageText("��ȯ!", transform.position, new Color32(255, 0, 200, 255));
            t.gameObject.SetActive(true);
        }
    }

    //�ǰ� ����Ʈ: ����
    public void AE_Cut(float dmg)
    {
        if (baseMonster.tier == 'n') AE_DecreaseHP(curHP * (dmg * 0.01f), Color.red);
        else AE_DecreaseHP(curHP * (dmg * 0.005f), Color.red);
    }

    //�ǰ� ����Ʈ: ����
    public void AE_Curse(int stack)
    {
        curseStack += stack;
    }

    //�ǰ� ����Ʈ: ������ ��� ȿ��
    public void AE_CurseDead()
    {
        float dmg = curseStack;
        dmg = maxHP * dmg * 0.01f;

        //��� ���͵鿡 �ڽſ��� ���� ���� ���ÿ� ����Ͽ� ���ظ� ����
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            BattleManager.instance.monsters[i].PlayParticleCollision(21, 0.0f);
            BattleManager.instance.monsters[i].AE_DecreaseHP(dmg, new Color32(50, 50, 50, 255));
        }
    }

    //�ǰ� ����Ʈ: ó��
    public void AE_Execution(float dmg, float rate)
    {
        if (baseMonster.tier != 'n') rate *= 0.5f;  //����Ʈ/���� ���ʹ� ó������ �� ������
        if (curHP - dmg < maxHP * rate) AE_DecreaseHP(1987654321, Color.red); //�� �������� ���� ���� HP �̸��� �Ǹ� ó��
        else AE_DecreaseHP(dmg, new Color32(70, 70, 70, 255));
    }

    //�ǰ� ����Ʈ: ����
    public void AE_Grow(float dmg, Ring ring)
    {
        AE_DecreaseHP(dmg, new Color32(30, 180, 30, 255));
        if (curHP <= 0 && ring.growStack < 20) //�ִ� 20���� ����
        {
            ring.growStack++;
            ring.ChangeCurATK(0.1f);
        }
    }

    //�ǰ� ����Ʈ: õ��
    public void AE_Angel()
    {
        AE_DecreaseHP(curHP * 0.2f, Color.yellow);
        movedDistance = 0;  //��� �̵��Ÿ��� 0���� ����
    }

    //�ǰ� ����Ʈ: ����
    public void AE_Chase(float dmg, float radius)
    {
        Monster monster;
        //�ڽ��� ������ ���� �ݰ� �� ���� ��ο��� �������ֱ�
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (monster.isInBattle && Vector2.Distance(transform.position, monster.transform.position) < radius)
                monster.AE_DecreaseHP(dmg, new Color32(100, 0, 0, 255));
        }
    }

    //�ǰ� ����Ʈ: ���
    public void AE_InstantDeath(float dmg, float prob)
    {
        if (Random.Range(0.0f, 1.0f) < prob)    //���� Ȯ���� ���(�� �Ϲ� ���Ͱ� �ƴϸ� HP��� ����������)
        {
            if (baseMonster.tier != 'n') AE_DecreaseHP(maxHP * prob * 0.5f, new Color32(80, 80, 80, 255));
            else AE_DecreaseHP(1987654321, Color.black);
        }
        else AE_DecreaseHP(dmg, new Color32(80, 80, 80, 255));  //��� �ƴϸ� ����ϰ� ����������
    }

    //����Ʈ ���� ��ų: ������
    void Elite_Berserker()
    {
        baseSPD = baseMonster.baseSPD * (1.0f + (maxHP - curHP) / maxHP); //���� HP ��� �ӵ��� ������
    }

    //����Ʈ ���� ��ų: ����
    void Elite_Messenger()
    {
        if (skillUseTime != 0)  //��ų ������̶�� �̼��� �ι�� �����.
        {
            baseSPD = baseMonster.baseSPD * 2;
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 2.0f) //2�ʰ� ��ų�� ���ӵȴ�.
            {
                skillUseTime = 0.0f;
                skillCoolTime1 = 0.0f;
            }
        }
        else   //5�� ��Ÿ���� ��ٸ�
        {
            skillCoolTime1 += Time.deltaTime;
            if (skillCoolTime1 > 5.0f)
            {
                skillCoolTime1 = 0.0f;
                skillUseTime = 0.0001f;
            }
        }
    }

    //����Ʈ ���� ��ų: ö������
    void Elite_Giant()
    {
        if (curHP < maxHP * 0.2f) //�ִ� HP 20% �̸��� �Ǹ�
        {
            if (skillUseTime < 10.0f) //10�ʰ� ��� ����&�̵� ���� ȿ�� �鿪
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

    //����Ʈ ���� ��ų: ����Ű�
    void Elite_DarkPriest()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 1.0f) //1�ʸ���
        {
            skillCoolTime1 = 0.0f;

            //�ڽ��� ������ ���� �ݰ� �� ���� ����� HP�� ȸ��
            Monster monster;
            for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
            {
                monster = BattleManager.instance.monsters[i];
                if (monster.isInBattle && Vector2.Distance(transform.position, monster.transform.position) < 3.0f && monster != this)
                    monster.AE_DecreaseHP(-monster.maxHP * 0.05f, Color.green);
            }
        }
    }

    //����Ʈ ���� ��ų: �Ǹ� ���ְ�
    void Elite_DevilCommander()
    {
        Monster monster;
        //�ڽ��� ������ ���� �ݰ� �� ���� ����� �̼��� ����
        for (int i = BattleManager.instance.monsters.Count - 1; i >= 0; i--)
        {
            monster = BattleManager.instance.monsters[i];
            if (monster.isInBattle && Vector2.Distance(transform.position, monster.transform.position) < 3.0f && monster != this)
                monster.baseSPD = monster.baseMonster.baseSPD * 1.2f;
            else monster.baseSPD = monster.baseMonster.baseSPD;
        }
    }

    //����Ʈ ���� ��ų: ��ȭ��
    void Elite_Purifier()
    {
        immuneInterrupt = true;
    }

    //����Ʈ ���� ��ų: ������
    void Elite_Predator()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 5.0f)  //5�ʸ���
        {
            skillCoolTime1 = 0.0f;

            BattleManager.instance.ChangePlayerRP(-BattleManager.instance.rp * 0.2f); //�÷��̾� RP ����
        }
    }

    //���� ���� ��ų: ��������
    void Boss_Puppeteer()
    {
        Monster monster;
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)    //10�ʸ���
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            //������ ��ȯ�Ѵ�. �ʱ⿡�� ������ ������ �ָ� ����߷����ƾ� path�� �߰����� �۸�ġ ���� �ʴ´�. ������ ������ ���� �Ŀ��� �ڽ��� ���� ���ʿ��� ��ġ��Ų��.
            monster = GameManager.instance.GetMonsterFromPool(29);
            monster.gameObject.transform.position = new Vector2(100, 100);  //
            monster.InitializeMonster(cloneID--, FloorManager.instance.curRoom.pathID, 1.0f);
            monster.movedDistance = movedDistance + 1.0f;
            monster.gameObject.SetActive(true);
            BattleManager.instance.monsters.Add(monster);
        }

        //���� ����ִ� ������ �ִٸ� ������ �鿪�̴�.
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

    //���� ���� ��ų: �нż���
    void Boss_CloneMage()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)    //10�ʸ���
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            //HP�� 2/3���� �ϴ� �н� �� ���� ������.
            Monster monster;
            float scale = 0.5f * (FloorManager.instance.floor.floorNum + 1) * 2.0f;

            monster = GameManager.instance.GetMonsterFromPool(23);
            monster.gameObject.transform.position = new Vector2(100, 100);
            monster.InitializeMonster(cloneID--, FloorManager.instance.curRoom.pathID, scale);

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

    //���� ���� ��ų: ���μ���
    void Boss_Sealer()
    {
        if (skillUseTime != 0)  //��ų ������̶�� 5�� �Ŀ� ������ Ǭ��.
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
            if (skillCoolTime1 >= 10.0f)    //10�� ��Ÿ���� ��ٷ��� ������ �Ǵ�.
            {
                skillUseTime = 0.001f;
                anim.SetTrigger("Attack");

                int targetSealNum = DeckManager.instance.rings.Count / 2;   //��ü ���� ���ݸ�ŭ �����ϰ� ���ΰǴ�.
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

    //���� ���� ��ų: ���ּ���
    void Boss_DarkWarlock()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)    //10�ʸ���
        {
            int ringID;

            //�ٿ�׷��̵� �� �� �ִ� ���� �ִ��� Ȯ���Ѵ�.
            bool canDowngrade = false;
            for (int i = 0; i < DeckManager.instance.deck.Count; i++)
            {
                ringID = DeckManager.instance.deck[i];
                if (GameManager.instance.baseRings[ringID].level > 1)
                {
                    canDowngrade = true;
                    break;
                }
            }

            //�ٿ�׷��̵尡 �����ϴٸ�
            if (canDowngrade)
            {
                skillCoolTime1 = 0.0f;
                anim.SetTrigger("Attack");

                //������ �� �ϳ��� �ٿ�׷��̵��ϰ�, �� ���� ���� ������ ��(���׷��̵� ǥ�ø� �ٲٱ� ����), �ٿ�׷��̵� ������ ����Ѵ�(���� ���� �� 50�� Ȯ���� ���� �����ϱ� ����).
                do ringID = DeckManager.instance.deck[Random.Range(0, DeckManager.instance.deck.Count)];
                while (GameManager.instance.baseRings[ringID].level == 1);
                GameManager.instance.baseRings[ringID].Downgrade();
                UIManager.instance.OpenBattleDeckPanel();
                BattleManager.instance.ringDowngrade.Add(ringID);
            }
        }
    }

    //���� ���� ��ų: �����п���
    void Boss_Spacer()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)    //10�ʸ���
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            //��� �Ϲݸ��͵鳢�� �ڸ��� �ٲ۴�.
            Monster monster1, monster2;
            int monsterNum = BattleManager.instance.monsters.Count;
            float tmp;
            for (int i = monsterNum - 1; i >= 0; i--)
            {
                monster1 = BattleManager.instance.monsters[i];
                monster2 = BattleManager.instance.monsters[Random.Range(0, monsterNum)];
                if (monster1.baseMonster.tier != 'n' || monster2.baseMonster.tier != 'n') continue;
                tmp = monster1.movedDistance;
                monster1.movedDistance = monster2.movedDistance;
                monster2.movedDistance = tmp;
            }
        }
    }

    //���� ���� ��ų: ó����
    void Boss_Executer()
    {
        skillCoolTime1 += Time.deltaTime;
        if (skillCoolTime1 >= 10.0f)    //10�ʸ���
        {
            skillCoolTime1 = 0.0f;
            anim.SetTrigger("Attack");

            //������ ���� �����Ѵ�.
            DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.rings[Random.Range(0, DeckManager.instance.rings.Count)]);
        }
    }

    //���� ���� ��ų: Ÿ���� ��
    void Boss_King()
    {
        if (skillUseTime != 0)  //��ų(����) ������̸�
        {
            skillUseTime += Time.deltaTime;
            if (skillUseTime > 5.0f)    //������ ����Ѵ�.
            {
                skillUseTime = 0.0f;
                skillCoolTime1 = 0.0f;
                for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) DeckManager.instance.rings[i].isSealed = false;
            }
        }
        else 
        {
            skillCoolTime1 += Time.deltaTime;
            if (skillCoolTime1 >= 10.0f) //10�ʸ��� ������ ��ų�� ����Ѵ�.
            {
                skillCoolTime1 = 0.0f;
                anim.SetTrigger("Attack");

                bool canDowngrade = false;
                do
                {
                    switch (Random.Range(0, 3)) //���� �� �� �ϳ��� ������ ������ �����Ѵ�.
                    {
                        case 0: //�� �ϳ��� �����ϰ� �����Ѵ�.
                            DeckManager.instance.RemoveRingFromBattle(DeckManager.instance.rings[Random.Range(0, DeckManager.instance.rings.Count)]);
                            canDowngrade = true;
                            break;
                        case 1: //�� �ϳ��� �����ϰ� �ٿ�׷��̵��Ѵ�.
                            int ringID;
                            for (int i = 0; i < DeckManager.instance.deck.Count; i++)
                            {
                                ringID = DeckManager.instance.deck[i];
                                if (GameManager.instance.baseRings[ringID].level > 1)
                                {
                                    canDowngrade = true;
                                    break;
                                }
                            }
                            if (canDowngrade)
                            {
                                skillCoolTime1 = 0.0f;
                                do ringID = DeckManager.instance.deck[Random.Range(0, DeckManager.instance.deck.Count)];
                                while (GameManager.instance.baseRings[ringID].level == 1);
                                GameManager.instance.baseRings[ringID].Downgrade();
                                UIManager.instance.OpenBattleDeckPanel();
                                BattleManager.instance.ringDowngrade.Add(ringID);
                            }
                            break;
                        case 2: //�� ������ �����Ѵ�.
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
        if (skillCoolTime2 >= 5.0f)     //5�ʸ��� �Ϲ� ��ų�� ����Ѵ�.
        {
            skillCoolTime2 = 0.0f;
            anim.SetTrigger("Attack");

            switch (Random.Range(0, 2))
            {
                case 0: //�̵� ���� ȿ���� �鿪�̴�.
                    immuneInterrupt = true;
                    break;
                case 1: //���� HP�� �Ϻθ� ȸ���Ѵ�.
                    immuneInterrupt = false;
                    AE_DecreaseHP(-(maxHP - curHP) * 0.05f, Color.green);
                    break;
            }
        }
    }

    //���͸� ������Ʈ Ǯ�� �ǵ��� �� Invoke�� ������ ��� ���δ�.
    void InvokeReturnMonsterToPool()
    {
        GameManager.instance.ReturnMonsterToPool(this);
    }
}
