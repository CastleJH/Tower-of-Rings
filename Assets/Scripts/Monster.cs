using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public bool noBarrierBlock; //���κ��� �̵��� ���ع��� �ʴ��� ����

    //�׷���
    SpriteRenderer spriteRenderer;  //���� �̹���
    public TextMesh hpText;   //HP �ؽ�Ʈ
    public List<ParticleSystem> particles; //������ �ǰ� ��ƼŬ
    public List<int> particlesID;   //������ �ǰ� ��ƼŬ ����

    //��Ÿ ����
    float snowTime;         //������ ���ο� ȿ���� ���ӵ� �ð�
    float snowEndTime;      //������ ���ο� ȿ���� ������ �ð�

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (curHP > 0)  //����ִ� ��쿡�� ������ �� ����.
        {
            curSPD = baseSPD;

            spriteRenderer.color = Color.white; //����(���� �̻� ǥ�� ��)�� �ʱ�ȭ

            //�̵����ش� ���Ѽ�->���Ѽ����� ����Ǿ�� �Ѵ�.
            if (snowTime < snowEndTime) //���� ���� ��ȭ�� �������̶��
            {
                curSPD = baseSPD * 0.5f;
                snowTime += Time.deltaTime;
                spriteRenderer.color = Color.cyan;
            }

            if (noBarrierBlock) //��迡 ������ ������ �̵�
            {
                movedDistance += curSPD * Time.deltaTime;
                transform.position = path.path.GetPointAtDistance(movedDistance);
                transform.Translate(0.0f, 0.0f, id * 0.001f);
            }
        }
    }

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
        noBarrierBlock = true;

        //�׷���
        spriteRenderer.sprite = GameManager.instance.monsterSprites[baseMonster.type];
        spriteRenderer.color = Color.white; //����(���� �̻� ǥ�� ��)�� �ʱ�ȭ
        SetHPText();
        InvokeParticleOff();

        //��Ÿ ����
        snowEndTime = -1.0f;
    }

    //��ƼŬ�� �÷����Ѵ�.
    public void PlayParticleCollision(int id, float time)
    {
        //�ǰ� ��ƼŬ�� ������ �����Ѵ�.
        ParticleSystem par = GameManager.instance.GetParticleFromPool(id);

        //���� �ð��� �����ؾ� �Ѵٸ� �����Ѵ�.
        if (time != 0.0f)
        {
            ParticleSystem[] pars = par.GetComponentsInChildren<ParticleSystem>();
            par.Stop(true);

            for (int i = 0; i < pars.Length; i++)
            {
                var main = pars[i].main;
                main.duration = time;
            }
        }

        //�����Ѵ�.
        particles.Add(par);
        particlesID.Add(id);

        //��ġ�� �����Ѵ�.
        par.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.1f);
        par.gameObject.transform.parent = gameObject.transform;

        //�÷����Ѵ�.
        par.gameObject.SetActive(true);
        par.Play();
    }

    //�����ؾ� �ϴ� ��ƼŬ���� Ȯ���ϰ� �����Ѵ�.
    public void InvokeParticleOff()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            if (!particles[i].IsAlive(true) || !BattleManager.instance.isBattlePlaying)   //��ƼŬ ����� �����ų� ������ ���� ���
            {
                //����� �ѹ� �� ���߰� �����Ѵ�(���� ���ῡ ���� �����ؾ� �ϴ� ���� �� �����Ƿ�).
                particles[i].Stop(true);
                GameManager.instance.ReturnParticleToPool(particles[i], particlesID[i]);
                particles.RemoveAt(i);
                particlesID.RemoveAt(i);
            }
        }

        if (BattleManager.instance.isBattlePlaying && curHP > 0)  //������ ���� �������̰� �ڽ��� ���� HP�� �����ִٸ� ������ �� �Լ��� �� �θ���.
            Invoke("InvokeParticleOff", 1f);
    }

    //HP �ؽ�Ʈ�� �����Ѵ�.
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
            RemoveFromScene(0.5f);
        }
    }

    //���ӿ��� time �� �� �����Ѵ�.
    public void RemoveFromScene(float time)
    {
        if (time == 0.0f) InvokeReturnMonsterToPool();
        else Invoke("InvokeReturnMonsterToPool", time);
    }

    //������ ���ӿ��� �����Ѵ�. ���� ���� ���� ��� ȿ���� �����Ѵ�. ���/RP�� ������Ų��.
    void InvokeReturnMonsterToPool()
    {
        if (!gameObject.activeSelf) return;

        BattleManager.instance.monsters.Remove(this);
        //BattleManager.instance.totalGetGold += 10;
        //BattleManager.instance.totalKilledMonster++;
        //BattleManager.instance.rp += BattleManager.instance.genRP;
        //UIManager.instance.SetIngameTotalRPText();

        //���� ���� ����� ���� ȿ���� �����Ѵ�.
        //if (curseStack != 0) AE_CurseDead();
        //if (growRings.Count > 0) AE_GrowDead();
        //if (DeckManager.instance.isNecro) DeckManager.instance.necroCnt++;

        //��ƼŬ�� ��� �����Ѵ�.
        CancelInvoke();
        InvokeParticleOff();

        //HP�� 0���� �ٲ۴�.
        curHP = 0;

        GameManager.instance.ReturnMonsterToPool(this);
    }

    //���� ����Ʈ: HP����
    public void AE_DecreaseHP(float dmg, Color32 color)
    {
        if (dmg != -1)
        {
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
    public void AE_Snow(float time)
    {
        //�̹� �ް��ִ� ���� ���� ��ȭ ȿ���� �� ���� ���ٸ� ���� ���
        if (snowEndTime - snowTime > time) return;
        snowEndTime = time;
        snowTime = 0;
    }
}
