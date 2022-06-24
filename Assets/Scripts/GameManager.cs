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
    private Queue<ParticleSystem>[] particlePool;
    private Queue<DamageText> damageTextPool;

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
        particlePool = new Queue<ParticleSystem>[ringstoneDB.Count];
        damageTextPool = new Queue<DamageText>();

        for (int i = 0; i < ringstoneDB.Count; i++)
        {
            bulletPool[i] = new Queue<Bullet>();
            particlePool[i] = new Queue<ParticleSystem>();
        }
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

    //파티클을 오브젝트 풀에서 받아온다. disabled 상태로 준다.
    public ParticleSystem GetParticleFromPool(int id)
    {
        if (particlePool[id].Count > 0) return particlePool[id].Dequeue();
        else return Instantiate(particlePrefabs[id]).GetComponent<ParticleSystem>();
    }

    //파티클을 오브젝트 풀에 반환한다. enabled 여부에 상관없이 주는 순간 disabled된다.
    public void ReturnParticleToPool(ParticleSystem particle, int id)
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
}
