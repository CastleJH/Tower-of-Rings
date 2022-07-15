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

        phase = 1;
        StartPhase();
    }

    //����� �����Ѵ�. ���� ������ �ʱ�ȭ�ϰ� ���� ���� �ڷ�ƾ�� �����Ѵ�.
    void StartPhase()
    {
        if (phase == 2) numGenMonster = 45;
        else numGenMonster = 30;
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
            if (FloorManager.instance.floor.floorNum == 7) scale = 4.0f;
            else scale = 0.5f * (FloorManager.instance.floor.floorNum + 1);
            scale += 0.05f * (phase - 1);

            //���߿� �����ϼ���
            scale = 1.0f;

            //���� ����
            Monster monster = GameManager.instance.GetMonsterFromPool();
            monster.gameObject.transform.position = new Vector2(100, 100);  //�ʱ⿡�� �ָ� ����߷����ƾ� path�� �߰����� �۸�ġ ���� ����.
            if (phase == 3 && newMonsterID == 0)   //������ 3�� ù ���ʹ� �ݵ�� ����Ʈ/����
            {
                if (FloorManager.instance.curRoom.type == 1) monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(3, 10)], FloorManager.instance.curRoom.pathID, scale);
                //if (FloorManager.instance.curRoom.type == 1) monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[9], FloorManager.instance.curRoom.pathID, scale);
                else monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(10, 17)], FloorManager.instance.curRoom.pathID, scale);
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
            if (phase == 3)     //������ ������ٸ� ������ �ְ� ������ �����Ѵ�.
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
                    if (DeckManager.instance.rings[i].ringBase.id == 17)
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
                phase++;
                StartPhase();
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
                    case "10.00":
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
