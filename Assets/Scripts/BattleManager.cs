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
    public float rp;        //���� ���� RP
    public int goldGet;
    public int emeraldGet;
    public List<int> ringDowngrade;

    //���̺� �� ����
    public int wave;   //���� ���̺�
    private int numGenMonster;  //������ ���� ��
    private int newMonsterID;   //���� ������ ���� ���̵�

    void Awake()
    {
        instance = this;
        monsters = new List<Monster>();
        ringDowngrade = new List<int>();
    }

    void Update()
    {
        if (isBattlePlaying)    //���� ���� ���
        {
            //���� ���� ���� Ȯ��
            CheckBattleOver();
        }
    }

    //������ �����Ѵ�.
    public void StartBattle()
    {
        //���� �� ���� �ʱ�ȭ
        isBattlePlaying = true;
        goldGet = 0;
        emeraldGet = 0;
        ringDowngrade.Clear();

        //������ Ų��.
        UIManager.instance.TurnMapOnOff(false);
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(true);
        UIManager.instance.TurnDeckOnOff(true);

        //���� �̹���/RP ������ �ʱ�ȭ�Ѵ�.
        UIManager.instance.SetBattleDeckRingImageAndRPAll();

        //�ʱ� RP�� ���ϰ� UI�� ������Ʈ�Ѵ�.
        ChangeCurrentRP(500);   //��Ȱ�� �׽�Ʈ�� ���� �ʱ� RP�� �˳��ϰ� ���. �����δ� ���� ���� ���ε ���� 80 ��ó���� ������.

        //�� �Ŵ����� ���� ���� �������� �ʱ�ȭ�Ѵ�.
        DeckManager.instance.PrepareBattle();

        wave = 1;
        //���켼��!
        //wave = 3;
        StartWave();
    }

    //���̺긦 �����Ѵ�. ���� ������ �ʱ�ȭ�ϰ� ���� ���� �ڷ�ƾ�� �����Ѵ�.
    void StartWave()
    {
        if (wave == 2) numGenMonster = 45;
        else numGenMonster = 30;
        //���켼��!
        //numGenMonster = 1;
        newMonsterID = 0;
        StartCoroutine(GenerateMonster());
    }

    //���͸� �����Ѵ�.
    IEnumerator GenerateMonster()
    {
        while (newMonsterID < numGenMonster)
        {
            //���� �ɷ�ġ ������ �����Ѵ�.
            float scale;
            scale = 0.5f * (FloorManager.instance.floor.floorNum + 1) + 0.05f * (wave - 1);

            //���� ����
            Monster monster = GameManager.instance.GetMonsterFromPool();
            monster.gameObject.transform.position = new Vector2(100, 100);  //�ʱ⿡�� �ָ� ����߷����ƾ� path�� �߰����� �۸�ġ ���� ����.
            if (wave == 3 && newMonsterID == 0)   //���̺� 3�� ù ���ʹ� �ݵ�� ����Ʈ/����
            {
                if (FloorManager.instance.curRoom.type == 1) monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(3, 10)], FloorManager.instance.curRoom.pathID, 1.0f);
                //���켼��!
                //if (FloorManager.instance.curRoom.type == 1) monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[14], FloorManager.instance.curRoom.pathID, 1.0f);
                else monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[FloorManager.instance.floor.floorNum + 9], FloorManager.instance.curRoom.pathID, 1.0f);
            }
            else monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(0, 3)], FloorManager.instance.curRoom.pathID, scale);    //�׿ܿ��� �Ϲ� ����
            monster.gameObject.SetActive(true);
            monsters.Add(monster);
            newMonsterID++;
            yield return new WaitForSeconds(30.0f / numGenMonster); //30�� ���� ���͵��� ������Ѿ� �Ѵ�. ���� ���� ����Ͽ� ���� �ֱ⸦ �����Ѵ�.
        }
    }

    //��Ʋ�� ����Ǿ����� Ȯ���Ѵ�.
    void CheckBattleOver()
    {
        if (monsters.Count == 0 && newMonsterID == numGenMonster)
        {
            if (wave == 3)     //������ ���̺꿴�ٸ� ������ �ְ� ������ �����Ѵ�.
            {
                //��Ʋ�� �����Ѵ�.
                isBattlePlaying = false;

                //���� ���� ����
                StopAllCoroutines();

                //���� ����Ʈ ����
                /*for (int i = monsters.Count - 1; i >= 0; i--)
                    monsters[i].RemoveFromBattle(0.0f);
                monsters.Clear();*/

                float greedyATK = -1.0f;
                float greedyEFF = 0.0f;

                //�� ����
                for (int i = 0; i < DeckManager.instance.rings.Count; i++)
                {
                    if (DeckManager.instance.rings[i].baseRing.id == 17)
                    {
                        greedyATK = DeckManager.instance.rings[i].curATK;
                        greedyEFF = DeckManager.instance.rings[i].curEFF;
                    }
                    GameManager.instance.ReturnRingToPool(DeckManager.instance.rings[i]);
                }
                DeckManager.instance.rings.Clear();

                //�� ����/���� ���̾��ٸ� �̰͵� ����
                if (DeckManager.instance.isEditRing)
                {
                    if (DeckManager.instance.genRing != null) GameManager.instance.ReturnRingToPool(DeckManager.instance.genRing);
                    else DeckManager.instance.ringRemover.transform.position = new Vector3(100, 100, 0);
                }

                //�ٿ�׷��̵� �� ������ 50% Ȯ���� ����
                for (int i = 0; i < ringDowngrade.Count; i++)
                    if (Random.Range(0, 2) == 1) GameManager.instance.ringDB[ringDowngrade[i]].Upgrade();

                emeraldGet = 0;
                if (greedyATK != -1.0f)   //Ž�帵�� �����ߴٸ� ������ �ø�
                {
                    goldGet += (int)(goldGet * greedyATK * 0.01f);
                    if (Random.Range(0.0f, 1.0f) <= greedyATK * 0.01f + greedyEFF) emeraldGet = Random.Range(3, 6);
                }
                GameManager.instance.ChangeGold(goldGet);
                GameManager.instance.ChangeEmerald(emeraldGet);

                //������ ���� ���� �����ϰ� ��Ż�� �����ش�.
                GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(false);
                UIManager.instance.TurnDeckOnOff(false);
                FloorManager.instance.ChangeCurRoomToIdle();
                UIManager.instance.RevealMapArea(FloorManager.instance.playerX, FloorManager.instance.playerY);
                UIManager.instance.TurnMapOnOff(true);
                FloorManager.instance.TurnPortalsOnOff(true);
            }
            else
            {
                wave++;
                StartWave();
            }
        }
    }

    //���� RP���� �ٲ۴�.
    public void ChangeCurrentRP(float _rp)
    {
        int consumeRP;
        rp = _rp;
        UIManager.instance.battleRPText.text = ((int)rp).ToString();
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)    //RP�� ���� �� ���� ������ ���� ��ư Ȱ��ȭ(=��ư ������ ��Ȱ��ȭ)
        {
            if (int.TryParse(UIManager.instance.battleDeckRingRPText[i].text, out consumeRP))
            {
                if (consumeRP <= rp) UIManager.instance.battleRPNotEnough[i].SetActive(false);
                else UIManager.instance.battleRPNotEnough[i].SetActive(true);
            }
            else
            {
                switch (UIManager.instance.battleDeckRingRPText[i].text)
                {
                    case "20.00":
                    case "20/20":
                        UIManager.instance.battleRPNotEnough[i].SetActive(false);
                        break;
                    default:
                        UIManager.instance.battleRPNotEnough[i].SetActive(true);
                        break;
                }
            }
        }
        if (rp < 10) UIManager.instance.battleRPNotEnough[DeckManager.instance.maxDeckLength].SetActive(true);
        else UIManager.instance.battleRPNotEnough[DeckManager.instance.maxDeckLength].SetActive(false);
    }
}
