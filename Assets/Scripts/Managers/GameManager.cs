using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool debugFlag;
    public int debugInt;

    //������
    public GameObject ringPrefab;
    public SPUM_Prefabs[] spum_prefabs;
    public GameObject[] monsterPrefabs;
    public GameObject[] monsterParticlePrefabs;
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
    public AudioSource audioSource;
    public AudioClip[] bgms;
    public AudioClip[] ringAttackAudios;
    public AudioClip[] specialAudios;   //��ư �ظ� �� ������ ��Ż
    
    //���� �̵� ��� & �� ����
    public PathCreator[] monsterPaths;
    public SpriteRenderer[] monsterPathImages;
    public GameObject sinkholeNorth;
    public GameObject sinkholeSouth;

    //DB
    [HideInInspector]
    public List<BaseRing> baseRings;
    [HideInInspector]
    public List<BaseMonster> baseMonsters;
    [HideInInspector]
    public List<BaseRelic> baseRelics;

    //������Ʈ Ǯ
    private Queue<Ring> ringPool;
    private Queue<Monster>[] monsterPool;
    private Queue<ParticleChecker>[] monsterParticlePool;
    private Queue<Bullet>[] bulletPool;
    private Queue<ParticleChecker>[] particlePool;
    private Queue<Barrier> barrierPool;
    private Queue<Blizzard> blizzardPool;
    private Queue<Amplifier> amplifierPool;
    private Queue<DamageText> damageTextPool;
    private Queue<DropRP> dropRPPool;
    private Queue<Item> itemPool;

    //���� ���� ���� ���� & �÷��̾� ���� �������(�Ϻδ� GPGS�� �����)
    public int diamond;                         //GPGS ����
    public int hardModeOpen;                    //GPGS ����
    public int[] spiritMaxLevel;
    public float[] spiritBaseEnhanceCost;
    public int[] spiritEnhanceLevel;            //GPGS ����
    public int[] ringCollecionRewardAmount;
    public int[,] ringCollectionMaxProgress;
    public int[,] ringCollectionProgress;       //GPGS ����(-1�̸� �̹� ������ ȹ���Ͽ��ٴ� ����)
    public int[] relicCollecionRewardAmount;
    public int[,] relicCollectionMaxProgress;
    public int[,] relicCollectionProgress;      //GPGS ����(-1�̸� �̹� ������ ȹ���Ͽ��ٴ� ����)
    public int[] monsterCollectionMaxProgress;  
    public int[] monsterCollectionProgress;     //GPGS ����(-1�̸� �̹� ������ ȹ���Ͽ��ٴ� ����)

    //���� ���� ���� ����
    public bool isNormalMode;
    public int playerMaxHP;
    public int playerCurHP;
    public int gold;
    public List<int> relics;
    public List<int> cursedRelics;
    public bool revivable;
    public bool saveFloor;


    void Awake()
    {
        instance = this;

        //DB�б�
        ReadDB();
        if (baseMonsters.Count != monsterPrefabs.Length) Debug.LogError("num of monster sprites does not match");
        if (baseRings.Count != ringSprites.Length) Debug.LogError("num of ring sprites does not match");
        if (baseRings.Count != ringAttackAudios.Length) Debug.LogError("num of audios does not match");
        if (baseRelics.Count != relicSprites.Length) Debug.LogError("num of relic sprites does not match");

        //������Ʈ Ǯ �ʱ�ȭ
        ringPool = new Queue<Ring>();
        monsterPool = new Queue<Monster>[baseMonsters.Count];
        monsterParticlePool = new Queue<ParticleChecker>[baseMonsters.Count];
        bulletPool = new Queue<Bullet>[baseRings.Count];
        particlePool = new Queue<ParticleChecker>[baseRings.Count];
        barrierPool = new Queue<Barrier>();
        blizzardPool = new Queue<Blizzard>();
        amplifierPool = new Queue<Amplifier>();
        damageTextPool = new Queue<DamageText>();
        dropRPPool = new Queue<DropRP>();
        itemPool = new Queue<Item>();

        for (int i = 0; i < baseMonsters.Count; i++)
        {
            monsterPool[i] = new Queue<Monster>();
            monsterParticlePool[i] = new Queue<ParticleChecker>();
        }

        for (int i = 0; i < baseRings.Count; i++)
        {
            bulletPool[i] = new Queue<Bullet>();
            particlePool[i] = new Queue<ParticleChecker>();
        }

        //��ȥ ��ȭ&�ݷ��� ���
        spiritMaxLevel = new int[11] { 10, 10, 5, 5, 5, 5, 5, 5, 1, 1, 1 };
        spiritBaseEnhanceCost = new float[11] { 3.0f, 3.0f, 10.0f, 10.0f, 10.0f, 5.0f, 20.0f, 20.0f, 50.0f, 70.0f, 70.0f };
        ringCollectionMaxProgress = new int[baseRings.Count, 5];
        for (int i = 0; i < baseRings.Count; i++)
        {
            ringCollectionMaxProgress[i, 0] = 1;
            ringCollectionMaxProgress[i, 1] = 10;
            ringCollectionMaxProgress[i, 2] = 5;
            ringCollectionMaxProgress[i, 3] = 3;
            ringCollectionMaxProgress[i, 4] = 1;
        }
        ringCollecionRewardAmount = new int[5] { 1, 2, 3, 3, 3 };
        relicCollectionMaxProgress = new int[baseRelics.Count, 5];
        for (int i = 0; i < baseRelics.Count; i++)
        {
            relicCollectionMaxProgress[i, 0] = 1;
            relicCollectionMaxProgress[i, 1] = 10;
            relicCollectionMaxProgress[i, 2] = 5;
            relicCollectionMaxProgress[i, 3] = 3;
            relicCollectionMaxProgress[i, 4] = 1;
        }
        relicCollecionRewardAmount = new int[5] { 1, 2, 2, 3, 3 };
        monsterCollectionMaxProgress = new int[baseMonsters.Count];
        for (int i = 0; i < baseMonsters.Count; i++)
        {
            switch (baseMonsters[i].tier)
            {
                case 'n':
                    monsterCollectionMaxProgress[i] = 200;
                    break;
                case 'e':
                    monsterCollectionMaxProgress[i] = 10;
                    break;
                case 'b':
                    monsterCollectionMaxProgress[i] = 1;
                    break;
            }
        }


        if (!GPGSManager.instance.useGPGS)
        {
            InitializeUserData();
            UIManager.instance.gameStartPanelSignInButton.SetActive(false);
            UIManager.instance.gameStartPanelMoveToGameButton.SetActive(false);
            UIManager.instance.gameStartPanelMoveToLobbyButton.SetActive(true);
        }
    }

    void Update()
    {
        if (debugFlag)
        {
            debugFlag = false;
            UIManager.instance.gameStartPanel.SetActive(false);
            BattleManager.instance.StopBattleSystem();
            GameStart();
            DeckManager.instance.AddRingToDeck(debugInt, true);
        }
    }

    public void InitializeUserData()
    {
        //��ȥ ��ȭ ���� �ʱ�ȭ
        spiritEnhanceLevel = new int[11] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        ringCollectionProgress = new int[baseRings.Count, 5];
        for (int i = 0; i < baseRings.Count; i++)
            for (int j = 0; j < 5; j++)
                ringCollectionProgress[i, j] = 0;
        relicCollectionProgress = new int[baseRelics.Count, 5];
        for (int i = 0; i < baseRelics.Count; i++)
            for (int j = 0; j < 5; j++)
                relicCollectionProgress[i, j] = 0;
        monsterCollectionProgress = new int[baseMonsters.Count];
        for (int i = 0; i < baseMonsters.Count; i++)
            monsterCollectionProgress[i] = 0;
        gold = 0;
        diamond = 0;
        saveFloor = false;
    }

    //������ �����Ѵ�.
    public void GameStart()
    {
        saveFloor = true;
        InitializeGame();
        UIManager.instance.mapPanel.SetActive(true);
        FloorManager.instance.endPortal.SetActive(false);
        FloorManager.instance.CreateAndMoveToFloor(1);
    }

    //����� ������ �����Ѵ�(1���� �ƴ� ������ ����).
    public void GameStartSaved()
    {
        saveFloor = false;
        InitializeGameSaved();
        UIManager.instance.mapPanel.SetActive(true);
        FloorManager.instance.endPortal.SetActive(false);
        FloorManager.instance.CreateAndMoveToFloor(FloorManager.instance.floor.floorNum);
    }

    //������ �ʱ�ȭ�Ѵ�.
    void InitializeGame()
    {
        isNormalMode = !UIManager.instance.lobbyHardModeToggleButton.isOn;
        Time.timeScale = 1.0f;
        
        //���� ������ ���� ���� ����/��/���� ���� ���� �ʱ�� �ǵ�����(DB���� ��ġ�ϵ���).
        ResetBases(isNormalMode);

        //������ ��� ����.
        relics.Clear();
        cursedRelics.Clear();

        //�÷��̾� ���� HP���� ���Ѵ�.
        playerMaxHP = 100 + spiritEnhanceLevel[2] * 4;
        if (!isNormalMode) playerMaxHP /= 2;
        playerCurHP = playerMaxHP;
        ChangePlayerCurHP(0);   //���⼭ ��ȥ��ȭ�� ���� �� �⺻ ���ݷ�/���� ��Ÿ�ӵ� ���Ѵ�.

        //�÷��̾� ���� ��差�� ���Ѵ�. ���̾Ƹ��� ���� ���Ӱ� �������̹Ƿ� �ٲ��� �ʴ´�.
        gold = spiritEnhanceLevel[4] * 2 ;
        ChangeGold(0);
        ChangeDiamond(0);

        //��Ȱ ���� ���θ� �ʱ�ȭ�Ѵ�.
        revivable = false;

        //���� �ʱ�ȭ�Ѵ�.
        DeckManager.instance.InitializeDeck();
    }

    void InitializeGameSaved()
    {
        Time.timeScale = 1.0f;

        ChangePlayerCurHP(0);   //���⼭ ��ȥ��ȭ�� ���� �� �⺻ ���ݷ�/���� ��Ÿ�ӵ� ���Ѵ�.

        ChangeGold(0);
        ChangeDiamond(0);
    }

    //�������̴� ������ ���ӿ��� ó���Ѵ�(HP�� 0�� �Ǿ��ų�, �÷��̾ �޴����� ���⸦ �����ų�).
    public void OnGameOver(int a, int b)
    {
        UIManager.instance.OpenEndingPanel(0);  //���ӿ��� �̹����� �г��� ����.
        BattleManager.instance.StopBattleSystem();     //��Ʋ�ý����� ��� �����Ѵ�.
        Time.timeScale = 1;         //�ӵ��� ������� ������.
    }

    //�������̴� ������ Ŭ���� ó���Ѵ�(7���� ������ óġ���� ��).
    public void OnGameClear(int a, int b)
    {
        UIManager.instance.OpenEndingPanel(1);  //����Ŭ���� �̹����� �г��� ����.
        UIManager.instance.lobbyHardModeToggleButton.gameObject.SetActive(true);
        hardModeOpen = 1;
        BattleManager.instance.StopBattleSystem();     //��Ʋ�ý����� ��� �����Ѵ�.
        Time.timeScale = 1;         //�ӵ��� ������� ������.
    }

    //���� ������ ���� ���� ����/��/���� ���� ���� DB���� ��ġ��Ų��(�ٸ�, �ϵ���� ���� HP�� 2�谡 �� �� �ִ�).
    public void ResetBases(bool isNormal)
    {
        int mult = isNormal ? 1 : 2;    //�ϵ���� ���� HP�� �ι�� ���ش�.
        for (int i = 0; i < baseRings.Count; i++) baseRings[i].Init();
        for (int i = 0; i < baseRelics.Count; i++) baseRelics[i].Init();
        for (int i = 0; i < baseMonsters.Count; i++) baseMonsters[i].Init(mult);

    }

    //"*_db.csv"�� �о�´�.
    void ReadDB()
    {
        List<Dictionary<string, object>> csvRing = DBReader.Read("ring_db");
        baseRings = new List<BaseRing>();
        for (int i = 0; i < csvRing.Count; i++)
        {
            BaseRing r = new BaseRing();
            r.id = (int)csvRing[i]["id"];
            r.name = (string)csvRing[i]["name"];
            r.maxlvl = (int)csvRing[i]["maxlvl"];
            r.csvATK = (int)csvRing[i]["atk"];
            r.csvSPD = float.Parse(csvRing[i]["spd"].ToString());
            r.csvNumTarget = (int)csvRing[i]["target"];
            r.csvRange = (int)csvRing[i]["range"];
            r.csvRP = (int)csvRing[i]["rp"];
            r.csvEFF = float.Parse(csvRing[i]["eff"].ToString());
            r.description = (string)csvRing[i]["description"];
            r.toSame = (string)csvRing[i]["identical"];
            r.toAll = (string)csvRing[i]["all"];

            baseRings.Add(r);
        }

        List<Dictionary<string, object>> csvMonster = DBReader.Read("monster_db");
        baseMonsters = new List<BaseMonster>();
        for (int i = 0; i < csvMonster.Count; i++)
        {
            BaseMonster m = new BaseMonster();
            m.type = (int)csvMonster[i]["type"];
            m.name = (string)csvMonster[i]["name"];
            m.csvATK = (int)csvMonster[i]["atk"];
            m.csvHP = (int)csvMonster[i]["hp"];
            m.csvSPD = float.Parse(csvMonster[i]["spd"].ToString());
            m.description = (string)csvMonster[i]["description"];
            m.tier = ((string)csvMonster[i]["tier"])[0];
            baseMonsters.Add(m);
        }

        List<Dictionary<string, object>> csvRelic = DBReader.Read("relic_db");
        baseRelics = new List<BaseRelic>();
        for (int i = 0; i < csvRelic.Count; i++)
        {
            BaseRelic r = new BaseRelic();
            r.id = (int)csvRelic[i]["id"];
            r.name = (string)csvRelic[i]["name"];
            r.have = false;
            r.isPure = true;
            r.pureDescription = (string)csvRelic[i]["effect"];
            r.cursedDescription = (string)csvRelic[i]["effect_cursed"];
            baseRelics.Add(r);
        }
    }

    //���͸� ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public Monster GetMonsterFromPool(int id)
    {
        if (monsterPool[id].Count > 0) return monsterPool[id].Dequeue();
        else
        {
            Monster monster = Instantiate(monsterPrefabs[id]).GetComponent<Monster>();
            monster.baseMonster = baseMonsters[id];
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

    //���� ��ƼŬ�� ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public ParticleChecker GetMonsterParticleFromPool(int id)
    {
        if (monsterParticlePool[id].Count > 0) return monsterParticlePool[id].Dequeue();
        else
        {
            ParticleChecker ret = Instantiate(monsterParticlePrefabs[id]).GetComponent<ParticleChecker>();
            ret.gameObject.SetActive(false);
            return ret;
        }
    }

    //���� ��ƼŬ�� ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnMonsterParticleToPool(ParticleChecker particle, int id)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (monsterParticlePool[id].Contains(particle))
        {
            Debug.LogError("already enqueued monster particle");
            return;
        }
        particle.gameObject.SetActive(false);
        monsterParticlePool[id].Enqueue(particle);
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

    //�÷��̾��� ���� HP�� �ٲ۴�. false�� ��ȯ�ϸ� �ٲ� HP�� ���� ����̴�. �ٲ� ���� HP�� 0�̶�� ���ӿ����� �Ѿ��. 
    public bool ChangePlayerCurHP(int _HP)
    {
        if ((playerCurHP == playerMaxHP) && (_HP > 0)) return false;
        playerCurHP = Mathf.Clamp(playerCurHP + _HP, 0, playerMaxHP);
        if (playerCurHP <= playerMaxHP * 0.2f) UIManager.instance.playerHPText.color = Color.red;
        else UIManager.instance.playerHPText.color = Color.white;
        UIManager.instance.playerHPText.text = playerCurHP.ToString() + "/" + playerMaxHP.ToString();
        if (playerCurHP == 0 && !UIManager.instance.gameStartPanel.activeSelf)
        {
            if (baseRelics[4].have && baseRelics[4].isPure && revivable)
            {
                revivable = false;
                ChangePlayerCurHP(10);
            }
            else
            {
                BattleManager.instance.isBattlePlaying = false;
                Time.timeScale = 0.0f;
                SceneChanger.instance.ChangeScene(OnGameOver, 0, 0, 0);
            }
        }

        for (int i = 0; i < baseRings.Count; i++)
        {
            baseRings[i].RenewStat();
        }
        for (int i = DeckManager.instance.rings.Count - 1; i >= 0; i--) DeckManager.instance.rings[i].ChangeCurATK(0);

        if (_HP == 0) return false;
        return true;
    }

    //��带 �ٲ۴�. ��尡 �����ϸ� false�� ��ȯ�Ѵ�.
    public bool ChangeGold(int _gold)
    {
        if (gold + _gold < 0) return false;
        gold += _gold;
        UIManager.instance.playerGoldText.text = gold.ToString();
        return true;
    }

    //���̾Ƹ�带 �ٲ۴�. ���̾Ƹ�尡 �����ϸ� false�� ��ȯ�Ѵ�.
    public bool ChangeDiamond(int _diamond)
    {
        if (diamond + _diamond < 0) return false;
        diamond += _diamond;
        UIManager.instance.playerDiamondText.text = diamond.ToString();
        return true;
    }

    //������ �÷��̾�� �ش�. �̹� �ִ� �����̸鼭 ���� ���α��� �Ȱ��ٸ� false�� ��ȯ�Ѵ�. �׷��� �ʴٸ� ȹ����� ȿ���� �����ϰ� true�� ��ȯ�Ѵ�.
    public bool AddRelicToPlayer(int id, bool isPure, bool isProgressUp)
    {
        if (baseRelics[id].have && baseRelics[id].isPure == isPure) return false;
        if (!baseRelics[id].have)
        {
            relics.Add(id);
            baseRelics[id].have = true;
        }
        baseRelics[id].isPure = isPure;
        if (!isPure) cursedRelics.Add(id);

        switch (id)
        {
            case 1:     //������ �Ӹ�
                if (baseRelics[id].isPure)
                {
                    playerMaxHP += 20;
                    //if (isProgressUp) ChangePlayerCurHP(20);
                }
                else
                {
                    playerMaxHP -= 20;
                    playerCurHP = Mathf.Min(playerCurHP, playerMaxHP);
                    //if (isProgressUp) ChangePlayerCurHP(0);
                }
                break;
            case 4:     //�Ҹ����� ����
                if (!isProgressUp) break;
                if (baseRelics[id].isPure) revivable = true;
                else
                    for (int i = 0; i < baseMonsters.Count; i++)
                        if (baseMonsters[i].tier == 'n') baseMonsters[i].baseATK = 2;
                break;
            case 11:    //������ �ڷ�
            case 13:    //������ ����
                if (!isProgressUp) break;
                if (FloorManager.instance.floor != null)
                    for (int i = 1; i <= 9; i++)
                        for (int j = 1; j <= 9; j++)
                        {
                            if (FloorManager.instance.floor.rooms[i, j].type == 8)
                            {
                                Room storeRoom = FloorManager.instance.floor.rooms[i, j];

                                for (int it = 0; it < storeRoom.items.Count; it++)
                                {
                                    if (storeRoom.items[it].itemType == 1 && id == 13 && !isPure) ReturnItemToPool(storeRoom.items[it]);
                                    else storeRoom.items[it].InitializeItem(storeRoom.items[it].itemType, storeRoom.items[it].pos, storeRoom.items[it].costType, storeRoom.items[it].baseCost);
                                }

                                break;
                            }
                        }
                break;
            case 12:    //��ȯ���� �ູ
                if (baseRelics[id].isPure)
                    for (int i = 0; i < baseRings.Count; i++)
                        baseRings[i].baseRP = (int)(baseRings[i].csvRP * 0.9f);
                else
                    for (int i = 0; i < baseRings.Count; i++)
                        baseRings[i].baseRP = (int)(baseRings[i].csvRP * 1.1f);
                break;
            case 14:    //���� ����
                if (baseRelics[id].isPure)
                    for (int i = 0; i < baseMonsters.Count; i++) baseMonsters[i].baseSPD = baseMonsters[i].initSPD * 0.9f;
                else
                    for (int i = 0; i < baseMonsters.Count; i++) baseMonsters[i].baseSPD = baseMonsters[i].initSPD * 1.05f;
                break;
            case 16:    //�л����� ��
                if (baseRelics[id].isPure)
                {
                    for (int i = 0; i < baseMonsters.Count; i++)
                        if (baseMonsters[i].tier != 'n') baseMonsters[i].baseMaxHP = baseMonsters[i].initMaxHP * 0.9f;
                }
                else
                {
                    for (int i = 0; i < baseMonsters.Count; i++)
                        if (baseMonsters[i].tier != 'n') baseMonsters[i].baseMaxHP = baseMonsters[i].initMaxHP * 1.05f;
                }
                break;
        }
        ChangePlayerCurHP(0);
        if (isProgressUp)
        {
            RelicCollectionProgressUp(id, 1);
            if (!isPure) RelicCollectionProgressUp(id, 2);
        }
        return true;
    }

    //�� �ݷ��� �����Ȳ�� �ϳ� �ø���.
    public void RingCollectionProgressUp(int ringID, int questID)
    {
        if (ringCollectionProgress[ringID, questID] != -1) ringCollectionProgress[ringID, questID] = Mathf.Clamp(ringCollectionProgress[ringID, questID] + 1, 0, ringCollectionMaxProgress[ringID, questID]);
    }

    //���� �ݷ��� �����Ȳ�� �ϳ� �ø���.
    public void RelicCollectionProgressUp(int relicID, int questID)
    {
        if (relicCollectionProgress[relicID, questID] != -1) relicCollectionProgress[relicID, questID] = Mathf.Clamp(relicCollectionProgress[relicID, questID] + 1, 0, relicCollectionMaxProgress[relicID, questID]);
    }

    //�� �ݷ��� �����Ȳ�� �ϳ� �ø���.
    public void MonsterCollectionProgressUp(int monsterID)
    {
        if (monsterCollectionProgress[monsterID] != -1) monsterCollectionProgress[monsterID] = Mathf.Clamp(monsterCollectionProgress[monsterID] + 1, 0, monsterCollectionMaxProgress[monsterID]);
    }
}