using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;

    public GameObject ringRemover;

    //덱 구성 변수
    public int maxDeckLength = 6;
    public List<int> deck;  //플레이어 덱

    //배틀 중 변수
    public List<Ring> rings;    //전투 중인 링들
    public Ring genRing = null;    //생성 중인 링
    public bool isGenRing;  //링 생성버튼이 눌렸는지 여부

    void Awake()
    {
        instance = this;

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
    }

    //사용자 입력을 받는다.
    void GetInput()
    {
        Vector2 touchPos;
        if (Input.touchCount > 1) return;
        if (Input.GetMouseButton(0))    //터치(마우스)가 눌리는 중일 때
        {
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            if (genRing != null)    //링 생성의 경우
            {
                //링의 위치를 이동시키고 배치 가능 표시를 바꾼다.
                genRing.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                genRing.transform.Translate(Vector3.forward * 2);
                genRing.CheckArragePossible();
            }
            else
            {   
                //링 제거 표시의 위치를 이동시킨다.
                ringRemover.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                ringRemover.transform.Translate(Vector3.forward * 1);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;
            if (genRing != null)    //링 생성의 경우
            {
                //플레이어의 마지막 터치 지점에 해당하는 곳에 가능하다면 링을 생성한다.
                genRing.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                genRing.transform.Translate(Vector3.forward * 3);
                TryPutRingIntoScene();
            }
            else //링 제거의 경우
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f);
                Ring ring;
                if (hit.collider != null && hit.collider.tag == "Ring" && BattleManager.instance.rp >= 10)//마지막 터치 지점에 링이 있다면 제거한다.
                {
                    ring = hit.collider.gameObject.GetComponent<Ring>();
                    ring.RemoveSynergy();
                    rings.Remove(ring);
                    GameManager.instance.ReturnRingToPool(ring);
                    BattleManager.instance.ChangeCurrentRP(BattleManager.instance.rp - 10);
                }
                ringRemover.transform.position = new Vector3(100, 100, 0);
            }
            genRing = null;
            isGenRing = false;
            UIManager.instance.SetBattleArrangeFail(null);
        }
    }

    //링을 scene에 배치한다.
    public void TryPutRingIntoScene()
    {
        //올바른 위치가 아닌 경우 취소
        if (!genRing.CheckArragePossible())
        {
            GameManager.instance.ReturnRingToPool(genRing);
            return;
        }

        //소모 rp값을 계산
        int deckIdx = deck.IndexOf(genRing.ringBase.id);
        int rpCost = int.Parse(UIManager.instance.battleDeckRingRPText[deckIdx].text);

        //충분한 rp가 있다면 생성. 아니면 취소
        if (rpCost <= BattleManager.instance.rp)
        {
            genRing.PutRingIntoScene();
            rings.Add(genRing);
            //RenewAllRingsStat();
            UIManager.instance.SetBattleDeckRingRPText(deckIdx, (int)(rpCost * 1.5f));
            BattleManager.instance.ChangeCurrentRP(BattleManager.instance.rp - rpCost);
        }
        else GameManager.instance.ReturnRingToPool(genRing);
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
        return true;
    }
}
