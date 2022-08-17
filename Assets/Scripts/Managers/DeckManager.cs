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
    public bool isAngelEffect;     //천사 링의 효과가 유효한지 여부
    public Ring angelRing;  //생성한 천사링
    public int sleepActivated;    //유효한 동면 링의 수
    public int sleepGenerated; //생성한 동면 링의 수
    float plantCoolTime;    //식물 링의 생성 쿨타임
    int plantIdx;       //식물 링의 덱에서의 인덱스(0 이상인 경우만 쿨타임이 돌아간다)
    public int necroCount;         //네크로 링의 사망 카운트
    public int necroIdx;       //네크로 링의 덱에서의 인덱스

    //기타
    int ringLayerMask;  //링 레이어마스크

    void Awake()
    {
        instance = this;

        deck = new List<int>();
        rings = new List<Ring>();

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
            if (plantIdx != -1)
            {
                if (plantCoolTime < 10)
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
    
    //덱을 초기화한다.
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

    //전투 준비한다. 필요한 변수들을 초기화한다.
    public void PrepareBattle()
    {
        rings.Clear();
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
        int deckIdx = deck.IndexOf(genRing.baseRing.id);
        int rpCost;
        if (!int.TryParse(UIManager.instance.battleDeckRingRPText[deckIdx].text, out rpCost)) rpCost = 0;

        //충분한 rp가 있다면 더 자세하게 생성 가능 여부 확인. 아니면 취소
        if (BattleManager.instance.ChangePlayerRP(-rpCost))
        {
            if (genRing.baseRing.id == 17)  //탐욕링이라면 앞으로 생성 불가하게 막음
            {
                UIManager.instance.SetBattleDeckRingRPText(deckIdx, "MAX");
            }
            else if (genRing.baseRing.id == 25)  //돌연변이라면 똑같은 확률로 다른 링으로 변경(천사, 동면 링 제외)
            {
                int mutantIdx;
                do mutantIdx = Random.Range(0, deck.Count);
                while (deck[mutantIdx] == 17 || deck[mutantIdx] == 27 || deck[mutantIdx] == 32);
                genRing.InitializeRing(deck[mutantIdx]);
            }
            else if (genRing.baseRing.id == 27)  //천사링이라면 효과를 키고 앞으로 생성 불가하게 막음
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
                UIManager.instance.SetBattleDeckRingRPText(deckIdx, "0/20");
            }
            else if (genRing.baseRing.id == 32)  //동면링이라면 효과 발동 가능한지 확인하고 3개 생성이면 앞으로 생성 불가하게 막음
            {
                if (++sleepGenerated == 3) UIManager.instance.SetBattleDeckRingRPText(deckIdx, "MAX");
                else UIManager.instance.SetBattleDeckRingRPText(deckIdx, (int)(rpCost * 1.5f));
                sleepActivated++;
            }
            else UIManager.instance.SetBattleDeckRingRPText(deckIdx, (int)(rpCost * 1.5f));  //다음 필요 RP값을 계산
            genRing.PutIntoBattle(ringNumber++);
            rings.Add(genRing);
            GetCommanderNearestForAllRings();
        }
        else GameManager.instance.ReturnRingToPool(genRing);
    }

    //링을 전투에서 제거한다.
    public void RemoveRingFromBattle(Ring ring)
    {
        ring.ApplyRemoveEffect();
        rings.Remove(ring);
        GameManager.instance.ReturnRingToPool(ring);
        GetCommanderNearestForAllRings();
    }

    //덱에 링을 넣는다.
    public bool AddRingToDeck(int ringID)
    {
        if (deck.Count >= maxDeckLength) return false;
        if (deck.Contains(ringID)) return false;
        if (ringID < 0 || ringID >= GameManager.instance.baseRings.Count) return false;
        deck.Add(ringID);
        return true;
    }

    //덱에서 링을 제거한다.
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


    //덱의 모든 링들에 대하여 가장 가까운 사령관 링을 찾는다.
    void GetCommanderNearestForAllRings()
    {
        if (!deck.Contains(19)) return;

        Ring ring;
        List<Ring> commanderList = new List<Ring>();
        for (int i = rings.Count - 1; i >= 0; i--) //모든 링에 대하여 이 링이 가장 가까운 사령관 링이었던 경우 삭제하고, 같은 사령관 링이면 저장한다.
        {
            ring = rings[i];
            if (ring.baseRing.id == 19) commanderList.Add(ring);
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
