using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public SPUM_Prefabs[] spum_prefabs;

    //프리팹
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
    public AudioClip[] ringAttackAudios;

    //몬스터 이동 경로
    public PathCreator[] monsterPaths;
    public SpriteRenderer[] monsterPathImages;

    //DB
    [HideInInspector]
    public List<BaseRing> ringDB;
    [HideInInspector]
    public List<BaseMonster> monsterDB;
    [HideInInspector]
    public List<BaseRelic> relicDB;

    //오브젝트 풀
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

    //게임 진행상황 관련 변수
    public int playerMaxHP;
    public int playerCurHP;
    public int gold;
    public int diamond;
    public List<int> relics;
    public List<int> cursedRelics;

    void Awake()
    {
        instance = this;

        //DB읽기
        ReadDB();
        if (monsterDB.Count != monsterPrefabs.Length) Debug.LogError("num of monster sprites does not match");
        if (ringDB.Count != ringSprites.Length) Debug.LogError("num of ring sprites does not match");
        if (ringDB.Count != ringAttackAudios.Length) Debug.LogError("num of audios does not match");
        if (relicDB.Count != relicSprites.Length) Debug.LogError("num of relic sprites does not match");

        //오브젝트 풀 초기화
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

    //"*_db.csv"를 읽어온다.
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
            r.Upgrade(2.0f);
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
            r.isPure = true;
            r.pureDescription = (string)dataRelic[i]["effect"];
            r.cursedDescription = (string)dataRelic[i]["effect_cursed"];
            relicDB.Add(r);
        }
    }

    //몬스터를 오브젝트 풀에서 받아온다. disabled 상태로 준다.
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

    //플레이어의 현재 HP를 바꾼다. false를 반환하면 HP 변화가 없는 경우이다. 바꾼 후의 HP가 0이라면 게임오버로 넘어간다. 
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

    //골드를 바꾼다.
    public bool ChangeGold(int _gold)
    {
        if (gold + _gold < 0) return false;
        gold += _gold;
        UIManager.instance.playerGoldText.text = gold.ToString();
        return true;
    }
    
    //다이아몬드를 바꾼다.
    public bool ChangeDiamond(int _diamond)
    {
        if (diamond + _diamond < 0) return false;
        diamond += _diamond;
        UIManager.instance.playerDiamondText.text = diamond.ToString();
        return true;
    }

    public bool AddRelicToDeck(int id)
    {
        if (relicDB[id].have) return false;
        relics.Add(id);
        relicDB[id].have = true;
        return true;
    }
}
