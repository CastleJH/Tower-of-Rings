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
    public int pathID;      //���� ���� ��ȣ
    public float rp;        //���� ���� RP

    //������ �� ����
    public int phase;   //���� ������
    private int numGenMonster;  //������ ���� ��
    private int newMonsterID;   //���� ������ ���� ���̵�

    void Awake()
    {
        instance = this;
        monsters = new List<Monster>();
    }

    void Update()
    {
        if (debugFlag)
        {
            debugFlag = false;
            StartBattle();
        }
        if (isBattlePlaying)    //���� ���� ���
        {
            //���� ���� ���� Ȯ��
            CheckBattleOver();
        }
    }

    //������ �����Ѵ�.
    void StartBattle()
    {
        //���� �� ���� �ʱ�ȭ
        isBattlePlaying = true;
        isBattleOver = false;
        pathID = 0; //�ϴ� �ӽ÷� �����ߴ�. �����δ� �������� ����������.

        //���� �̹���/RP ������ �ʱ�ȭ�Ѵ�.
        UIManager.instance.SetBattleDeckRingImageAndRPAll();

        //�ʱ� RP�� ���ϰ� UI�� ������Ʈ�Ѵ�.
        ChangeCurrentRP(500);   //��Ȱ�� �׽�Ʈ�� ���� �ʱ� RP�� �˳��ϰ� ���. �����δ� ���� ���� ���ε ���� 80 ��ó���� ������.

        //�� �Ŵ����� ���� ���� �������� �ʱ�ȭ�Ѵ�.
        DeckManager.instance.PrepareBattle();

        //ī�޶� �ش��ϴ� �������� �̵��Ѵ�.
        Camera.main.transform.position = GameManager.instance.monsterPaths[pathID].transform.position;
        Camera.main.transform.Translate(0, -2, -10);

        phase = 1;
        StartPhase();
    }

    //����� �����Ѵ�. ���� ������ �ʱ�ȭ�ϰ� ���� ���� �ڷ�ƾ�� �����Ѵ�.
    void StartPhase()
    {
        numGenMonster = 15 * (phase + 1);
        newMonsterID = 0;
        StartCoroutine(GenerateMonster());
    }

    //���͸� �����Ѵ�.
    IEnumerator GenerateMonster()
    {
        while (newMonsterID < numGenMonster)
        {
            //���� �ɷ�ġ ������ �����Ѵ�.
            float scale = 5.0f;     //�ϴ��� ������ 1���� �������� ���о��� �ϴ� ���̹Ƿ� �����س���.
            //�� �κ��� �Ŀ� ������ 3���� ��� �÷����ϰ� �������� ������ ���� �� �ִ´�.
            /*if (GameManager.instance.floor == 7) scale = 4.0f;
            else scale = 0.5f * (GameManager.instance.floor + 1);
            scale += 0.05f * (GameManager.instance.stage - 1);*/

            //���� ����
            Monster monster = GameManager.instance.GetMonsterFromPool();
            monster.gameObject.transform.position = new Vector2(100, 100);  //�ʱ⿡�� �ָ� ����߷����ƾ� path�� �߰����� �۸�ġ ���� ����.
            if (phase == 3 && (newMonsterID == 29 || newMonsterID == 59))   //������ 3�� �߰�/������ ���ʹ� �ݵ�� ����
                monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(3, 17)], pathID, scale);
            else monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(0, 3)], pathID, scale);    //�׿ܿ��� �Ϲ� ����
            monster.gameObject.SetActive(true);
            monsters.Add(monster);
            newMonsterID++;
            yield return new WaitForSeconds(30.0f / numGenMonster); //30�� ���� ���͵��� ������Ѿ� �Ѵ�. ���� ���� ����Ͽ� ���� �ֱ⸦ �����Ѵ�.
        }
    }

    //��Ʋ�� ����Ǿ����� Ȯ���Ѵ�.
    void CheckBattleOver()
    {
        //����� Ŭ����� ������ ���� ������ �ϵ��� �� ������.
        if ((monsters.Count == 0 && newMonsterID == numGenMonster) || isBattleOver)
        {
            Debug.Log("End Battle");

            //��Ʋ�� �����Ѵ�.
            isBattlePlaying = false;

            //���� ���� ����
            StopAllCoroutines();

            //���� ����Ʈ ����
            for (int i = monsters.Count - 1; i >= 0; i--)
                monsters[i].RemoveFromBattle(0.0f);
            monsters.Clear();

            //�� ����
            for (int i = 0; i < DeckManager.instance.rings.Count; i++)
                GameManager.instance.ReturnRingToPool(DeckManager.instance.rings[i]);
            DeckManager.instance.rings.Clear();


            //�� ����/���� ���̾��ٸ� �̰͵� ����
            if (DeckManager.instance.isEditRing)
            {
                if (DeckManager.instance.genRing != null) GameManager.instance.ReturnRingToPool(DeckManager.instance.genRing);
                else DeckManager.instance.ringRemover.transform.position = new Vector3(100, 100, 0);
            }   
        }
    }

    //���� RP���� �ٲ۴�.
    public void ChangeCurrentRP(float _rp)
    {
        rp = _rp;
        UIManager.instance.battleRPText.text = ((int)rp).ToString();
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)    //RP���� ����� ���� ��ư Ȱ��ȭ(=��ư ������ ��Ȱ��ȭ)
        {
            if (i < DeckManager.instance.deck.Count && int.Parse(UIManager.instance.battleDeckRingRPText[i].text) <= rp)
                UIManager.instance.battleRPNotEnough[i].SetActive(false);
            else UIManager.instance.battleRPNotEnough[i].SetActive(true);
        }
        if (rp < 10) UIManager.instance.battleRPNotEnough[DeckManager.instance.maxDeckLength].SetActive(true);
        else UIManager.instance.battleRPNotEnough[DeckManager.instance.maxDeckLength].SetActive(false);
    }
}
