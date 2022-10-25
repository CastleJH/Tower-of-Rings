using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //public Text debugText;

    //프리팹
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

    //스프라이트
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

    //사운드
    public AudioSource audioSource;
    public AudioClip[] bgms;
    public AudioClip[] ringAttackAudios;
    public AudioClip[] specialAudios;   //버튼 해머 겟 적등장 포탈
    
    //몬스터 이동 경로 & 맵 구성
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

    //오브젝트 풀
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

    //통합 게임 관련 변수 & 플레이어 통합 진행사항(일부는 GPGS에 저장됨)
    public int diamond;                         //GPGS 저장
    public int hardModeOpen;                    //GPGS 저장
    public int[] spiritMaxLevel;
    public float[] spiritBaseEnhanceCost;
    public int[] spiritEnhanceLevel;            //GPGS 저장
    public int[] ringCollecionRewardAmount;
    public int[,] ringCollectionMaxProgress;
    public int[,] ringCollectionProgress;       //GPGS 저장(-1이면 이미 보상을 획득하였다는 뜻임)
    public int[] relicCollecionRewardAmount;
    public int[,] relicCollectionMaxProgress;
    public int[,] relicCollectionProgress;      //GPGS 저장(-1이면 이미 보상을 획득하였다는 뜻임)
    public int[] monsterCollectionMaxProgress;  
    public int[] monsterCollectionProgress;     //GPGS 저장(-1이면 이미 보상을 획득하였다는 뜻임)
    bool tutorialDone;

    //광고 보상(일부는 GPGS에 저장됨)
    public int diamondRewardTakeNum;
    public DateTime diamondAdLastTookTime;
    public int boostRewardTakeNum;
    public DateTime boostAdLastTookTime;
    public int boostLeft;

    //개별 게임 관련 변수
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

        //DB읽기
        ReadDB();
        if (baseMonsters.Count != monsterPrefabs.Length) Debug.LogError("num of monster sprites does not match");
        if (baseRings.Count != ringSprites.Length) Debug.LogError("num of ring sprites does not match");
        if (baseRings.Count != ringAttackAudios.Length) Debug.LogError("num of audios does not match");
        if (baseRelics.Count != relicSprites.Length) Debug.LogError("num of relic sprites does not match");

        //오브젝트 풀 초기화
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

        //영혼 강화&콜렉션 기록
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
            UIManager.instance.gameStartPanelSignInButton.gameObject.SetActive(false);
            UIManager.instance.gameStartPanelMoveToGameButton.SetActive(false);
            UIManager.instance.gameStartPanelMoveToLobbyButton.SetActive(true);
        }

        tutorialDone = false;
    }

    public void InitializeUserData()
    {
        //영혼 강화 정보 초기화
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

    //게임을 시작한다.
    public void GameStart()
    {
        for (int i = 0; i < baseRelics.Count; i++)
            if (ringCollectionProgress[i, 0] != 0)
            {
                tutorialDone = true;
                break;
            }

        saveFloor = true;

        int startFloor = 0;
        if (tutorialDone) startFloor = 1;
        tutorialDone = true;

        if (startFloor == 0) TutorialManager.instance.isTutorial = true;
        InitializeGame();
        UIManager.instance.mapPanel.SetActive(true);
        FloorManager.instance.endPortal.SetActive(false);
        
        FloorManager.instance.CreateAndMoveToFloor(startFloor);
        boostLeft = 0;
    }

    //저장된 게임을 시작한다(1층이 아닌 곳에서 시작).
    public void GameStartSaved()
    {
        saveFloor = false;
        InitializeGameSaved();
        UIManager.instance.mapPanel.SetActive(true);
        FloorManager.instance.endPortal.SetActive(false);
        FloorManager.instance.CreateAndMoveToFloor(FloorManager.instance.floor.floorNum);
        boostLeft = 0;
    }

    //게임을 초기화한다.
    void InitializeGame()
    {
        isNormalMode = !UIManager.instance.lobbyHardModeToggleButton.isOn;
        Time.timeScale = 1.0f;
        
        //유물 등으로 인해 변한 몬스터/링/유물 원형 값을 초기로 되돌린다(DB값과 일치하도록).
        ResetBases(isNormalMode);

        //유물을 모두 비운다.
        relics.Clear();
        cursedRelics.Clear();

        //플레이어 시작 HP값을 정한다.
        playerMaxHP = 100 + spiritEnhanceLevel[2] * 4;
        if (!isNormalMode) playerMaxHP /= 2;
        if (TutorialManager.instance.isTutorial) playerMaxHP = 999;
        playerCurHP = playerMaxHP;
        ChangePlayerCurHP(0);   //여기서 영혼강화로 인한 링 기본 공격력/공격 쿨타임도 변한다.

        //플레이어 시작 골드량을 정한다. 다이아몬드는 개별 게임과 독립적이므로 바꾸지 않는다.
        gold = spiritEnhanceLevel[4] * 2 ;
        ChangeGold(0);
        ChangeDiamond(0);

        //부활 가능 여부를 초기화한다.
        revivable = false;

        //덱을 초기화한다.
        DeckManager.instance.InitializeDeck();
    }

    void InitializeGameSaved()
    {
        Time.timeScale = 1.0f;

        ChangePlayerCurHP(0);   //여기서 영혼강화로 인한 링 기본 공격력/공격 쿨타임도 변한다.

        ChangeGold(0);
        ChangeDiamond(0);
    }

    //진행중이던 게임을 게임오버 처리한다(HP가 0이 되었거나, 플레이어가 메뉴에서 포기를 눌렀거나).
    public void OnGameOver(int a, int b)
    {
        UIManager.instance.OpenEndingPanel(0);  //게임오버 이미지로 패널을 연다.
        BattleManager.instance.StopBattleSystem();     //배틀시스템을 모두 종료한다.
        Time.timeScale = 1;         //속도를 원래대로 돌린다.
    }

    //진행중이던 게임을 클리어 처리한다(7층의 보스를 처치했을 때).
    public void OnGameClear(int a, int b)
    {
        UIManager.instance.OpenEndingPanel(1);  //게임클리어 이미지로 패널을 연다.
        UIManager.instance.lobbyHardModeToggleButton.gameObject.SetActive(true);
        hardModeOpen = 1;
        BattleManager.instance.StopBattleSystem();     //배틀시스템을 모두 종료한다.
        Time.timeScale = 1;         //속도를 원래대로 돌린다.
    }

    //유물 등으로 인해 변한 몬스터/링/유물 원형 값을 DB값과 일치시킨다(다만, 하드모드면 몬스터 HP가 2배가 될 수 있다).
    public void ResetBases(bool isNormal)
    {
        int mult = isNormal ? 1 : 2;    //하드모드면 몬스터 HP만 두배로 해준다.
        for (int i = 0; i < baseRings.Count; i++) baseRings[i].Init();
        for (int i = 0; i < baseRelics.Count; i++) baseRelics[i].Init();
        for (int i = 0; i < baseMonsters.Count; i++) baseMonsters[i].Init(mult);

    }

    //"*_db.csv"를 읽어온다.
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

    //몬스터를 오브젝트 풀에서 받아온다. disabled 상태로 준다.
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

    //몬스터를 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnMonsterToPool(Monster monster)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (monsterPool[monster.baseMonster.type].Contains(monster))
        {
            Debug.LogError("already enqueued monster");
            return;
        }
        monster.gameObject.SetActive(false);
        monsterPool[monster.baseMonster.type].Enqueue(monster);
    }

    //몬스터 파티클을 오브젝트 풀에서 받아온다. disabled 상태로 준다.
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

    //몬스터 파티클을 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnMonsterParticleToPool(ParticleChecker particle, int id)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (monsterParticlePool[id].Contains(particle))
        {
            Debug.LogError("already enqueued monster particle");
            return;
        }
        particle.gameObject.SetActive(false);
        monsterParticlePool[id].Enqueue(particle);
    }

    //링을 오브젝트 풀에서 받아온다. disabled 상태로 준다.
    public Ring GetRingFromPool()
    {
        if (ringPool.Count > 0) return ringPool.Dequeue();
        else return Instantiate(ringPrefab).GetComponent<Ring>();
    }

    //링을 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnRingToPool(Ring ring)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
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

    //불렛을 오브젝트 풀에서 받아온다. disabled 상태로 준다.
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

    //불렛을 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnBulletToPool(Bullet bullet)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (bulletPool[bullet.destroyID].Contains(bullet))
        {
            Debug.LogError("already enqueued bullet");
            return;
        }
        bullet.gameObject.SetActive(false);
        bulletPool[bullet.destroyID].Enqueue(bullet);
    }

    //불렛 풀에 있는 모든 오브젝트를 제거한다.
    public void EmptyBulletPool(int id)
    {
        while (bulletPool[id].Count > 0) Destroy(bulletPool[id].Dequeue().gameObject);
    }

    //파티클을 오브젝트 풀에서 받아온다. disabled 상태로 준다.
    public ParticleChecker GetParticleFromPool(int id)
    {
        if (particlePool[id].Count > 0) return particlePool[id].Dequeue();
        else return Instantiate(particlePrefabs[id]).GetComponent<ParticleChecker>();
    }

    //파티클을 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnParticleToPool(ParticleChecker particle, int id)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (particlePool[id].Contains(particle))
        {
            Debug.LogError("already enqueued particle");
            return;
        }
        particle.gameObject.SetActive(false);
        particlePool[id].Enqueue(particle);
    }

    //파티클 풀에 있는 모든 오브젝트를 제거한다.
    public void EmptyParticlePool(int id)
    {
        while (particlePool[id].Count > 0) Destroy(particlePool[id].Dequeue().gameObject);
    }

    //결계를 오브젝트 풀에서 받아온다. disabled 상태로 준다.
    public Barrier GetBarrierFromPool()
    {
        if (barrierPool.Count > 0) return barrierPool.Dequeue();
        else return Instantiate(barrierPrefab).GetComponent<Barrier>();
    }

    //결계를 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnBarrierToPool(Barrier barrier)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (barrierPool.Contains(barrier))
        {
            Debug.LogError("already enqueued barrier");
            return;
        }
        barrier.gameObject.SetActive(false);
        barrierPool.Enqueue(barrier);
    }

    //눈보라를 오브젝트 풀에서 받아온다. disabled 상태로 준다.
    public Blizzard GetBlizzardFromPool()
    {
        if (blizzardPool.Count > 0) return blizzardPool.Dequeue();
        else return Instantiate(blizzardPrefab).GetComponent<Blizzard>();
    }


    //눈보라를 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnBlizzardToPool(Blizzard blizzard)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (blizzardPool.Contains(blizzard))
        {
            Debug.LogError("already enqueued blizzard");
            return;
        }
        blizzard.gameObject.SetActive(false);
        blizzardPool.Enqueue(blizzard);
    }

    //증폭을 오브젝트 풀에서 받아온다. disabled 상태로 준다.
    public Amplifier GetAmplifierFromPool()
    {
        if (amplifierPool.Count > 0) return amplifierPool.Dequeue();
        else return Instantiate(amplifierPrefab).GetComponent<Amplifier>();
    }


    //증폭을 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnAmplifierToPool(Amplifier amplifier)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (amplifierPool.Contains(amplifier))
        {
            Debug.LogError("already enqueued amplifier");
            return;
        }
        amplifier.gameObject.SetActive(false);
        amplifierPool.Enqueue(amplifier);
    }

    //데미지 표시기를 오브젝트 풀에서 받아온다. disabled 상태로 준다.
    public DamageText GetDamageTextFromPool()
    {
        if (damageTextPool.Count > 0) return damageTextPool.Dequeue();
        else return Instantiate(damageTextPrefab).GetComponent<DamageText>();
    }

    //데미지 표시기를 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnDamageTextToPool(DamageText damageText)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (damageTextPool.Contains(damageText))
        {
            Debug.LogError("already enqueued damageText");
            return;
        }
        damageText.gameObject.SetActive(false);
        damageTextPool.Enqueue(damageText);
    }

    //링의 정수를 오브젝트 풀에서 받아온다. disabled 상태로 준다.
    public DropRP GetDropRPFromPool()
    {
        if (dropRPPool.Count > 0) return dropRPPool.Dequeue();
        else return Instantiate(dropRPPrefab).GetComponent<DropRP>();
    }


    //링의 정수를 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnDropRPToPool(DropRP dropRP)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (dropRPPool.Contains(dropRP))
        {
            Debug.LogError("already enqueued dropRP");
            return;
        }
        dropRP.gameObject.SetActive(false);
        dropRPPool.Enqueue(dropRP);
    }

    //아이템을 오브젝트 풀에서 받아온다. disabled 상태로 준다.
    public Item GetItemFromPool()
    {
        if (itemPool.Count > 0) return itemPool.Dequeue();
        else return Instantiate(itemPrefab).GetComponent<Item>();
    }


    //아이템을 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnItemToPool(Item item)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (itemPool.Contains(item))
        {
            Debug.LogError("already enqueued item");
            return;
        }
        item.gameObject.SetActive(false);
        itemPool.Enqueue(item);
    }

    //플레이어의 현재 HP를 바꾼다. false를 반환하면 바꾼 HP가 없는 경우이다. 바꾼 후의 HP가 0이라면 게임오버로 넘어간다. 
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

    //골드를 바꾼다. 골드가 부족하면 false를 반환한다.
    public bool ChangeGold(int _gold)
    {
        if (gold + _gold < 0) return false;
        gold += _gold;
        UIManager.instance.playerGoldText.text = gold.ToString();
        return true;
    }

    //다이아몬드를 바꾼다. 다이아몬드가 부족하면 false를 반환한다.
    public bool ChangeDiamond(int _diamond)
    {
        if (diamond + _diamond < 0) return false;
        diamond += _diamond;
        UIManager.instance.playerDiamondText.text = diamond.ToString();
        return true;
    }

    //유물을 플레이어에게 준다. 이미 있던 유물이면서 저주 여부까지 똑같다면 false를 반환한다. 그렇지 않다면 획득시의 효과를 적용하고 true를 반환한다.
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
            case 1:     //거인의 머리
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
            case 4:     //불멸자의 심장
                if (!isProgressUp) break;
                if (baseRelics[id].isPure) revivable = true;
                else
                    for (int i = 0; i < baseMonsters.Count; i++)
                        if (baseMonsters[i].tier == 'n') baseMonsters[i].baseATK = 2;
                break;
            case 11:    //상인의 자루
            case 13:    //고물상의 수레
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
            case 12:    //소환사의 축복
                if (baseRelics[id].isPure)
                    for (int i = 0; i < baseRings.Count; i++)
                        baseRings[i].baseRP = (int)(baseRings[i].csvRP * 0.9f);
                else
                    for (int i = 0; i < baseRings.Count; i++)
                        baseRings[i].baseRP = (int)(baseRings[i].csvRP * 1.1f);
                break;
            case 14:    //빙하 조각
                if (baseRelics[id].isPure)
                    for (int i = 0; i < baseMonsters.Count; i++) baseMonsters[i].baseSPD = baseMonsters[i].initSPD * 0.9f;
                else
                    for (int i = 0; i < baseMonsters.Count; i++) baseMonsters[i].baseSPD = baseMonsters[i].initSPD * 1.05f;
                break;
            case 16:    //학살자의 낫
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

    //링 콜렉션 진행상황을 하나 올린다.
    public void RingCollectionProgressUp(int ringID, int questID)
    {
        if (TutorialManager.instance.isTutorial) return;
        if (ringCollectionProgress[ringID, questID] != -1) ringCollectionProgress[ringID, questID] = Mathf.Clamp(ringCollectionProgress[ringID, questID] + 1, 0, ringCollectionMaxProgress[ringID, questID]);
    }

    //유물 콜렉션 진행상황을 하나 올린다.
    public void RelicCollectionProgressUp(int relicID, int questID)
    {
        if (TutorialManager.instance.isTutorial) return;
        if (relicCollectionProgress[relicID, questID] != -1) relicCollectionProgress[relicID, questID] = Mathf.Clamp(relicCollectionProgress[relicID, questID] + 1, 0, relicCollectionMaxProgress[relicID, questID]);
    }

    //적 콜렉션 진행상황을 하나 올린다.
    public void MonsterCollectionProgressUp(int monsterID)
    {
        if (TutorialManager.instance.isTutorial) return;
        if (monsterCollectionProgress[monsterID] != -1) monsterCollectionProgress[monsterID] = Mathf.Clamp(monsterCollectionProgress[monsterID] + 1, 0, monsterCollectionMaxProgress[monsterID]);
    }
}