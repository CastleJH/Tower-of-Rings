using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //프리팹
    public GameObject ringPrefab;
    public GameObject monsterPrefab;
    public GameObject[] bulletPrefabs;
    public GameObject[] particlePrefabs;
    public GameObject barrierPrefab;
    public GameObject blizzardPrefab;
    public GameObject amplifierPrefab;
    public GameObject damageTextPrefab;

    //스프라이트
    public Sprite[] ringSprites;
    public Sprite[] monsterSprites;
    public Sprite emptyRingSprite;

    //사운드
    public AudioClip[] ringAttackAudios;

    //몬스터 이동 경로
    public PathCreator[] monsterPaths;

    //DB
    [HideInInspector]
    public List<Ringstone> ringstoneDB;
    [HideInInspector]
    public List<BaseMonster> monsterDB;

    //오브젝트 풀
    private Queue<Ring> ringPool;
    private Queue<Monster> monsterPool;
    private Queue<Bullet>[] bulletPool;
    private Queue<ParticleChecker>[] particlePool;
    private Queue<Barrier> barrierPool;
    private Queue<Blizzard> blizzardPool;
    private Queue<Amplifier> amplifierPool;
    private Queue<DamageText> damageTextPool;

    //게임 진행상황 관련 변수
    private int playerMaxHP;
    private int playerCurHP;
    public int gold;
    public int emerald;
    public Floor floor;


    void Awake()
    {
        instance = this;

        //DB읽기
        ReadDB();
        if (monsterDB.Count != monsterSprites.Length) Debug.LogError("num of monster sprites does not match");
        if (ringstoneDB.Count != ringSprites.Length) Debug.LogError("num of ring sprites does not match");
        if (ringstoneDB.Count != ringAttackAudios.Length) Debug.LogError("num of audios does not match");

        //오브젝트 풀 초기화
        ringPool = new Queue<Ring>();
        monsterPool = new Queue<Monster>();
        bulletPool = new Queue<Bullet>[ringstoneDB.Count];
        particlePool = new Queue<ParticleChecker>[ringstoneDB.Count];
        barrierPool = new Queue<Barrier>();
        blizzardPool = new Queue<Blizzard>();
        amplifierPool = new Queue<Amplifier>();
        damageTextPool = new Queue<DamageText>();

        for (int i = 0; i < ringstoneDB.Count; i++)
        {
            bulletPool[i] = new Queue<Bullet>();
            particlePool[i] = new Queue<ParticleChecker>();
        }

        InitializeGame();
    }

    //게임을 초기화한다.
    void InitializeGame()
    {
        playerMaxHP = 100;
        playerCurHP = 100;
        gold = 0;
        emerald = 0;
        floor = new Floor();
        floor.Generate(2);
    }

    //"*_db.csv"를 읽어온다.
    void ReadDB()
    {
        List<Dictionary<string, object>> dataRing = DBReader.Read("ring_db");
        ringstoneDB = new List<Ringstone>();
        for (int i = 0; i < dataRing.Count; i++)
        {
            Ringstone r = new Ringstone();
            r.id = (int)dataRing[i]["id"];
            r.rarity = (int)dataRing[i]["rarity"];
            r.name = (string)dataRing[i]["name"];
            r.dbATK = (int)dataRing[i]["atk"];
            r.dbSPD = float.Parse(dataRing[i]["spd"].ToString());
            r.baseNumTarget = (int)dataRing[i]["target"];
            r.baseRP = (int)dataRing[i]["rp"];
            r.baseEFF = float.Parse(dataRing[i]["eff"].ToString());
            r.description = (string)dataRing[i]["description"];
            r.range = (int)dataRing[i]["range"];
            r.identical = (string)dataRing[i]["identical"];
            r.different = (string)dataRing[i]["all"];
            r.level = 0;
            r.Upgrade();
            ringstoneDB.Add(r);
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
            monsterDB.Add(m);
        }
    }

    //몬스터를 오브젝트 풀에서 받아온다. disabled 상태로 준다.
    public Monster GetMonsterFromPool()
    {
        if (monsterPool.Count > 0) return monsterPool.Dequeue();
        else return Instantiate(monsterPrefab).GetComponent<Monster>();
    }


    //몬스터를 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnMonsterToPool(Monster monster)
    {
        /*추가 구현 필요 - 예외처리 제거할 것*/
        if (monsterPool.Contains(monster))
        {
            Debug.LogError("already enqueued monster");
            return;
        }
        monster.gameObject.SetActive(false);
        monsterPool.Enqueue(monster);
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
        ring.ringBase = null;
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

    //플레이어의 현재 HP를 바꾼다. 0이라면 게임오버.
    public void ChangePlayerCurHP(int _HP)
    {
        playerCurHP = Mathf.Clamp(playerCurHP + _HP, 0, playerMaxHP);
        if (playerCurHP == 0)
        {
            Time.timeScale = 0.0f;
            Debug.Log("GAME OVER!!!");
        }
    }

    //골드를 바꾼다.
    public bool ChangeGold(int _gold)
    {
        if (gold + _gold < 0) return false;
        gold += _gold;
        return true;
    }
    
    //에메랄드를 바꾼다.
    public bool ChangeEmerald(int _emerald)
    {
        if (emerald + _emerald < 0) return false;
        emerald += _emerald;
        return true;
    }
}
