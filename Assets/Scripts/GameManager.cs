using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //������
    public GameObject monsterPrefab;

    //��������Ʈ
    public Sprite[] monsterSprites;

    //���� �̵� ���
    public PathCreator[] monsterPaths;

    //DB
    [HideInInspector]
    public List<BaseMonster> monsterDB;

    //������Ʈ Ǯ
    private Queue<Monster> monsterPool;

    void Awake()
    {
        instance = this;

        //DB�б�
        ReadDB();
        if (monsterDB.Count != monsterSprites.Length) Debug.LogError("num of monster sprites does not match");
        
        //������Ʈ Ǯ �ʱ�ȭ
        monsterPool = new Queue<Monster>();
    }

    //"*_db.csv"�� �о�´�.
    void ReadDB()
    {
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
        Debug.Log("Clear!");
        monster.gameObject.SetActive(false);
        monsterPool.Enqueue(monster);
    }
}
