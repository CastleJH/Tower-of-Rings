using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;

    public GameObject ringRemover;

    //�� ���� ����
    public int maxDeckLength = 6;
    public List<int> deck;          //�÷��̾� ��

    //��Ʋ �� ����
    public List<Ring> rings;        //���� ���� ����
    public Ring genRing = null;     //���� ���� ��
    public bool isEditRing;         //�� ����/���Ź�ư�� ���ȴ��� ����
    int ringNumber;                 //�� ������ �ο��ϴ� ���� ��ȣ
    public bool isAngelEffect;      //õ�� ���� ȿ���� ��ȿ���� ����
    public Ring angelRing;          //������ õ�縵
    public int sleepActivated;      //��ȿ�� ���� ���� ��
    public int sleepGenerated;      //������ ���� ���� ��
    float plantCoolTime;            //�Ĺ� ���� ���� ��Ÿ��
    int plantIdx;                   //�Ĺ� ���� �������� �ε���(0 �̻��� ��츸 ��Ÿ���� ���ư���)
    public int necroCount;          //��ũ�� ���� ��� ī��Ʈ
    public int necroIdx;            //��ũ�� ���� �������� �ε���

    //��Ÿ
    int ringLayerMask;  //�� ���̾��ũ

    void Awake()
    {
        instance = this;

        deck = new List<int>();
        rings = new List<Ring>();

        ringLayerMask = 1 << LayerMask.NameToLayer("Ring");
    }

    void Update()
    {
        if (BattleManager.instance.isBattlePlaying)
        {
            if (isEditRing) GetInput(); //�� ������ ���� ���
            if (plantIdx != -1)         //�Ĺ����� �����ϴ� ���
            {
                if (plantCoolTime < 10) //10�ʸ��� ���������ϰ� �Ѵ�.
                {
                    plantCoolTime += Time.deltaTime;
                    if (plantCoolTime >= 10.0f)
                    {
                        plantCoolTime = 10.0f;
                        UIManager.instance.SetBattleDeckRingRPText(plantIdx, "10.00");
                        BattleManager.instance.ChangePlayerRP(0);
                    }
                    else UIManager.instance.SetBattleDeckRingRPText(plantIdx, string.Format("{0:0.00}", plantCoolTime));
                }
            }
        }
    }
    
    //���� �ʱ�ȭ�Ѵ�.
    public void InitializeDeck()
    {
        RemoveRingFromDeck(0);
        RemoveRingFromDeck(0);
        RemoveRingFromDeck(0);
        RemoveRingFromDeck(0);
        RemoveRingFromDeck(0);
        RemoveRingFromDeck(0);
        AddRingToDeck(0);
        AddRingToDeck(7);
    }

    //���� �غ��Ѵ�. �ʿ��� �������� �ʱ�ȭ�Ѵ�.
    public void PrepareBattle()
    {
        genRing = null;
        isEditRing = false;
        ringNumber = 0;

        isAngelEffect = false;
        angelRing = null;
        sleepActivated = 0;
        sleepGenerated = 0;
        plantCoolTime = 0.0f;
        plantIdx = -1;
        necroCount = 0;
        necroIdx = -1;
    }

    //����� �Է��� �޴´�.
    void GetInput()
    {
        Vector2 touchPos;
        if (Input.touchCount > 1) return;
        if (Input.GetMouseButton(0))    //��ġ(���콺)�� ������ ���� ��
        {
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            if (genRing != null)    //�� ������ ���
            {
                //���� ��ġ�� �̵���Ű�� ��ġ ���� ǥ�ø� �ٲ۴�.
                genRing.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                genRing.transform.Translate(Vector3.forward * 2);
                genRing.CanBePlaced();
            }
            else   //�� ������ ���
            {   
                //�� ���� ǥ���� ��ġ�� �̵���Ų��.
                ringRemover.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                ringRemover.transform.Translate(Vector3.forward * 1);
            }
        }
        else if (Input.GetMouseButtonUp(0))  //��ġ(���콺)�� ������ ��
        {
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            if (genRing != null)    //�� ������ ���
            {
                //�÷��̾��� ������ ��ġ ������ �ش��ϴ� ���� �����ϴٸ� ���� �����Ѵ�.
                genRing.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                genRing.transform.Translate(Vector3.forward * 11);
                TryPutRingIntoBattle();
            }
            else   //�� ������ ���
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f, ringLayerMask);
                Ring ring;
                if (hit.collider != null && hit.collider.tag == "Ring" && BattleManager.instance.rp >= 10) //������ ��ġ ������ ���� �ִٸ� �����Ѵ�.
                {
                    if (BattleManager.instance.ChangePlayerRP(-10))
                    {
                        ring = hit.collider.gameObject.GetComponent<Ring>();
                        RemoveRingFromBattle(ring);
                    }
                }
                ringRemover.transform.position = new Vector3(100, 100, 0);
            }
            genRing = null;
            isEditRing = false;
            UIManager.instance.SetBattleArrangeFail(null);
        }
    }

    //���� scene�� ��ġ�Ѵ�.
    public void TryPutRingIntoBattle()
    {
        //�ùٸ� ��ġ�� �ƴ� ��� ���
        if (!genRing.CanBePlaced())
        {
            GameManager.instance.ReturnRingToPool(genRing);
            return;
        }

        //�Ҹ� rp���� ���
        int deckIdx = deck.IndexOf(genRing.baseRing.id);
        int rpCost;
        if (!int.TryParse(UIManager.instance.battleDeckRingRPText[deckIdx].text, out rpCost)) rpCost = 0;

        //����� rp�� �ִٸ� �� �ڼ��ϰ� ���� ���� ���� Ȯ��. �ƴϸ� ���
        if (BattleManager.instance.ChangePlayerRP(-rpCost))
        {
            if (genRing.baseRing.id == 17)  //Ž�帵�̶�� ������ ���� �Ұ��ϰ� ����
            {
                UIManager.instance.SetBattleDeckRingRPText(deckIdx, "MAX");
            }
            else if (genRing.baseRing.id == 25)  //�������̶�� �Ȱ��� Ȯ���� �ٸ� ������ ����(õ��, ���� �� ����)
            {
                int mutantIdx;
                do mutantIdx = Random.Range(0, deck.Count);
                while (deck[mutantIdx] == 17 || deck[mutantIdx] == 27 || deck[mutantIdx] == 32);
                genRing.InitializeRing(deck[mutantIdx]);
            }
            else if (genRing.baseRing.id == 27)  //õ�縵�̶�� ȿ���� Ű�� ������ ���� �Ұ��ϰ� ����
            {
                UIManager.instance.SetBattleDeckRingRPText(deckIdx, "MAX");
                angelRing = genRing;
                isAngelEffect = true;
            }
            else if (genRing.baseRing.id == 30)
            {
                plantCoolTime = 0.0f;
                plantIdx = deckIdx;
                UIManager.instance.SetBattleDeckRingRPText(deckIdx, "0.00");
            }
            else if (genRing.baseRing.id == 31)
            {
                necroCount = 0;
                necroIdx = deckIdx;
                UIManager.instance.SetBattleDeckRingRPText(deckIdx, "0/10");
            }
            else if (genRing.baseRing.id == 32)  //���鸵�̶�� ȿ�� �ߵ� �������� Ȯ���ϰ� 3�� �����̸� ������ ���� �Ұ��ϰ� ����
            {
                if (++sleepGenerated == 3) UIManager.instance.SetBattleDeckRingRPText(deckIdx, "MAX");
                else UIManager.instance.SetBattleDeckRingRPText(deckIdx, (int)(rpCost * 1.5f));
                sleepActivated++;
            }
            else UIManager.instance.SetBattleDeckRingRPText(deckIdx, (int)(rpCost * 1.3333f));  //���� �ʿ� RP���� ���
            genRing.PutIntoBattle(ringNumber++);
            rings.Add(genRing);
            GetCommanderNearestForAllRings();
        }
        else GameManager.instance.ReturnRingToPool(genRing);
    }

    //���� �������� �����Ѵ�.
    public void RemoveRingFromBattle(Ring ring)
    {
        ring.ApplyRemoveEffect();
        rings.Remove(ring);
        GameManager.instance.ReturnRingToPool(ring);
        GetCommanderNearestForAllRings();
    }

    //���� ���� �ִ´�.
    public bool AddRingToDeck(int ringID)
    {
        if (deck.Count >= maxDeckLength) return false;
        if (deck.Contains(ringID)) return false;
        if (ringID < 0 || ringID >= GameManager.instance.baseRings.Count) return false;
        deck.Add(ringID);
        return true;
    }

    //������ ���� �����Ѵ�.
    public bool RemoveRingFromDeck(int index)
    {
        if (deck.Count > index)
        {
            int ringID = deck[index];
            GameManager.instance.EmptyParticlePool(ringID);
            GameManager.instance.EmptyBulletPool(ringID);
            GameManager.instance.baseRings[ringID].Init();
            deck.RemoveAt(index);
            return true;
        }
        return false;
    }


    //���� ��� ���鿡 ���Ͽ� ���� ����� ��ɰ� ���� ã�´�.
    void GetCommanderNearestForAllRings()
    {
        if (!deck.Contains(19)) return;

        Ring ring;
        List<Ring> commanderList = new List<Ring>();
        for (int i = rings.Count - 1; i >= 0; i--) //��ɰ� ���� ��� ã�´�.
        {
            ring = rings[i];
            ring.commanderNearest = null;
            if (ring.baseRing.id == 19) commanderList.Add(ring);
        }

        if (commanderList.Count == 0) return;   //��ɰ� ���� �ϳ��� ������ ��.

        for (int i = rings.Count - 1; i >= 0; i--) //��� ���� ���Ͽ� ���Ӱ� ��ɰ� ���� ã���ش�.
        {
            ring = rings[i];
            ring.commanderNearest = commanderList[commanderList.Count - 1];
            for (int j = commanderList.Count - 1; j >= 0; j--)
                if (Vector2.Distance(ring.transform.position, commanderList[j].transform.position) < Vector2.Distance(ring.transform.position, ring.commanderNearest.transform.position))
                    ring.commanderNearest = commanderList[j];
        }
    }
}
