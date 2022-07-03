using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //������
    public GameObject ringPrefab;
    public GameObject monsterPrefab;
    public GameObject[] bulletPrefabs;
    public GameObject[] particlePrefabs;
    public GameObject barrierPrefab;
    public GameObject blizzardPrefab;
    public GameObject amplifierPrefab;
    public GameObject damageTextPrefab;

    //��������Ʈ
    public Sprite[] ringSprites;
    public Sprite[] monsterSprites;
    public Sprite emptyRingSprite;

    //����
    public AudioClip[] ringAttackAudios;

    //���� �̵� ���
    public PathCreator[] monsterPaths;

    //DB
    [HideInInspector]
    public List<Ringstone> ringstoneDB;
    [HideInInspector]
    public List<BaseMonster> monsterDB;

    //������Ʈ Ǯ
    private Queue<Ring> ringPool;
    private Queue<Monster> monsterPool;
    private Queue<Bullet>[] bulletPool;
    private Queue<ParticleChecker>[] particlePool;
    private Queue<Barrier> barrierPool;
    private Queue<Blizzard> blizzardPool;
    private Queue<Amplifier> amplifierPool;
    private Queue<DamageText> damageTextPool;

    void Awake()
    {
        instance = this;

        //DB�б�
        ReadDB();
        if (monsterDB.Count != monsterSprites.Length) Debug.LogError("num of monster sprites does not match");
        if (ringstoneDB.Count != ringSprites.Length) Debug.LogError("num of ring sprites does not match");
        if (ringstoneDB.Count != ringAttackAudios.Length) Debug.LogError("num of audios does not match");

        //������Ʈ Ǯ �ʱ�ȭ
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
    }

    //"*_db.csv"�� �о�´�.
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

    //���͸� ������Ʈ Ǯ���� �޾ƿ´�. disabled ���·� �ش�.
    public Monster GetMonsterFromPool()
    {
        if (monsterPool.Count > 0) return monsterPool.Dequeue();
        else return Instantiate(monsterPrefab).GetComponent<Monster>();
    }


    //���͸� ������Ʈ Ǯ�� ��ȯ�Ѵ�. enabled ���ο� ������� �ִ� ���� disabled�ȴ�.
    public void ReturnMonsterToPool(Monster monster)
    {
        /*�߰� ���� �ʿ� - ����ó�� ������ ��*/
        if (monsterPool.Contains(monster))
        {
            Debug.LogError("already enqueued monster");
            return;
        }
        monster.gameObject.SetActive(false);
        monsterPool.Enqueue(monster);
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
        ring.ringBase = null;
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
}
