using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;

    //�� ���� ����
    public int maxDeckLength = 6;
    public List<int> deck;  //�÷��̾� ��
    public TextMeshProUGUI[] ingameRingRPTexts; //�� rp �ؽ�Ʈ

    //��Ʋ �� ����
    public List<Ring> rings;    //���� ���� ����
    public Ring genRing = null;    //���� ���� ��
    public bool isGenRing;  //�� ������ư�� ���ȴ��� ����

    void Awake()
    {
        instance = this;
        deck = new List<int>();
        rings = new List<Ring>();
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
        if (Input.GetMouseButton(0))
        {
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            if (genRing != null)
            {
                genRing.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                genRing.transform.Translate(Vector3.forward * 2);
                genRing.CheckArragePossible();
            }
            else
            {

            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            //�÷��̾��� ������ ��ġ ������ �ش��ϴ� ���� �����ϴٸ� ���� �����Ѵ�.
            if (genRing != null)
            {
                genRing.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                genRing.transform.Translate(Vector3.forward * 2);
                if (genRing.CheckArragePossible())
                {
                    genRing.isInBattle = true;
                    genRing.rangeRenderer.color = new Color(0, 0, 0, 0);
                    rings.Add(genRing);
                }
                else
                {
                    GameManager.instance.ReturnRingToPool(genRing);
                }
                genRing.collider.enabled = true;
            }
            else
            {

            }
            genRing = null;
            isGenRing = false;
        }
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
            for (int i = 0; i <= deck.Count; i++)
            {
                UIManager.instance.SetBattleDeckRingImage(i);
                UIManager.instance.SetBattleDeckRingRPText(i);
            }
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
        UIManager.instance.SetBattleDeckRingImage(deck.Count - 1);
        UIManager.instance.SetBattleDeckRingRPText(deck.Count - 1);
        return true;
    }
}
