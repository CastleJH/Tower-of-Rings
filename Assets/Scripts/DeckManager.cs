using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;

    public bool debugFlag;  //디버그용
    public int debugVariable;

    public GameObject ringRemover;

    //덱 구성 변수
    public int maxDeckLength = 6;
    public List<int> deck;  //플레이어 덱

    //배틀 중 변수
    public List<Ring> rings;    //전투 중인 링들
    public Ring genRing = null;    //생성 중인 링
    public bool isEditRing;  //링 생성/제거버튼이 눌렸는지 여부
    int ringNumber;    //링 생성시 부여하는 구분 번호

    //기타
    int ringLayerMask;  //링 레이어마스크

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
        if (debugFlag)
        {
            debugFlag = false;
        }
        if (BattleManager.instance.isBattlePlaying)
        {
            if (isEditRing) GetInput(); //링 생성이 눌린 경우
        }
    }
    
    //덱을 초기화한다.
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
        AddRingToDeck(7);   //공
        AddRingToDeck(14);  //속
        AddRingToDeck(10);  //타
        AddRingToDeck(3);     //효
        AddRingToDeck(19);  //사령관
        AddRingToDeck(23);
    }

    //전투 준비한다. 필요한 변수들을 초기화한다.
    public void PrepareBattle()
    {
        rings.Clear();
        genRing = null;
        isEditRing = false;
        ringNumber = 0;
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
                genRing.CanBePlaced();
            }
            else   //링 제거의 경우
            {   
                //링 제거 표시의 위치를 이동시킨다.
                ringRemover.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                ringRemover.transform.Translate(Vector3.forward * 1);
            }
        }
        else if (Input.GetMouseButtonUp(0))  //터치(마우스)가 끝났을 때
        {
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            if (genRing != null)    //링 생성의 경우
            {
                //플레이어의 마지막 터치 지점에 해당하는 곳에 가능하다면 링을 생성한다.
                genRing.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
                genRing.transform.Translate(Vector3.forward * 11);
                TryPutRingIntoBattle();
            }
            else   //링 제거의 경우
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f, ringLayerMask);
                Ring ring;
                if (hit.collider != null && hit.collider.tag == "Ring" && BattleManager.instance.rp >= 10) //마지막 터치 지점에 링이 있다면 제거한다.
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

    //링을 scene에 배치한다.
    public void TryPutRingIntoBattle()
    {
        //올바른 위치가 아닌 경우 취소
        if (!genRing.CanBePlaced())
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
            genRing.PutIntoBattle(ringNumber++);
            rings.Add(genRing);
            GetCommanderNearestForAllRings();
            UIManager.instance.SetBattleDeckRingRPText(deckIdx, (int)(rpCost * 1.5f));  //다음 필요 RP값을 계산한다.
            BattleManager.instance.ChangeCurrentRP(BattleManager.instance.rp - rpCost);
        }
        else GameManager.instance.ReturnRingToPool(genRing);
    }

    //링을 전투에서 제거한다.
    public void RemoveRingFromBattle(Ring ring)
    {
        ring.RemoveFromBattle();
        rings.Remove(ring);
        GameManager.instance.ReturnRingToPool(ring);
        GetCommanderNearestForAllRings();
    }

    //덱에 링스톤을 넣는다.
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

    //덱에서 링스톤을 제거한다.
    public bool RemoveRingFromDeck(int index)
    {
        if (deck.Count > index)
        {
            int ringID = deck[index];
            GameManager.instance.EmptyParticlePool(ringID);
            GameManager.instance.EmptyBulletPool(ringID);
            GameManager.instance.ringstoneDB[ringID].level = 0;
            GameManager.instance.ringstoneDB[ringID].Upgrade();
            deck.RemoveAt(index);
            return true;
        }
        return false;
    }


    //덱의 모든 링들에 대하여 가장 가까운 사령관 링을 찾는다.
    void GetCommanderNearestForAllRings()
    {
        if (!deck.Contains(19)) return;

        Ring ring;
        List<Ring> commanderList = new List<Ring>();
        for (int i = rings.Count - 1; i >= 0; i--) //모든 링에 대하여 이 링이 가장 가까운 사령관 링이었던 경우 삭제하고, 같은 사령관 링이면 저장한다.
        {
            ring = rings[i];
            if (ring.ringBase.id == 19) commanderList.Add(ring);
        }

        if (commanderList.Count == 0) return;

        for (int i = rings.Count - 1; i >= 0; i--) //모든 링에 대하여 새롭게 사령관 링을 찾아준다.
        {
            ring = rings[i];
            ring.commanderNearest = commanderList[commanderList.Count - 1];
            for (int j = commanderList.Count - 1; j >= 0; j--)
                if (Vector2.Distance(ring.transform.position, commanderList[j].transform.position) < Vector2.Distance(ring.transform.position, ring.commanderNearest.transform.position))
                    ring.commanderNearest = commanderList[j];
        }
    }
}
