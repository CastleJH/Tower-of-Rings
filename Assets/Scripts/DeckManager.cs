using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;

    public GameObject ringRemover;

    //�� ���� ����
    public int maxDeckLength = 6;
    public List<int> deck;  //�÷��̾� ��

    //��Ʋ �� ����
    public List<Ring> rings;    //���� ���� ����
    public Ring genRing = null;    //���� ���� ��
    public bool isEditRing;  //�� ����/���Ź�ư�� ���ȴ��� ����
    int ringNumber;    //�� ������ �ο��ϴ� ���� ��ȣ

    //��Ÿ
    int ringLayerMask;
    void Awake()
    {
        instance = this;

        deck = new List<int>();
        rings = new List<Ring>();
        
        InitializeDeck();

        ringLayerMask = 1 << LayerMask.NameToLayer("Ring");
    }

    void Update()
    {
        if (BattleManager.instance.isBattlePlaying)
        {
            if (isEditRing) GetInput(); //�� ������ ���� ���
        }
    }
    
    //���� �ʱ�ȭ�Ѵ�.
    public void InitializeDeck()
    {
        AddRingToDeck(1);
        AddRingToDeck(2);
        AddRingToDeck(3);
        AddRingToDeck(4);
        AddRingToDeck(5);
        AddRingToDeck(6);
        RemoveRingFromDeck(0);
        RemoveRingFromDeck(0);
        RemoveRingFromDeck(0);
        RemoveRingFromDeck(0);
        RemoveRingFromDeck(0);
        RemoveRingFromDeck(0);
        AddRingToDeck(20);
        //AddRingToDeck(7);   //��
        AddRingToDeck(14);  //��
        AddRingToDeck(10);  //Ÿ
        //AddRingToDeck(3);     //ȿ
        AddRingToDeck(19);  //��ɰ�
    }

    //���� �غ��Ѵ�. �ʿ��� �������� �ʱ�ȭ�Ѵ�.
    public void PrepareBattle()
    {
        rings.Clear();
        genRing = null;
        isEditRing = false;
        ringNumber = 0;
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
                genRing.CheckArrangePossible();
            }
            else
            {   
                //�� ���� ǥ���� ��ġ�� �̵���Ų��.
                ringRemover.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                ringRemover.transform.Translate(Vector3.forward * 1);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;
            if (genRing != null)    //�� ������ ���
            {
                //�÷��̾��� ������ ��ġ ������ �ش��ϴ� ���� �����ϴٸ� ���� �����Ѵ�.
                genRing.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                genRing.transform.Translate(Vector3.forward * 11);
                TryPutRingIntoScene();
            }
            else //�� ������ ���
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f, ringLayerMask);
                Ring ring;
                if (hit.collider != null && hit.collider.tag == "Ring" && BattleManager.instance.rp >= 10)//������ ��ġ ������ ���� �ִٸ� �����Ѵ�.
                {
                    ring = hit.collider.gameObject.GetComponent<Ring>();
                    RemoveRingFromBattle(ring);
                    BattleManager.instance.ChangeCurrentRP(BattleManager.instance.rp - 10);
                }
                ringRemover.transform.position = new Vector3(100, 100, 0);
            }
            genRing = null;
            isEditRing = false;
            UIManager.instance.SetBattleArrangeFail(null);
        }
    }

    //���� scene�� ��ġ�Ѵ�.
    public void TryPutRingIntoScene()
    {
        //�ùٸ� ��ġ�� �ƴ� ��� ���
        if (!genRing.CheckArrangePossible())
        {
            GameManager.instance.ReturnRingToPool(genRing);
            return;
        }

        //�Ҹ� rp���� ���
        int deckIdx = deck.IndexOf(genRing.ringBase.id);
        int rpCost = int.Parse(UIManager.instance.battleDeckRingRPText[deckIdx].text);

        //����� rp�� �ִٸ� ����. �ƴϸ� ���
        if (rpCost <= BattleManager.instance.rp)
        {
            genRing.PutIntoBattle(ringNumber++);
            rings.Add(genRing);
            UIManager.instance.SetBattleDeckRingRPText(deckIdx, (int)(rpCost * 1.5f));
            BattleManager.instance.ChangeCurrentRP(BattleManager.instance.rp - rpCost);
        }
        else GameManager.instance.ReturnRingToPool(genRing);
    }

    //���� �������� �����Ѵ�.
    public void RemoveRingFromBattle(Ring ring)
    {
        ring.RemoveFromBattle();
        GameManager.instance.ReturnRingToPool(ring);
    }

    //���� �������� �ִ´�.
    public bool AddRingToDeck(int ringID)
    {
        if (deck.Count >= maxDeckLength) return false;
        if (deck.Contains(ringID)) return false;
        if (ringID < 0 || ringID >= GameManager.instance.ringstoneDB.Count) return false;
        GameManager.instance.ringstoneDB[ringID].level = 0;
        GameManager.instance.ringstoneDB[ringID].Upgrade();
        deck.Add(ringID);
        return true;
    }

    //������ �������� �����Ѵ�.
    public bool RemoveRingFromDeck(int index)
    {
        if (deck.Count > index)
        {
            int ringID = deck[index];
            GameManager.instance.ringstoneDB[ringID].level = 0;
            GameManager.instance.ringstoneDB[ringID].Upgrade();
            deck.RemoveAt(index);
            return true;
        }
        return false;
    }
}
