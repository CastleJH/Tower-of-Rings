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
    public float rp;        //현재 보유 RP
    public int goldGet;
    public int emeraldGet;

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
        if (isBattlePlaying)    //전투 중인 경우
        {
            //전투 종료 여부 확인
            CheckBattleOver();
        }
    }

    //전투를 시작한다.
    public void StartBattle()
    {
        //전투 별 변수 초기화
        isBattlePlaying = true;
        goldGet = 0;
        emeraldGet = 0;

        //전장을 킨다.
        UIManager.instance.TurnMapOnOff(false);
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(true);
        UIManager.instance.TurnDeckOnOff(true);

        //덱의 이미지/RP 비용등을 초기화한다.
        UIManager.instance.SetBattleDeckRingImageAndRPAll();

        //초기 RP를 정하고 UI를 업데이트한다.
        ChangeCurrentRP(500);   //원활한 테스트를 위해 초기 RP를 넉넉하게 줬다. 실제로는 보유 유물 여부등에 따라 80 근처에서 시작함.

        //덱 매니저의 전투 관련 변수들을 초기화한다.
        DeckManager.instance.PrepareBattle();

        phase = 1;
        StartPhase();
    }

    //페이즈를 시작한다. 관련 변수를 초기화하고 몬스터 생성 코루틴을 시작한다.
    void StartPhase()
    {
        if (phase == 2) numGenMonster = 45;
        else numGenMonster = 30;
        newMonsterID = 0;
        StartCoroutine(GenerateMonster());
    }

    //몬스터를 생성한다.
    IEnumerator GenerateMonster()
    {
        while (newMonsterID < numGenMonster)
        {
            //몬스터 능력치 배율을 조정한다.
            float scale;
            if (FloorManager.instance.floor.floorNum == 7) scale = 4.0f;
            else scale = 0.5f * (FloorManager.instance.floor.floorNum + 1);
            scale += 0.05f * (phase - 1);

            //나중에 삭제하세요
            scale = 1.0f;

            //몬스터 생성
            Monster monster = GameManager.instance.GetMonsterFromPool();
            monster.gameObject.transform.position = new Vector2(100, 100);  //초기에는 멀리 떨어뜨려놓아야 path의 중간에서 글리치 하지 않음.
            if (phase == 3 && newMonsterID == 0)   //페이즈 3의 첫 몬스터는 반드시 엘리트/보스
            {
                if (FloorManager.instance.curRoom.type == 1) monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(3, 10)], FloorManager.instance.curRoom.pathID, scale);
                //if (FloorManager.instance.curRoom.type == 1) monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[9], FloorManager.instance.curRoom.pathID, scale);
                else monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(10, 17)], FloorManager.instance.curRoom.pathID, scale);
            }
            else monster.InitializeMonster(newMonsterID, GameManager.instance.monsterDB[Random.Range(0, 3)], FloorManager.instance.curRoom.pathID, scale);    //그외에는 일반 몬스터
            monster.gameObject.SetActive(true);
            monsters.Add(monster);
            newMonsterID++;
            yield return new WaitForSeconds(30.0f / numGenMonster); //30초 동안 몬스터들을 등장시켜야 한다. 몬스터 수에 비례하여 생성 주기를 결정한다.
        }
    }

    //배틀이 종료되었는지 확인한다.
    void CheckBattleOver()
    {
        if (monsters.Count == 0 && newMonsterID == numGenMonster)
        {
            if (phase == 3)     //마지막 페이즈였다면 보상을 주고 전투를 종료한다.
            {
                //배틀을 종료한다.
                isBattlePlaying = false;

                //몬스터 생성 종료
                StopAllCoroutines();

                //몬스터 리스트 정리
                /*for (int i = monsters.Count - 1; i >= 0; i--)
                    monsters[i].RemoveFromBattle(0.0f);
                monsters.Clear();*/

                float greedyATK = -1.0f;
                float greedyEFF = 0.0f;

                //덱 정리
                for (int i = 0; i < DeckManager.instance.rings.Count; i++)
                {
                    if (DeckManager.instance.rings[i].ringBase.id == 17)
                    {
                        greedyATK = DeckManager.instance.rings[i].curATK;
                        greedyEFF = DeckManager.instance.rings[i].curEFF;
                    }
                    GameManager.instance.ReturnRingToPool(DeckManager.instance.rings[i]);
                }
                DeckManager.instance.rings.Clear();

                //링 생성/제거 중이었다면 이것도 정리
                if (DeckManager.instance.isEditRing)
                {
                    if (DeckManager.instance.genRing != null) GameManager.instance.ReturnRingToPool(DeckManager.instance.genRing);
                    else DeckManager.instance.ringRemover.transform.position = new Vector3(100, 100, 0);
                }

                emeraldGet = 0;
                if (greedyATK != -1.0f)   //탐욕링이 존재했다면 보상을 늘림
                {
                    goldGet += (int)(goldGet * greedyATK * 0.01f);
                    if (Random.Range(0.0f, 1.0f) <= greedyATK * 0.01f + greedyEFF) emeraldGet = Random.Range(3, 6);
                }
                GameManager.instance.ChangeGold(goldGet);
                GameManager.instance.ChangeEmerald(emeraldGet);

                //전장을 끄고 맵을 갱신하고 포탈을 보여준다.
                GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(false);
                UIManager.instance.TurnDeckOnOff(false);
                FloorManager.instance.ChangeCurRoomToIdle();
                UIManager.instance.RevealMapArea(FloorManager.instance.playerX, FloorManager.instance.playerY);
                UIManager.instance.TurnMapOnOff(true);
                FloorManager.instance.TurnPortalsOnOff(true);
            }
            else
            {
                phase++;
                StartPhase();
            }
        }
    }

    //현재 RP량을 바꾼다.
    public void ChangeCurrentRP(float _rp)
    {
        int consumeRP;
        rp = _rp;
        UIManager.instance.battleRPText.text = ((int)rp).ToString();
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)    //RP값 변경 후 생성 가능한 링만 버튼 활성화(=버튼 가림막 비활성화)
        {
            if (int.TryParse(UIManager.instance.battleDeckRingRPText[i].text, out consumeRP))
            {
                if (consumeRP <= rp) UIManager.instance.battleRPNotEnough[i].SetActive(false);
                else UIManager.instance.battleRPNotEnough[i].SetActive(true);
            }
            else
            {
                switch (UIManager.instance.battleDeckRingRPText[i].text)
                {
                    case "10.00":
                    case "20/20":
                        UIManager.instance.battleRPNotEnough[i].SetActive(false);
                        break;
                    default:
                        UIManager.instance.battleRPNotEnough[i].SetActive(true);
                        break;
                }
            }
        }
        if (rp < 10) UIManager.instance.battleRPNotEnough[DeckManager.instance.maxDeckLength].SetActive(true);
        else UIManager.instance.battleRPNotEnough[DeckManager.instance.maxDeckLength].SetActive(false);
    }
}
