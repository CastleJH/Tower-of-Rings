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
    public int goldGet;
    public int diamondGet;
    public List<int> ringDowngrade;
    float rpGenerateTime;
    float rpNextGenerateTime;
    byte pathAlpha;

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

    void Update()
    {
        if (isBattlePlaying)    //���� ���� ���
        {
            //���� ���� ���� Ȯ��
            CheckBattleOver();
            
            if (pathAlpha < 255)
            {
                pathAlpha += 3;
                GameManager.instance.monsterPathImages[FloorManager.instance.curRoom.pathID].color = new Color32(255, 255, 255, pathAlpha);
                if (pathAlpha == 255) StartWave();
            }

            if (Time.timeScale != 0)
            {
                //rpGenerateTime += Time.unscaledDeltaTime;
                rpGenerateTime += Time.deltaTime;
                if (rpGenerateTime > rpNextGenerateTime)
                {
                    rpGenerateTime = 0.0f;
                    rpNextGenerateTime = Random.Range(10.0f, 14.0f);
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
        goldGet = 0;
        diamondGet = 0;
        ringDowngrade.Clear();
        rpGenerateTime = 0.0f;
        rpNextGenerateTime = Random.Range(10.0f, 14.0f);
        pathAlpha = 0;

        //������ Ų��.
        UIManager.instance.TurnMapOnOff(false);
        GameManager.instance.monsterPathImages[FloorManager.instance.curRoom.pathID].color = new Color(255, 255, 255, 0);
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(true);
        UIManager.instance.OpenBattleDeckPanel();

        //�ʱ� RP�� ���ϰ� UI�� ������Ʈ�Ѵ�.
        rp = 50;
        ChangePlayerRP(0);

        //�� �Ŵ����� ���� ���� �������� �ʱ�ȭ�Ѵ�.
        DeckManager.instance.PrepareBattle();

        wave = 1;
        newMonsterID = 0;
        if (GameManager.instance.baseRelics[5].have)
        {
            if (GameManager.instance.baseRelics[5].isPure) numGenMonster = 27;
            else numGenMonster = 33;
        }
        else numGenMonster = 30;
        isBattlePlaying = true;
    }

    //���̺긦 �����Ѵ�. ���� ������ �ʱ�ȭ�ϰ� ���� ���� �ڷ�ƾ�� �����Ѵ�.
    void StartWave()
    {
        if (GameManager.instance.baseRelics[5].have)
        {
            if (GameManager.instance.baseRelics[5].isPure)
            {
                if (wave == 2) numGenMonster = 40;
                else numGenMonster = 27;
            }
            else
            {
                if (wave == 2) numGenMonster = 50;
                else numGenMonster = 33;
            }
        }
        else
        {
            if (wave == 2) numGenMonster = 45;
            else numGenMonster = 30;
        }
        newMonsterID = 0;
        StartCoroutine(GenerateMonster());
    }

    //���͸� �����Ѵ�.
    IEnumerator GenerateMonster()
    {
        Debug.Log("Start Coroutine");
        while (newMonsterID < numGenMonster)
        {
            Debug.Log("gen!");
            //���� �ɷ�ġ ������ �����Ѵ�.
            float scale;
            scale = 0.5f * (FloorManager.instance.floor.floorNum + 1) + 0.05f * (wave - 1);

            //���� ����
            int monsterType;
            if (wave == 3 && newMonsterID == 0)   //���̺� 3�� ù ���ʹ� �ݵ�� ����Ʈ/����
            {
                if (FloorManager.instance.curRoom.type == 1) monsterType = Random.Range(15, 22);
                else monsterType = FloorManager.instance.floor.floorNum + 21;
            }
            else monsterType = Random.Range(0, 15);    //�׿ܿ��� �Ϲ� ����

            //���켼��!
            //monsterType = 10;

            Monster monster = GameManager.instance.GetMonsterFromPool(monsterType);
            monster.gameObject.transform.position = new Vector2(100, 100);  //�ʱ⿡�� �ָ� ����߷����ƾ� path�� �߰����� �۸�ġ ���� ����.
            monster.InitializeMonster(newMonsterID, FloorManager.instance.curRoom.pathID, scale);
            monster.gameObject.SetActive(true);
            monsters.Add(monster);
            newMonsterID++;
            yield return new WaitForSeconds(45.0f / numGenMonster); //45�� ���� ���͵��� ������Ѿ� �Ѵ�. ���� ���� ����Ͽ� ���� �ֱ⸦ �����Ѵ�.
        }
    }

    //��Ʋ�� ����Ǿ����� Ȯ���Ѵ�.
    void CheckBattleOver()
    {
        if (monsters.Count == 0 && newMonsterID == numGenMonster)
        {
            if (wave == 3)     //������ ���̺꿴�ٸ� ������ �ְ� ������ �����Ѵ�.
            {
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
        //FloorManager.instance.ChangeCurRoomToIdle();

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

        diamondGet = 0;
        if (greedyATK != -1.0f)   //Ž�帵�� �����ߴٸ� ������ �ø�
        {
            goldGet += (int)(goldGet * greedyATK * 0.01f);
            if (Random.Range(0.0f, 1.0f) <= greedyATK * 0.01f + greedyEFF) diamondGet = Random.Range(3, 6);
        }
        GameManager.instance.ChangeGold(goldGet);
        GameManager.instance.ChangeDiamond(diamondGet);

        //������ ���� ���� �����ϰ� ��Ż�� �����ش�.
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
