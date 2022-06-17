using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;

    //덱 구성 변수
    public int maxDeckLength = 6;
    public List<int> deck;  //플레이어 덱
    public TextMeshProUGUI[] ingameRingRPTexts; //덱 rp 텍스트

    //배틀 중 변수
    public List<Ring> rings;    //전투 중인 링들
    public Ring genRing = null;    //생성 중인 링
    public bool isGenRing;  //링 생성버튼이 눌렸는지 여부

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
            if (isGenRing) GetInput(); //링 생성이 눌린 경우
        }
    }
    
    //덱을 초기화한다.
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

    //사용자 입력을 받는다.
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

            //플레이어의 마지막 터치 지점에 해당하는 곳에 가능하다면 링을 생성한다.
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

    //덱에서 링을 제거한다.
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

    //덱에 링을 넣는다.
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
