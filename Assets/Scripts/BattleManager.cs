using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public bool debugFlag;  //����׿�
    public int debugVariable;

    public List<Monster> monsters; //���� ���������� ���͵�

    //���� �� ����
    public bool isBattlePlaying;  //���� ���� ������ ����
    public bool isBattleOver;       //���� ������ ����(���Ͱ� ������ο� �����)
    public int pathID;

    //������ �� ����
    public int phase;   //���� ������
    private int numGenMonster;  //������ ���� ��
    private int newMonsterID;   //���� ������ ���� ���̵�

    void Awake()
    {
        instance = this;
        //while (GameManager.instance == null || BattleManager.instance == null || DeckManager.instance == null || UIManager.instance == null) continue;
        monsters = new List<Monster>();
    }

    void Update()
    {
        if (debugFlag)
        {
            debugFlag = false;
            StartBattle();
        }
        if (isBattlePlaying)
        {
            //���� ���� Ȯ��
            CheckBattleOver();
        }
    }

    //������ �����Ѵ�.
    void StartBattle()
    {
        //���� �� ����
        isBattlePlaying = true;
        isBattleOver = false;
        pathID = 0;

        //ī�޶� �ش��ϴ� �������� �̵�
        Camera.main.transform.position = GameManager.instance.monsterPaths[pathID].transform.position;
        Camera.main.transform.Translate(0, -2, -10);

        phase = 1;
        StartPhase();
    }

    //����� �����Ѵ�.
    void StartPhase()
    {
        numGenMonster = 15 * (phase + 1);
        newMonsterID = 0;
        StartCoroutine(GenerateMonster());
    }

    IEnumerator GenerateMonster()
    {
        while (newMonsterID < numGenMonster)
        {
            float scale = 1.0f;
            /*if (GameManager.instance.floor == 7) scale = 4.0f;
            else scale = 0.5f * (GameManager.instance.floor + 1);
            scale += 0.05f * (GameManager.instance.stage - 1);*/

            //���� ����
            Monster monster = GameManager.instance.GetMonsterFromPool();
            monster.gameObject.transform.position = new Vector2(100, 100);
            if (phase == 3 && (newMonsterID == 29 || newMonsterID == 59))
                monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(3, 17)], pathID, scale);
            else monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(0, 3)], pathID, scale);
            //else monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[2], pathID, scale);
            monster.gameObject.SetActive(true); //���͸� Ŵ
            monsters.Add(monster); //����Ʈ�� ������
            newMonsterID++;
            yield return new WaitForSeconds(30.0f / numGenMonster);
        }
    }

    //��Ʋ�� ����Ǿ����� Ȯ���Ѵ�.
    void CheckBattleOver()
    {
        if (isBattleOver)
        {
            //��Ʋ�� �����Ѵ�.
            isBattlePlaying = false;

            //���� ���� ����
            StopAllCoroutines();

            //���� ����Ʈ ����
            for (int i = 0; i < monsters.Count; i++)
                GameManager.instance.ReturnMonsterToPool(monsters[i]);
            monsters.Clear();

            for (int i = 0; i < DeckManager.instance.rings.Count; i++)
                GameManager.instance.ReturnRingToPool(DeckManager.instance.rings[i]);
            DeckManager.instance.rings.Clear();
        }
    }
}
