using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public bool debugFlag;  //����׿�
    public int debugVariable;

    public List<Monster> monsters; //���� ���������� ���͵�
    public List<DropRP> dropRPs;

    //���� �� ����
    public bool isBattlePlaying;  //���� ���� ������ ����
    public float rp;        //���� ���� RP
    public List<int> ringDowngrade;
    float rpGenerateTime;
    float rpNextGenerateTime;
    byte pathAlpha;
    public bool isBossKilled;

    //���̺� �� ����
    public int wave;   //���� ���̺�
    private int numGenMonster;  //������ ���� ��
    private int newMonsterID;   //���� ������ ���� ���̵�

    void Awake()
    {
        instance = this;
        monsters = new List<Monster>();
        dropRPs = new List<DropRP>();
        ringDowngrade = new List<int>();
    }

    public void ResetBattleSystem()
    {
        isBattlePlaying = false;

        StopAllCoroutines();

        //���� ���� ����
        for (int i = dropRPs.Count - 1; i >= 0; i--)
            GameManager.instance.ReturnDropRPToPool(dropRPs[i]);
        dropRPs.Clear();

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

        //���� ����
        for (int i = monsters.Count - 1; i >= 0; i--) GameManager.instance.ReturnMonsterToPool(monsters[i]);
        monsters.Clear();

        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(false);
    }

    void Update()
    {
        if (isBattlePlaying)    //���� ���� ���
        {
            if (pathAlpha == 255)
            {
                //���� ���� ���� Ȯ��
                CheckBattleOver();
            }
            else
            {
                pathAlpha += 3;
                GameManager.instance.monsterPathImages[FloorManager.instance.curRoom.pathID].color = new Color32(255, 255, 255, pathAlpha);
                if (pathAlpha == 255) StartWave();
            }

            if (Time.timeScale != 0)
            {
                rpGenerateTime += Time.deltaTime;
                if (rpGenerateTime > rpNextGenerateTime)
                {
                    rpGenerateTime = 0.0f;
                    if (GameManager.instance.baseRelics[18].have)
                    {
                        if (GameManager.instance.baseRelics[18].isPure) rpNextGenerateTime = Random.Range(4.5f, 6.0f);
                        else rpNextGenerateTime = Random.Range(6.5f, 8.0f);
                    }
                    else rpNextGenerateTime = Random.Range(5.5f, 7.0f);
                    DropRP dropRP = GameManager.instance.GetDropRPFromPool();
                    dropRP.InitializeDropRP();
                    dropRPs.Add(dropRP);
                    dropRP.gameObject.SetActive(true);
                }
            }
        }
    }

    //������ �����Ѵ�.
    public void StartBattle()
    {
        //���� �� ���� �ʱ�ȭ
        ringDowngrade.Clear();
        rpGenerateTime = 3.0f;
        isBossKilled = false;
        if (GameManager.instance.baseRelics[18].have)
        {
            if (GameManager.instance.baseRelics[18].isPure) rpNextGenerateTime = Random.Range(9.0f, 12.0f);
            else rpNextGenerateTime = Random.Range(13.0f, 16.0f);
        }
        else rpNextGenerateTime = Random.Range(11.0f, 14.0f); 
        pathAlpha = 0;

        //������ Ų��.
        UIManager.instance.TurnMapOnOff(false);
        GameManager.instance.monsterPathImages[FloorManager.instance.curRoom.pathID].color = new Color(255, 255, 255, 0);
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(true);
        UIManager.instance.OpenBattleDeckPanel();

        //�ʱ� RP�� ���ϰ� UI�� ������Ʈ�Ѵ�.
        rp = 50;
        if (GameManager.instance.baseRelics[19].have)
        {
            if (GameManager.instance.baseRelics[19].isPure) rp = 55;
            else rp = 47;
        }
        ChangePlayerRP(0);

        //�� �Ŵ����� ���� ���� �������� �ʱ�ȭ�Ѵ�.
        DeckManager.instance.PrepareBattle();

        wave = 1;
        isBattlePlaying = true;
    }

    //���̺긦 �����Ѵ�. ���� ������ �ʱ�ȭ�ϰ� ���� ���� �ڷ�ƾ�� �����Ѵ�.
    void StartWave()
    {
        if (wave == 2) numGenMonster = 20;
        else numGenMonster = 15;
        if (GameManager.instance.baseRelics[5].have)
        {
            if (GameManager.instance.baseRelics[5].isPure) numGenMonster = (int)(numGenMonster * 0.9f);
            else numGenMonster = (int)(numGenMonster * 1.1f);
        }
        newMonsterID = 0;
        StartCoroutine(GenerateMonster());
    }

    //���͸� �����Ѵ�.
    IEnumerator GenerateMonster()
    {
        //Debug.Log("Start Coroutine");
        while (newMonsterID < numGenMonster && isBattlePlaying)
        {
            Debug.Log("gen!");
            //���� �ɷ�ġ ������ �����Ѵ�.
            float scale;
            scale = 0.5f * (FloorManager.instance.floor.floorNum + 1) * (1.0f + 0.5f * (wave - 1));

            //���� ����
            int monsterType;
            if (wave == 3 && newMonsterID == 0)   //���̺� 3�� ù ���ʹ� �ݵ�� ����Ʈ/����
            {
                if (FloorManager.instance.curRoom.type == 1) monsterType = Random.Range(15, 22);
                else monsterType = FloorManager.instance.floor.floorNum + 21;
            }
            else monsterType = Random.Range(0, 15);    //�׿ܿ��� �Ϲ� ����

            Monster monster = GameManager.instance.GetMonsterFromPool(monsterType);
            monster.gameObject.transform.position = new Vector2(100, 100);  //�ʱ⿡�� �ָ� ����߷����ƾ� path�� �߰����� �۸�ġ ���� ����.
            monster.InitializeMonster(newMonsterID, FloorManager.instance.curRoom.pathID, scale);
            monster.gameObject.SetActive(true);
            monsters.Add(monster);
            newMonsterID++;
            yield return new WaitForSeconds(1.0f);
        }
    }

    //��Ʋ�� ����Ǿ����� Ȯ���Ѵ�.
    void CheckBattleOver()
    {
        if (monsters.Count == 0 && newMonsterID == numGenMonster)
        {
            if (wave == 3)     //������ ���̺꿴�ٸ� ������ �ְ� ������ �����Ѵ�.
            {
                Debug.Log("NO!!!");
                if (GameManager.instance.playerCurHP > 0) Time.timeScale = 1;

                //��Ʋ�� �����Ѵ�.
                isBattlePlaying = false;

                //���� ���� ����
                StopAllCoroutines();

                SceneChanger.instance.ChangeScene(ReloadBattleRoom, FloorManager.instance.playerX, FloorManager.instance.playerY);
            }
            else
            {
                wave++;
                StartWave();
            }
        }
    }

    void ReloadBattleRoom(int x, int y)
    {
        if (GameManager.instance.playerCurHP == 0) return;

        //���� ���� ����
        for (int i = dropRPs.Count - 1; i >= 0; i--)
            GameManager.instance.ReturnDropRPToPool(dropRPs[i]);
        dropRPs.Clear();

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
            GameManager.instance.baseRings[ringDowngrade[i]].Upgrade(0.5f);

        //���� ���� �� �������� ����Ѵ�.
        bool isItemDrop = false;

        if (FloorManager.instance.curRoom.type != 9)
        {
            float goldProb = 0.9f;
            if (GameManager.instance.baseRelics[10].have)
            {
                if (GameManager.instance.baseRelics[10].isPure) goldProb = 0.8f;
                else goldProb = 2.0f;
            }
            if (Random.Range(0.0f, 1.0f) < goldProb)
            {
                //��� ���
                int goldGet = Random.Range(0, 3);
                //���켼��!
                //goldGet = 2;
                if (goldGet != 0) isItemDrop = true;
                for (int i = 0; i < goldGet; i++)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(4, FloorManager.instance.itemPos[i], 0, 0);
                    FloorManager.instance.curRoom.AddItem(item);
                }

                //���̾Ƹ�� ���
                int diamondGet = 0;
                if (greedyATK != -1.0f && Random.Range(0.0f, 1.0f) <= greedyATK * 0.01f + greedyEFF) diamondGet++;
                if (GameManager.instance.baseRelics[15].have && GameManager.instance.baseRelics[15].isPure && Random.Range(0.0f, 1.0f) <= 0.33f) diamondGet++;
                //���켼��!
                //diamondGet = 2;
                if (diamondGet != 0) isItemDrop = true;
                for (int i = 0; i < diamondGet; i++)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(5, FloorManager.instance.itemPos[i + 3], 0, 0);
                    FloorManager.instance.curRoom.AddItem(item);
                }
            }
            else
            {
                int itemID;
                Item item = GameManager.instance.GetItemFromPool();
                do itemID = Random.Range(0, GameManager.instance.baseRings.Count);
                while (DeckManager.instance.deck.Contains(itemID));
                item.InitializeItem(1000 + itemID, Vector3.forward, 0, 0);
                FloorManager.instance.curRoom.AddItem(item);
                isItemDrop = true;
            }
        }
        else
        {
            //���� ���̾��� ������ óġ������ ���� ���
            if (!GameManager.instance.baseRelics[15].have || GameManager.instance.baseRelics[15].isPure)
            {
                if (isBossKilled)
                {
                    int itemID;
                    Item item = GameManager.instance.GetItemFromPool();
                    do itemID = Random.Range(0, GameManager.instance.baseRelics.Count);
                    while (GameManager.instance.relics.Contains(itemID));
                    item.InitializeItem(2000 + itemID, FloorManager.instance.itemPos[3], 0, 0);
                    FloorManager.instance.curRoom.AddItem(item);
                    isItemDrop = true;
                }
            }
        }

        //�������� �ѹ��̶� ����Ǿ��ٸ� �� Ÿ���� �ٲ۴�.
        if (isItemDrop && FloorManager.instance.curRoom.type != 9) FloorManager.instance.curRoom.type = 6;

        //������ ���� ���� �����ϰ� ��Ż�� �����ش�.
        UIManager.instance.battleArrangeFail.SetActive(false);
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(false);
        UIManager.instance.ClosePanel(0);
        UIManager.instance.RevealMapAndMove(FloorManager.instance.playerX, FloorManager.instance.playerY);
        UIManager.instance.TurnMapOnOff(true);

        FloorManager.instance.MoveToRoom(x, y);
    }

    //���� RP���� �ٲ۴�.
    public bool ChangePlayerRP(float _rp)
    {
        if (rp + _rp < 0) return false;
        rp += _rp;
        UIManager.instance.battleHaveRPText.text = ((int)rp).ToString();
        int consumeRP;
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)    //RP�� ���� �� ���� ������ ���� ��ư Ȱ��ȭ(=��ư ������ ��Ȱ��ȭ)
        {
            if (int.TryParse(UIManager.instance.battleDeckRingRPText[i].text, out consumeRP))
            {
                if (consumeRP <= rp) UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(false);
                else UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(true);
            }
            else
            {
                switch (UIManager.instance.battleDeckRingRPText[i].text)
                {
                    case "20.00":
                    case "20/20":
                        UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(false);
                        break;
                    default:
                        UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(true);
                        break;
                }
            }
        }
        if (rp < 10) UIManager.instance.battleDeckRPNotEnoughCover[DeckManager.instance.maxDeckLength].SetActive(true);
        else UIManager.instance.battleDeckRPNotEnoughCover[DeckManager.instance.maxDeckLength].SetActive(false);

        return true;
    }
}
