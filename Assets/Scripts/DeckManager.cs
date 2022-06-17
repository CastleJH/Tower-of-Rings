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
    public bool isGenRing;  //�� ������ư�� ���ȴ��� ����

    void Awake()
    {
        instance = this;

        Debug.Log("Set!");
        if (instance == null) Debug.Log("What's wrong?!");
        deck = new List<int>();
        rings = new List<Ring>();
        
        InitializeDeck();
    }

    void Update()
    {
        if (BattleManager.instance.isBattlePlaying)
        {
            if (isGenRing) GetInput(); //�� ������ ���� ���
        }
    }
    
    //���� �ʱ�ȭ�Ѵ�.
    public void InitializeDeck()
    {
        AddToDeck(1);
        AddToDeck(2);
        AddToDeck(3);
        AddToDeck(4);
        AddToDeck(5);
        AddToDeck(0);
        RemoveFromDeck(0);
        RemoveFromDeck(0);
        RemoveFromDeck(0);
        RemoveFromDeck(0);
        RemoveFromDeck(0);
        AddToDeck(7);
        AddToDeck(30);
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
                genRing.CheckArragePossible();
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
                genRing.transform.Translate(Vector3.forward * 3);
                TryPutRingIntoScene();
                genRing.collider.enabled = true;
            }
            else //�� ������ ���
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f);
                if (hit.collider != null && hit.collider.tag == "Ring" && BattleManager.instance.rp >= 10)//������ ��ġ ������ ���� �ִٸ� �����Ѵ�.
                {
                    GameManager.instance.ReturnRingToPool(hit.collider.gameObject.GetComponent<Ring>());
                    BattleManager.instance.ChangeCurrentRP(BattleManager.instance.rp - 10);
                }
                ringRemover.transform.position = new Vector3(100, 100, 0);
            }
            genRing = null;
            isGenRing = false;
            UIManager.instance.SetBattleArrangeFail(null);
        }
    }

    //���� scene�� ��ġ�Ѵ�.
    public void TryPutRingIntoScene()
    {
        //�ùٸ� ��ġ�� �ƴ� ��� ���
        if (!genRing.CheckArragePossible())
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
            genRing.isInBattle = true;
            genRing.rangeRenderer.color = new Color(0, 0, 0, 0);
            rings.Add(genRing);
            //RenewAllRingsStat();
            UIManager.instance.SetBattleDeckRingRPText(deckIdx, (int)(rpCost * 1.5f));
            BattleManager.instance.ChangeCurrentRP(BattleManager.instance.rp - rpCost);
        }
        else GameManager.instance.ReturnRingToPool(genRing);
    }

    //������ ���� �����Ѵ�.
    public bool RemoveFromDeck(int index)
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

    //���� ���� �ִ´�.
    public bool AddToDeck(int ringID)
    {
        if (deck.Count >= maxDeckLength) return false;
        if (deck.Contains(ringID)) return false;
        if (ringID < 0 || ringID >= GameManager.instance.ringstoneDB.Count) return false;
        GameManager.instance.ringstoneDB[ringID].level = 0;
        GameManager.instance.ringstoneDB[ringID].Upgrade();
        deck.Add(ringID);
        return true;
    }
}
