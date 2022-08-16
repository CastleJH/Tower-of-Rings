using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public SPUM_Prefabs[] spum_prefabs;

    //������
    public GameObject ringPrefab;
    public GameObject[] monsterPrefabs;
    public GameObject[] bulletPrefabs;
    public GameObject[] particlePrefabs;
    public GameObject barrierPrefab;
    public GameObject blizzardPrefab;
    public GameObject amplifierPrefab;
    public GameObject damageTextPrefab;
    public GameObject dropRPPrefab;
    public GameObject itemPrefab;

    //��������Ʈ
    public Sprite[] itemSprites;
    public Sprite[] ringSprites;
    public Sprite[] relicSprites;
    public Sprite[] sceneRoomSprites;
    public Sprite[] mapRoomSprites;
    public Sprite[] ringUpgradeSprites;
    public Sprite[] ringInfoUpgradeSprites;
    public Sprite[] buttonSprites;
    public Sprite[] speedSprites;
    public Sprite emptyRingSprite; 

    //����
    public AudioClip[] ringAttackAudios;

    //���� �̵� ���
    public PathCreator[] monsterPaths;
    public SpriteRenderer[] monsterPathImages;

    //DB
    [HideInInspector]
    public List<BaseRing> ringDB;
    [HideInInspector]
    public List<BaseMonster> monsterDB;
    [HideInInspector]
    public List<BaseRelic> relicDB;

    //������Ʈ Ǯ
    private Queue<Ring> ringPool;
    private Queue<Monster>[] monsterPool;
    private Queue<Bullet>[] bulletPool;
    private Queue<ParticleChecker>[] particlePool;
    private Queue<Barrier> barrierPool;
    private Queue<Blizzard> blizzardPool;
    private Queue<Amplifier> amplifierPool;
    private Queue<DamageText> damageTextPool;
    private Queue<DropRP> dropRPPool;
    private Queue<Item> itemPool;

    //���� �����Ȳ ���� ����
    public int playerMaxHP;
    public int playerCurHP;
    public int gold;
    public int diamond;
    public List<int> relics;
    public List<int> cursedRelics;

    void Awake()
    {
        instance = this;

        //DB�б�
        ReadDB();
        if (monsterDB.Count != monsterPrefabs.Length) Debug.LogError("num of monster sprites does not match");
        if (ringDB.Count != ringSprites.Length) Debug.LogError("num of ring sprites does not match");
        if (ringDB.Count != ringAttackAudios.Length) Debug.LogError("num of audios does not match");
        if (relicDB.Count != relicSprites.Length) Debug.LogError("num of relic sprites does not match");

        //������Ʈ Ǯ �ʱ�ȭ
        ringPool = new Queue<Ring>();
        monsterPool = new Queue<Monster>[monsterDB.Count];
        bulletPool = new Queue<Bullet>[ringDB.Count];
        particlePool = new Queue<ParticleChecker>[ringDB.Count];
        barrierPool = new Queue<Barrier>();
        blizzardPool = new Queue<Blizzard>();
        amplifierPool = new Queue<Amplifier>();
        damageTextPool = new Queue<DamageText>();
        dropRPPool = new Queue<DropRP>();
        itemPool = new Queue<Item>();

        for (int i = 0; i < monsterDB.Count; i++)
        {
            monsterPool[i] = new Queue<Monster>();
        }

        for (int i = 0; i < ringDB.Count; i++)
        {
            bulletPool[i] = new Queue<Bullet>();
            particlePool[i] = new Queue<ParticleChecker>();
        }
    }

    public void TowerStart()
    {
        InitializeGame();
        FloorManager.instance.CreateAndMoveToFloor(1);
    }

    public void InitializeGame()
    {
        playerMaxHP = 100;
        playerCurHP = 100;
        ChangePlayerCurHP(0);
        gold = 0;
        ChangeGold(0);
        diamond = 0;
        ChangeDiamond(0);
        DeckManager.instance.InitializeDeck();
    }

    //"*_db.csv"�� �о�´�.
    void ReadDB()
    {
        List<Dictionary<string, object>> dataRing = DBReader.Read("ring_db");
        ringDB = new List<BaseRing>();
        for (int i = 0; i < dataRing.Count; i++)
        {
            BaseRing r = new BaseRing();
            r.id = (int)dataRing[i]["id"];
            r.rarity = (int)dataRing[i]["rarity"];
            r.name = (string)dataRing[i]["name"];
            r.maxlvl = (int)dataRing[i]["maxlvl"];
            r.dbATK = (int)dataRing[i]["atk"];
            r.dbSPD = float.Parse(dataRing[i]["spd"].ToString());
            r.baseNumTarget = (int)dataRing[i]["target"];
            r.dbRP = (int)dataRing[i]["rp"];
            r.baseRP = r.dbRP;
            r.baseEFF = float.Parse(dataRing[i]["eff"].ToString());
            r.description = (string)dataRing[i]["description"];
            r.range = (int)dataRing[i]["range"];
            r.toSame = (string)dataRing[i]["identical"];
            r.toAll = (string)dataRing[i]["all"];
            r.level = 0;
            r.Upgrade();
            ringDB.Add(r);
        }

        List<Dictionary<string, object>> dataMonster = DBReader.Read("monster_db");
        monsterDB = new List<BaseMonster>();
        for (int i = 0; i < dataMonster.Count; i++)
        {
            BaseMonster m = new BaseMonster();
            m.type = (int)dataMonster[i]["type"];
            m.name = (string)dataMonster[i]["name"];
            m.hp = (int)dataMonster[i]["hp"];
            m.spd = float.Parse(dataMonster[i]["spd"].ToString());
            m.description = (string)dataMonster[i]["description"];
            m.atk = (int)dataMonster[i]["atk"];
            monsterDB.Add(m);
        }

        List<Dictionary<string, object>> dataRelic = DBReader.Read("relic_db");
        relicDB = new List<BaseRelic>();
        for (int i = 0; i < dataRelic.Count; i++)
        {
            BaseRelic r = new BaseRelic();
            r.id = (int)dataRelic[i]["id"];
            r.name = (string)dataRelic[i]["name"];
            r.have = false;
            r.isCursed = false;
            r.pureDescription = (string)dataRelic[i]["effect"];
            r.cursedDescription = (string)dataRelic[i]["effect_cursed"];
            relicDB.Add(r);
        }
    }

    //���͸� ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public Monster GetMonsterFromPool(int id)
    {
        if (monsterPool[id].Count > 0) return monsterPool[id].Dequeue();
        else
        {
            Monster monster = Instantiate(monsterPrefabs[id]).GetComponent<Monster>();
            monster.baseMonster = monsterDB[id];
            return monster;
        }
    }


    //���͸� ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnMonsterToPool(Monster monster)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (monsterPool[monster.baseMonster.type].Contains(monster))
        {
            Debug.LogError("already enqueued monster");
            return;
        }
        monster.gameObject.SetActive(false);
        monsterPool[monster.baseMonster.type].Enqueue(monster);
    }

    //���� ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public Ring GetRingFromPool()
    {
        if (ringPool.Count > 0) return ringPool.Dequeue();
        else return Instantiate(ringPrefab).GetComponent<Ring>();
    }


    //���� ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnRingToPool(Ring ring)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (ringPool.Contains(ring))
        {
            Debug.LogError("already enqueued ring");
            return;
        }
        ring.baseRing = null;
        ring.isInBattle = false;
        ring.gameObject.SetActive(false);
        ringPool.Enqueue(ring);
    }

    //�ҷ��� ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public Bullet GetBulletFromPool(int id)
    {
        if (bulletPool[id].Count > 0) return bulletPool[id].Dequeue();
        else
        {
            Bullet bullet = Instantiate(bulletPrefabs[id]).GetComponent<Bullet>();
            bullet.destroyID = id;
            return bullet;
        }
    }

    //�ҷ��� ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnBulletToPool(Bullet bullet)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (bulletPool[bullet.destroyID].Contains(bullet))
        {
            Debug.LogError("already enqueued bullet");
            return;
        }
        bullet.gameObject.SetActive(false);
        bulletPool[bullet.destroyID].Enqueue(bullet);
    }

    //�ҷ� Ǯ�� �ִ� ��� ������Ʈ�� �����Ѵ�.
    public void EmptyBulletPool(int id)
    {
        while (bulletPool[id].Count > 0) Destroy(bulletPool[id].Dequeue().gameObject);
    }

    //��ƼŬ�� ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public ParticleChecker GetParticleFromPool(int id)
    {
        if (particlePool[id].Count > 0) return particlePool[id].Dequeue();
        else return Instantiate(particlePrefabs[id]).GetComponent<ParticleChecker>();
    }

    //��ƼŬ�� ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnParticleToPool(ParticleChecker particle, int id)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (particlePool[id].Contains(particle))
        {
            Debug.LogError("already enqueued particle");
            return;
        }
        particle.gameObject.SetActive(false);
        particlePool[id].Enqueue(particle);
    }

    //��ƼŬ Ǯ�� �ִ� ��� ������Ʈ�� �����Ѵ�.
    public void EmptyParticlePool(int id)
    {
        while (particlePool[id].Count > 0) Destroy(particlePool[id].Dequeue().gameObject);
    }

    //��踦 ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public Barrier GetBarrierFromPool()
    {
        if (barrierPool.Count > 0) return barrierPool.Dequeue();
        else return Instantiate(barrierPrefab).GetComponent<Barrier>();
    }

    //��踦 ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnBarrierToPool(Barrier barrier)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (barrierPool.Contains(barrier))
        {
            Debug.LogError("already enqueued barrier");
            return;
        }
        barrier.gameObject.SetActive(false);
        barrierPool.Enqueue(barrier);
    }

    //������ ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public Blizzard GetBlizzardFromPool()
    {
        if (blizzardPool.Count > 0) return blizzardPool.Dequeue();
        else return Instantiate(blizzardPrefab).GetComponent<Blizzard>();
    }


    //������ ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnBlizzardToPool(Blizzard blizzard)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (blizzardPool.Contains(blizzard))
        {
            Debug.LogError("already enqueued blizzard");
            return;
        }
        blizzard.gameObject.SetActive(false);
        blizzardPool.Enqueue(blizzard);
    }

    //������ ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public Amplifier GetAmplifierFromPool()
    {
        if (amplifierPool.Count > 0) return amplifierPool.Dequeue();
        else return Instantiate(amplifierPrefab).GetComponent<Amplifier>();
    }


    //������ ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnAmplifierToPool(Amplifier amplifier)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (amplifierPool.Contains(amplifier))
        {
            Debug.LogError("already enqueued amplifier");
            return;
        }
        amplifier.gameObject.SetActive(false);
        amplifierPool.Enqueue(amplifier);
    }

    //������ ǥ�ñ⸦ ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public DamageText GetDamageTextFromPool()
    {
        if (damageTextPool.Count > 0) return damageTextPool.Dequeue();
        else return Instantiate(damageTextPrefab).GetComponent<DamageText>();
    }

    //������ ǥ�ñ⸦ ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnDamageTextToPool(DamageText damageText)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (damageTextPool.Contains(damageText))
        {
            Debug.LogError("already enqueued damageText");
            return;
        }
        damageText.gameObject.SetActive(false);
        damageTextPool.Enqueue(damageText);
    }

    //���� ������ ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public DropRP GetDropRPFromPool()
    {
        if (dropRPPool.Count > 0) return dropRPPool.Dequeue();
        else return Instantiate(dropRPPrefab).GetComponent<DropRP>();
    }


    //���� ������ ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnDropRPToPool(DropRP dropRP)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (dropRPPool.Contains(dropRP))
        {
            Debug.LogError("already enqueued dropRP");
            return;
        }
        dropRP.gameObject.SetActive(false);
        dropRPPool.Enqueue(dropRP);
    }

    //�������� ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public Item GetItemFromPool()
    {
        if (itemPool.Count > 0) return itemPool.Dequeue();
        else return Instantiate(itemPrefab).GetComponent<Item>();
    }


    //�������� ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnItemToPool(Item item)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (itemPool.Contains(item))
        {
            Debug.LogError("already enqueued item");
            return;
        }
        item.gameObject.SetActive(false);
        itemPool.Enqueue(item);
    }

    //�÷��̾��� ���� HP�� �ٲ۴�. false�� ��ȯ�ϸ� HP ��ȭ�� ���� ����̴�. �ٲ� ���� HP�� 0�̶�� ���ӿ����� �Ѿ��. 
    public bool ChangePlayerCurHP(int _HP)
    {
        if ((playerCurHP == playerMaxHP) && (_HP > 0)) return false;
        playerCurHP = Mathf.Clamp(playerCurHP + _HP, 0, playerMaxHP);
        if (playerCurHP <= playerMaxHP * 0.2f) UIManager.instance.playerHPText.color = Color.red;
        else UIManager.instance.playerHPText.color = Color.white;
        UIManager.instance.playerHPText.text = playerCurHP.ToString() + "/" + playerMaxHP.ToString();
        if (playerCurHP == 0)
        {
            Time.timeScale = 0.0f;
            Debug.Log("GAME OVER!!!");
        }
        if (_HP == 0) return false;
        return true;
    }

    //��带 �ٲ۴�.
    public bool ChangeGold(int _gold)
    {
        if (gold + _gold < 0) return false;
        gold += _gold;
        UIManager.instance.playerGoldText.text = gold.ToString();
        return true;
    }
    
    //���̾Ƹ�带 �ٲ۴�.
    public bool ChangeDiamond(int _diamond)
    {
        if (diamond + _diamond < 0) return false;
        diamond += _diamond;
        UIManager.instance.playerDiamondText.text = diamond.ToString();
        return true;
    }

    public bool AddRelicToDeck(int id)
    {
        if (relics.Contains(id)) return false;
        relics.Add(id);
        relicDB[id].have = true;
        return true;
    }
}
