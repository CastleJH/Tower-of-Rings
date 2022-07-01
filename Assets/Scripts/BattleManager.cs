using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public bool debugFlag;  //디버그용
    public int debugVariable;

    public List<Monster> monsters; //현재 스테이지의 몬스터들

    //전투 별 변수
    public bool isBattlePlaying;  //게임 진행 중인지 여부
    public bool isBattleOver;       //게임 오버의 여부(몬스터가 엔드라인에 닿았음)
    public int pathID;
    public float rp;

    //페이즈 별 변수
    public int phase;   //현재 페이즈
    private int numGenMonster;  //생성할 몬스터 수
    private int newMonsterID;   //새로 생성할 몬스터 아이디

    void Awake()
    {
        instance = this;
        monsters = new List<Monster>();
    }

    void Update()
    {
        if (debugFlag)
        {
            debugFlag = false;
            StartBattle();
        }
        if (isBattlePlaying)
        {
            //종료 여부 확인
            CheckBattleOver();
        }
    }

    //전투를 시작한다.
    void StartBattle()
    {
        //전투 별 변수
        isBattlePlaying = true;
        isBattleOver = false;
        pathID = 0;
        ChangeCurrentRP(500);

        //덱의 UI(링 스프라이트 및 소모 RP)변경
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)
        {
            UIManager.instance.SetBattleDeckRingImage(i);
            if (i < DeckManager.instance.deck.Count) UIManager.instance.SetBattleDeckRingRPText(i, (int)GameManager.instance.ringstoneDB[DeckManager.instance.deck[i]].baseRP);
            else UIManager.instance.SetBattleDeckRingRPText(i, 0);
        }

        //덱 매니저의 전투 관련 변수들을 초기화
        DeckManager.instance.PrepareBattle();

        //카메라를 해당하는 전장으로 이동
        Camera.main.transform.position = GameManager.instance.monsterPaths[pathID].transform.position;
        Camera.main.transform.Translate(0, -2, -10);

        phase = 1;
        StartPhase();
    }

    //페이즈를 시작한다.
    void StartPhase()
    {
        numGenMonster = 15 * (phase + 1);
        newMonsterID = 0;
        StartCoroutine(GenerateMonster());
    }

    IEnumerator GenerateMonster()
    {
        while (newMonsterID < numGenMonster)
        {
            float scale = 5.0f;
            /*if (GameManager.instance.floor == 7) scale = 4.0f;
            else scale = 0.5f * (GameManager.instance.floor + 1);
            scale += 0.05f * (GameManager.instance.stage - 1);*/

            //몬스터 생성
            Monster monster = GameManager.instance.GetMonsterFromPool();
            monster.gameObject.transform.position = new Vector2(100, 100);
            if (phase == 3 && (newMonsterID == 29 || newMonsterID == 59))
                monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(3, 17)], pathID, scale);
            //else monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(0, 3)], pathID, scale);
            else monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(0, 3)], pathID, scale);
            monster.gameObject.SetActive(true); //몬스터를 킴
            monsters.Add(monster); //리스트에 삽입함
            newMonsterID++;
            yield return new WaitForSeconds(30.0f / numGenMonster);
        }
    }

    //배틀이 종료되었는지 확인한다.
    void CheckBattleOver()
    {
        //현재는 클리어든 오버든 같은 동작을 하도록 한 상태임.
        if ((monsters.Count == 0 && newMonsterID == numGenMonster) || isBattleOver)
        {
            Debug.Log("End Battle");

            //배틀을 종료한다.
            isBattlePlaying = false;

            //몬스터 생성 종료
            StopAllCoroutines();

            //몬스터 리스트 정리
            for (int i = monsters.Count - 1; i >= 0; i--)
                monsters[i].RemoveFromScene(0.0f);
            monsters.Clear();

            //덱 정리
            for (int i = 0; i < DeckManager.instance.rings.Count; i++)
                GameManager.instance.ReturnRingToPool(DeckManager.instance.rings[i]);
            DeckManager.instance.rings.Clear();


            //링 생성/제거 중이었다면 이것도 정리
            if (DeckManager.instance.isEditRing)
            {
                if (DeckManager.instance.genRing != null) GameManager.instance.ReturnRingToPool(DeckManager.instance.genRing);
                else DeckManager.instance.ringRemover.transform.position = new Vector3(100, 100, 0);

            }   
        }
    }

    //현재 RP량을 바꾼다.
    public void ChangeCurrentRP(float _rp)
    {
        rp = _rp;
        UIManager.instance.battleRPText.text = ((int)rp).ToString();
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)
        {
            if (i < DeckManager.instance.deck.Count && int.Parse(UIManager.instance.battleDeckRingRPText[i].text) <= rp)
                UIManager.instance.battleRPNotEnough[i].SetActive(false);
            else UIManager.instance.battleRPNotEnough[i].SetActive(true);
        }
        if (rp < 10) UIManager.instance.battleRPNotEnough[DeckManager.instance.maxDeckLength].SetActive(true);
        else UIManager.instance.battleRPNotEnough[DeckManager.instance.maxDeckLength].SetActive(false);
    }
}
