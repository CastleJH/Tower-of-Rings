using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public bool debugFlag;  //����׿�
    public int debugVariable;

    public List<Monster> monsters;  //���� �������� ���͵�
    public List<DropRP> dropRPs;    //���� ������

    //���� �� ����
    public bool isBattlePlaying;    //���� ���� ������ ����
    public float rp;                //���� ���� RP
    public List<int> ringDowngrade; //�̹� �������� �ٿ�׷��̵� �� ����
    float rpGenerateTime;           //���� ���� ���� ���� �� �ð�
    float rpNextGenerateTime;       //���� ���� ���� ���� �ð�
    byte pathAlpha;                 //���� ��� ������
    public bool isBossKilled;       //���� ���� óġ ����

    //���̺� �� ����
    public int wave;                //���� ���̺�
    private int numGenMonster;      //������ ���� ��
    private int newMonsterID;       //���� ������ ������ ���̵�

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
            if (pathAlpha == 255)   //���� ��ΰ� ������ �����ִ� ���
            {
                //���� ���� ���� Ȯ��
                CheckWaveOver();
            }
            else                    //���� ��θ� ������ �Ҵ�.
            {
                pathAlpha += 3;
                GameManager.instance.monsterPathImages[FloorManager.instance.curRoom.pathID].color = new Color32(255, 255, 255, pathAlpha);
                if (pathAlpha == 255) StartWave();  //�� ������ ù��° ���̺긦 �����Ѵ�.
            }

            if (Time.timeScale != 0)    //�Ͻ����� ���� �ƴ϶��
            {
                rpGenerateTime += Time.deltaTime;
                if (rpGenerateTime > rpNextGenerateTime)    //���� �ð����� ���� ������ �����Ѵ�.
                {
                    rpGenerateTime = 0.0f;

                    DropRP dropRP = GameManager.instance.GetDropRPFromPool();
                    dropRP.InitializeDropRP();
                    dropRPs.Add(dropRP);
                    dropRP.gameObject.SetActive(true);

                    //������ ��� ���� ���� ���ο� ���� ���� ���� ���� ������� �ɸ��� �ð��� �����Ѵ�.
                    if (GameManager.instance.baseRelics[18].have)   
                    {
                        if (GameManager.instance.baseRelics[18].isPure) rpNextGenerateTime = Random.Range(4.5f, 6.0f);
                        else rpNextGenerateTime = Random.Range(6.5f, 8.0f);
                    }
                    else rpNextGenerateTime = Random.Range(5.5f, 7.0f);
                }
            }
        }
    }

    //������ �����Ѵ�.
    public void StartBattle()
    {
        //���� �� ���� �ʱ�ȭ
        ringDowngrade.Clear();
        isBossKilled = false;
        pathAlpha = 0;
        rpGenerateTime = 0.0f;
        rpNextGenerateTime = 5.0f;
        rp = 50;
        if (GameManager.instance.baseRelics[19].have)
        {
            if (GameManager.instance.baseRelics[19].isPure) rp = 55;
            else rp = 47;
        }

        //������ Ű�� UI�� �����Ѵ�.
        GameManager.instance.monsterPathImages[FloorManager.instance.curRoom.pathID].color = new Color(255, 255, 255, 0);
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(true);
        UIManager.instance.TurnMapOnOff(false);
        UIManager.instance.OpenBattleDeckPanel();
        ChangePlayerRP(0);

        //�� �Ŵ����� ���� ���� �������� �ʱ�ȭ�Ѵ�.
        DeckManager.instance.PrepareBattle();

        //���̺� 1�� �����Ѵ�.
        wave = 1;
        isBattlePlaying = true;
    }

    //���̺긦 �����Ѵ�. ���� ������ �ʱ�ȭ�ϰ� ���� ���� �ڷ�ƾ�� �����Ѵ�.
    void StartWave()
    {
        //������ ���� ���� �����Ѵ�.
        if (wave == 2) numGenMonster = 20;
        else numGenMonster = 15;
        if (GameManager.instance.baseRelics[5].have)
        {
            if (GameManager.instance.baseRelics[5].isPure) numGenMonster = (int)(numGenMonster * 0.9f);
            else numGenMonster = (int)(numGenMonster * 1.1f);
        }

        //ù��° ���ͺ��� ���� ID�� 0���� ���� �����Ѵ�.
        newMonsterID = 0;
        StartCoroutine(GenerateMonster());
    }

    //���͸� �����Ѵ�.
    IEnumerator GenerateMonster()
    {
        while (newMonsterID < numGenMonster && isBattlePlaying) //�������̰� ���� ��ǥ ���� ����ŭ �������� ���ߴٸ�
        {
            //���� �ɷ�ġ ������ �����Ѵ�.
            float scale = 0.5f * (FloorManager.instance.floor.floorNum + 1) * (1.0f + 0.5f * (wave - 1));

            //���� Ÿ���� �����Ѵ�.
            int monsterType;
            if (wave == 3 && newMonsterID == 0)   //���̺� 3�� ù ���ʹ� �ݵ�� ����Ʈ/����
            {
                if (FloorManager.instance.curRoom.type == 1) monsterType = Random.Range(15, 22);
                else monsterType = FloorManager.instance.floor.floorNum + 21;
            }
            else monsterType = Random.Range(0, 15);    //�׿ܿ��� �Ϲ� ����

            //�����Ѵ�.
            Monster monster = GameManager.instance.GetMonsterFromPool(monsterType);
            monster.gameObject.transform.position = new Vector2(100, 100);  //�ʱ⿡�� �ָ� ����߷����ƾ� path�� �߰����� �۸�ġ ���� ����.
            monster.InitializeMonster(newMonsterID++, FloorManager.instance.curRoom.pathID, scale);
            monster.gameObject.SetActive(true);
            monsters.Add(monster);

            //1�� �� �ٽ� �����Ѵ�.
            yield return new WaitForSeconds(1.0f);
        }
    }

    //���̺갡 ����Ǿ����� Ȯ���Ѵ�.
    void CheckWaveOver()
    {
        if (monsters.Count == 0 && newMonsterID == numGenMonster)   //�����ִ� ���Ͱ� ���� ������ ���ʹ� �� ����������
        {
            if (wave == 3)     //������ ���̺꿴�ٸ� ������ �ְ� ������ �����Ѵ�.
            {
                isBattlePlaying = false;

                //HP�� �����ִ� ä�� �������� �ٽ� ���� �ӵ��� ������� ������.
                if (GameManager.instance.playerCurHP > 0) Time.timeScale = 1;

                //���� �ִ� ���� �ٽ� �ε��Ѵ�.
                SceneChanger.instance.ChangeScene(ReloadBattleRoom, FloorManager.instance.playerX, FloorManager.instance.playerY); 
            }
            else //���� ���̺긦 �ٷ� �����Ѵ�.
            {
                wave++;
                StartWave();
            }
        }
    }

    //���� �ִ� �������� �ٽ� �ε��Ѵ�.
    void ReloadBattleRoom(int x, int y)
    {
        //���ӿ����� �ε带 ����Ѵ�.
        if (GameManager.instance.playerCurHP == 0) return;

        //�ٿ�׷��̵� �� ������ 50% Ȯ���� ����
        for (int i = 0; i < ringDowngrade.Count; i++)
            GameManager.instance.baseRings[ringDowngrade[i]].Upgrade(0.5f);
        ringDowngrade.Clear();

        //���� �������� ����Ѵ�. �������� �� ���� ����Ǿ��ٸ� �� Ÿ���� �ٲ㼭 �� ���¸� ������ �� �ֵ��� �Ѵ�. ��, �������̸� �������� �ʴ´�.
        if (RewardItemDrop() && FloorManager.instance.curRoom.type != 9) FloorManager.instance.curRoom.type = 6;

        //������ ������ �����Ѵ�.
        StopBattleSystem();

        //UI�� �����Ѵ�.
        UIManager.instance.battleArrangeFail.SetActive(false);
        UIManager.instance.ClosePanel(0);
        UIManager.instance.RevealMapAndMoveMarker(FloorManager.instance.playerX, FloorManager.instance.playerY);
        UIManager.instance.TurnMapOnOff(true);

        //���� ���� �ٽ� �ε��Ѵ�(��Ż�� Ų��).
        FloorManager.instance.MoveToRoom(x, y);
    }

    //���� �������� �������� ����Ѵ�. ���� ����ϸ� true, �ƹ��͵� ������� ������ false�� ��ȯ�Ѵ�.
    bool RewardItemDrop()
    {
        bool isItemDrop = false;
        if (FloorManager.instance.curRoom.type != 9)    //�Ϲ� �������� ���
        {
            //��ȭ�� �� �� ��� ���� ȹ������ �����Ѵ�. �� Ȯ���� ���� ���� ���ο� ���� �����Ѵ�.
            float goldProb = 0.9f;
            if (GameManager.instance.baseRelics[10].have)
            {
                if (GameManager.instance.baseRelics[10].isPure) goldProb = 0.8f;
                else goldProb = 2.0f;
            }
            if (Random.Range(0.0f, 1.0f) < goldProb)    //��ȭ ȹ���� ���
            {
                //��� ���(0~2��)
                int goldGet = Random.Range(0, 3);
                if (goldGet != 0) isItemDrop = true;
                for (int i = 0; i < goldGet; i++)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(4, FloorManager.instance.itemPos[i], 0, 0);
                    FloorManager.instance.curRoom.AddItem(item);
                }

                //���̾Ƹ�� ���(0~2��)
                float diamondProb = -1.0f;
                int diamondGet = 0;
                for (int i = 0; i < DeckManager.instance.rings.Count; i++)
                    if (DeckManager.instance.rings[i].baseRing.id == 17)
                    {
                        diamondProb = DeckManager.instance.rings[i].curATK * 0.01f + DeckManager.instance.rings[i].curEFF;
                        break;
                    }
                if (Random.Range(0.0f, 1.0f) <= diamondProb) diamondGet++;
                if (GameManager.instance.baseRelics[15].have && GameManager.instance.baseRelics[15].isPure && Random.Range(0.0f, 1.0f) <= 0.33f) diamondGet++;
                if (diamondGet != 0) isItemDrop = true;
                for (int i = 0; i < diamondGet; i++)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(5, FloorManager.instance.itemPos[i + 3], 0, 0);
                    FloorManager.instance.curRoom.AddItem(item);
                }
            }
            else    //�� ȹ���� ���
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
        else  //�������� ���
        {
            if (isBossKilled && (!GameManager.instance.baseRelics[15].have || GameManager.instance.baseRelics[15].isPure))    //������ óġ�߰� ������ ���� ���� ��� �Ұ� ���°� �ƴϸ�
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
        return isItemDrop;
    }

    //������ �����Ѵ�. ���ݱ��� ������ ���� ����, ��, ���� ���� ��� ������Ʈ Ǯ�� �ǵ�����, ���� ��θ� ����.
    public void StopBattleSystem()
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
        for (int i = monsters.Count - 1; i >= 0; i--)
            GameManager.instance.ReturnMonsterToPool(monsters[i]);
        monsters.Clear();

        //���� ��� ����
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(false);
    }

    //���� RP���� �ٲ۴�. ���濡 �����ϸ� true, �����ϸ� false�� ��ȯ�Ѵ�.
    public bool ChangePlayerRP(float _rp)
    {
        if (rp + _rp < 0) return false; //�ٲ� ����� ������ �Ǹ� �ȵȴ�.
        
        //���� �����ϰ� UI�� �����Ѵ�.
        rp += _rp;
        UIManager.instance.battleHaveRPText.text = ((int)rp).ToString();

        //RP�� ���� ��, �� ���� ������ ���� ������ ���� ��ư �������� ��Ȱ��ȭ�Ѵ�.
        int consumeRP;
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)
        {
            if (int.TryParse(UIManager.instance.battleDeckRingRPText[i].text, out consumeRP))   //�ܼ� RP �Ҹ� ������� �����ϴ� ���
            {
                if (consumeRP <= rp) UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(false); //����� RP�� �ִ� ��츸 ��Ȱ��ȭ
                else UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(true);
            }
            else   //�ִ� �����ѵ��� �����߰ų� Ư�� ���ǿ� ���� �����ϴ� ���
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

        //RP�� ���� ��, ����� RP�� �ִ� ��츸 �� ���� ��ư �������� ��Ȱ��ȭ�Ѵ�.
        UIManager.instance.battleDeckRPNotEnoughCover[DeckManager.instance.maxDeckLength].SetActive(rp < 10);

        return true;
    }
}
