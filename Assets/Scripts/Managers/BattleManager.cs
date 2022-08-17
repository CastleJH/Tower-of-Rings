using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public bool debugFlag;  //디버그용
    public int debugVariable;

    public List<Monster> monsters; //현재 스테이지의 몬스터들
    public List<DropRP> dropRPs;

    //전투 별 변수
    public bool isBattlePlaying;  //게임 진행 중인지 여부
    public float rp;        //현재 보유 RP
    public List<int> ringDowngrade;
    float rpGenerateTime;
    float rpNextGenerateTime;
    byte pathAlpha;

    //웨이브 별 변수
    public int wave;   //현재 웨이브
    private int numGenMonster;  //생성할 몬스터 수
    private int newMonsterID;   //새로 생성할 몬스터 아이디

    void Awake()
    {
        instance = this;
        monsters = new List<Monster>();
        dropRPs = new List<DropRP>();
        ringDowngrade = new List<int>();
    }

    void Update()
    {
        if (isBattlePlaying)    //전투 중인 경우
        {
            //전투 종료 여부 확인
            CheckBattleOver();
            
            if (pathAlpha < 255)
            {
                pathAlpha += 3;
                GameManager.instance.monsterPathImages[FloorManager.instance.curRoom.pathID].color = new Color32(255, 255, 255, pathAlpha);
                if (pathAlpha == 255) StartWave();
            }

            if (Time.timeScale != 0)
            {
                //rpGenerateTime += Time.unscaledDeltaTime;
                rpGenerateTime += Time.deltaTime;
                if (rpGenerateTime > rpNextGenerateTime)
                {
                    rpGenerateTime = 0.0f;
                    rpNextGenerateTime = Random.Range(10.0f, 14.0f);
                    DropRP dropRP = GameManager.instance.GetDropRPFromPool();
                    dropRP.InitializeDropRP();
                    dropRPs.Add(dropRP);
                    dropRP.gameObject.SetActive(true);
                }
            }
        }
    }

    //전투를 시작한다.
    public void StartBattle()
    {
        //전투 별 변수 초기화
        ringDowngrade.Clear();
        rpGenerateTime = 0.0f;
        rpNextGenerateTime = Random.Range(10.0f, 14.0f);
        pathAlpha = 0;

        //전장을 킨다.
        UIManager.instance.TurnMapOnOff(false);
        GameManager.instance.monsterPathImages[FloorManager.instance.curRoom.pathID].color = new Color(255, 255, 255, 0);
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(true);
        UIManager.instance.OpenBattleDeckPanel();

        //초기 RP를 정하고 UI를 업데이트한다.
        rp = 50;
        ChangePlayerRP(0);

        //덱 매니저의 전투 관련 변수들을 초기화한다.
        DeckManager.instance.PrepareBattle();

        wave = 1;
        newMonsterID = 0;
        if (GameManager.instance.baseRelics[5].have)
        {
            if (GameManager.instance.baseRelics[5].isPure) numGenMonster = 27;
            else numGenMonster = 33;
        }
        else numGenMonster = 30;
        isBattlePlaying = true;
    }

    //웨이브를 시작한다. 관련 변수를 초기화하고 몬스터 생성 코루틴을 시작한다.
    void StartWave()
    {
        if (GameManager.instance.baseRelics[5].have)
        {
            if (GameManager.instance.baseRelics[5].isPure)
            {
                if (wave == 2) numGenMonster = 40;
                else numGenMonster = 27;
            }
            else
            {
                if (wave == 2) numGenMonster = 50;
                else numGenMonster = 33;
            }
        }
        else
        {
            if (wave == 2) numGenMonster = 45;
            else numGenMonster = 30;
        }
        newMonsterID = 0;
        StartCoroutine(GenerateMonster());
    }

    //몬스터를 생성한다.
    IEnumerator GenerateMonster()
    {
        Debug.Log("Start Coroutine");
        while (newMonsterID < numGenMonster)
        {
            Debug.Log("gen!");
            //몬스터 능력치 배율을 조정한다.
            float scale;
            scale = 0.5f * (FloorManager.instance.floor.floorNum + 1) + 0.05f * (wave - 1);

            //몬스터 생성
            int monsterType;
            if (wave == 3 && newMonsterID == 0)   //웨이브 3의 첫 몬스터는 반드시 엘리트/보스
            {
                if (FloorManager.instance.curRoom.type == 1) monsterType = Random.Range(15, 22);
                else monsterType = FloorManager.instance.floor.floorNum + 21;
            }
            else monsterType = Random.Range(0, 15);    //그외에는 일반 몬스터

            Monster monster = GameManager.instance.GetMonsterFromPool(monsterType);
            monster.gameObject.transform.position = new Vector2(100, 100);  //초기에는 멀리 떨어뜨려놓아야 path의 중간에서 글리치 하지 않음.
            monster.InitializeMonster(newMonsterID, FloorManager.instance.curRoom.pathID, scale);
            monster.gameObject.SetActive(true);
            monsters.Add(monster);
            newMonsterID++;
            yield return new WaitForSeconds(45.0f / numGenMonster); //45초 동안 몬스터들을 등장시켜야 한다. 몬스터 수에 비례하여 생성 주기를 결정한다.
        }
    }

    //배틀이 종료되었는지 확인한다.
    void CheckBattleOver()
    {
        if (monsters.Count == 0 && newMonsterID == numGenMonster)
        {
            if (wave == 3)     //마지막 웨이브였다면 보상을 주고 전투를 종료한다.
            {
                if (GameManager.instance.playerCurHP > 0) Time.timeScale = 1;

                //배틀을 종료한다.
                isBattlePlaying = false;

                //몬스터 생성 종료
                StopAllCoroutines();

                SceneChanger.instance.ChangeScene(ReloadBattleRoom, FloorManager.instance.playerX, FloorManager.instance.playerY);
            }
            else
            {
                wave++;
                StartWave();
            }
        }
    }

    void ReloadBattleRoom(int x, int y)
    {
        //링의 정수 정리
        for (int i = dropRPs.Count - 1; i >= 0; i--)
            GameManager.instance.ReturnDropRPToPool(dropRPs[i]);
        dropRPs.Clear();

        float greedyATK = -1.0f;
        float greedyEFF = 0.0f;

        //덱 정리
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
        {
            if (DeckManager.instance.rings[i].baseRing.id == 17)
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

        //다운그레이드 된 링들을 50% 확률로 복구
        for (int i = 0; i < ringDowngrade.Count; i++)
            GameManager.instance.baseRings[ringDowngrade[i]].Upgrade(0.5f);

        //전투 종료 후 아이템을 드랍한다.
        bool isItemDrop = false;

        //골드 드랍
        int goldGet = Random.Range(0, 3);
        if (goldGet != 0) isItemDrop = true;
        for (int i = 0; i < goldGet; i++)
        {
            Item item = GameManager.instance.GetItemFromPool();
            item.InitializeItem(4, FloorManager.instance.itemPos[i], 0, 0);
            FloorManager.instance.curRoom.AddItem(item);
        }

        //다이아몬드 드랍
        int diamondGet = 0;
        if (greedyATK != -1.0f)   //탐욕링이 존재했다면 보상을 늘림
            if (Random.Range(0.0f, 1.0f) <= greedyATK * 0.01f + greedyEFF) diamondGet = Random.Range(1, 3);
        if (diamondGet != 0) isItemDrop = true;
        for (int i = 0; i < diamondGet; i++)
        {
            Item item = GameManager.instance.GetItemFromPool();
            item.InitializeItem(5, FloorManager.instance.itemPos[i + 2], 0, 0);
            FloorManager.instance.curRoom.AddItem(item);
        }

        //아이템이 한번이라도 드랍되었다면 방 타입을 바꾼다.
        if (isItemDrop) FloorManager.instance.curRoom.type = 6;

        //전장을 끄고 맵을 갱신하고 포탈을 보여준다.
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(false);
        UIManager.instance.ClosePanel(0);
        UIManager.instance.RevealMapAndMove(FloorManager.instance.playerX, FloorManager.instance.playerY);
        UIManager.instance.TurnMapOnOff(true);

        FloorManager.instance.MoveToRoom(x, y);
    }

    //현재 RP량을 바꾼다.
    public bool ChangePlayerRP(float _rp)
    {
        if (rp + _rp < 0) return false;
        rp += _rp;
        UIManager.instance.battleHaveRPText.text = ((int)rp).ToString();
        int consumeRP;
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)    //RP값 변경 후 생성 가능한 링만 버튼 활성화(=버튼 가림막 비활성화)
        {
            if (int.TryParse(UIManager.instance.battleDeckRingRPText[i].text, out consumeRP))
            {
                if (consumeRP <= rp) UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(false);
                else UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(true);
            }
            else
            {
                switch (UIManager.instance.battleDeckRingRPText[i].text)
                {
                    case "20.00":
                    case "20/20":
                        UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(false);
                        break;
                    default:
                        UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(true);
                        break;
                }
            }
        }
        if (rp < 10) UIManager.instance.battleDeckRPNotEnoughCover[DeckManager.instance.maxDeckLength].SetActive(true);
        else UIManager.instance.battleDeckRPNotEnoughCover[DeckManager.instance.maxDeckLength].SetActive(false);

        return true;
    }
}
