using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public bool debugFlag;  //디버그용
    public int debugVariable;

    public List<Monster> monsters;  //현재 전투중인 몬스터들
    public List<DropRP> dropRPs;    //링의 정수들

    //전투 별 변수
    public bool isBattlePlaying;    //전투 진행 중인지 여부
    public float rp;                //현재 보유 RP
    public List<int> ringDowngrade; //이번 전투에서 다운그레이드 된 링들
    float rpGenerateTime;           //이전 링의 정수 생산 후 시간
    float rpNextGenerateTime;       //다음 링의 정수 생산 시간
    byte pathAlpha;                 //몬스터 경로 불투명도
    public bool isBossKilled;       //보스 몬스터 처치 여부

    //웨이브 별 변수
    public int wave;                //현재 웨이브
    private int numGenMonster;      //생성할 몬스터 수
    private int newMonsterID;       //새로 생성할 몬스터의 아이디

    //기타
    public AudioSource audioSource;

    void Awake()
    {
        instance = this;
        monsters = new List<Monster>();
        dropRPs = new List<DropRP>();
        ringDowngrade = new List<int>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isBattlePlaying)    //전투 중인 경우
        {
            if (pathAlpha == 255)   //몬스터 경로가 완전히 켜져있는 경우
            {
                //전투 종료 여부 확인
                CheckWaveOver();
            }
            else                    //몬스터 경로를 서서히 켠다.
            {
                pathAlpha += 3;
                GameManager.instance.monsterPathImages[FloorManager.instance.curRoom.pathID].color = new Color32(255, 255, 255, pathAlpha);
                if (pathAlpha == 255) StartWave();  //다 켰으면 첫번째 웨이브를 시작한다.
            }

            if (Time.timeScale != 0)    //일시정지 중이 아니라면
            {
                rpGenerateTime += Time.deltaTime;
                if (rpGenerateTime > rpNextGenerateTime)    //일정 시간마다 링의 정수를 생성한다.
                {
                    rpGenerateTime = 0.0f;

                    DropRP dropRP = GameManager.instance.GetDropRPFromPool();
                    dropRP.InitializeDropRP();
                    dropRPs.Add(dropRP);
                    dropRP.gameObject.SetActive(true);

                    //정수의 요람 유물 보유 여부에 따라 다음 링의 정수 생산까지 걸리는 시간을 결정한다.
                    if (GameManager.instance.baseRelics[18].have)   
                    {
                        if (GameManager.instance.baseRelics[18].isPure) rpNextGenerateTime = Random.Range(4.5f, 6.0f);
                        else rpNextGenerateTime = Random.Range(6.5f, 8.0f);
                    }
                    else rpNextGenerateTime = Random.Range(5.5f, 7.0f);
                }
            }
        }
    }

    //전투를 시작한다.
    public void StartBattle()
    {
        Time.timeScale = 1.0f;

        //전투 별 변수 초기화
        ringDowngrade.Clear();
        isBossKilled = false;
        pathAlpha = 0;
        rpGenerateTime = 0.0f;
        rpNextGenerateTime = 5.0f;
        rp = 50 + GameManager.instance.spiritEnhanceLevel[3] * 2;
        if (GameManager.instance.baseRelics[19].have)
        {
            if (GameManager.instance.baseRelics[19].isPure) rp *= 1.1f;
            else rp *= 0.9f;
        }

        //전장을 키고 UI를 셋팅한다.
        GameManager.instance.monsterPathImages[FloorManager.instance.curRoom.pathID].color = new Color(255, 255, 255, 0);
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(true);
        UIManager.instance.TurnMapOnOff(false);
        UIManager.instance.OpenBattleDeckPanel();
        ChangePlayerRP(0);

        //덱 매니저의 전투 관련 변수들을 초기화한다.
        DeckManager.instance.PrepareBattle();

        //웨이브 1을 시작한다.
        wave = 1;
        isBattlePlaying = true;
    }

    //웨이브를 시작한다. 관련 변수를 초기화하고 몬스터 생성 코루틴을 시작한다.
    void StartWave()
    {
        //생성할 몬스터 수를 결정한다.
        if (FloorManager.instance.isNotTutorial)
        {
            if (wave == 2) numGenMonster = 20;
            else numGenMonster = 15;
            if (GameManager.instance.baseRelics[5].have)
            {
                if (GameManager.instance.baseRelics[5].isPure) numGenMonster = (int)(numGenMonster * 0.9f);
                else numGenMonster = (int)(numGenMonster * 1.1f);
            }
        }
        else numGenMonster = 5;
        //첫번째 몬스터부터 생성 ID를 0으로 생성 시작한다.
        newMonsterID = 0;
        audioSource.Play();
        StartCoroutine(GenerateMonster());
    }

    //몬스터를 생성한다.
    IEnumerator GenerateMonster()
    {
        while (newMonsterID < numGenMonster && isBattlePlaying) //전투중이고 아직 목표 몬스터 수만큼 생성하지 못했다면
        {
            //몬스터 능력치 배율을 조정한다.
            float scale = 0.5f * (FloorManager.instance.floor.floorNum + 1) * (1.0f + 0.5f * (wave - 1));
            if (!FloorManager.instance.isNotTutorial) scale = 0.5f;

            //몬스터 타입을 결정한다.
            int monsterType;
            if (wave == 3 && newMonsterID == 0)   //웨이브 3의 첫 몬스터는 반드시 엘리트/보스
            {
                if (FloorManager.instance.curRoom.type == 1) monsterType = Random.Range(15, 22);
                else
                {
                    scale = 1.0f;
                    monsterType = FloorManager.instance.floor.floorNum + 21;
                    if (!FloorManager.instance.isNotTutorial)
                    {
                        scale = 0.1f;
                        monsterType = 22;
                    }
                }
            }
            else monsterType = Random.Range(0, 15);    //그외에는 일반 몬스터

            //생성한다.
            Monster monster = GameManager.instance.GetMonsterFromPool(monsterType);
            monster.gameObject.transform.position = new Vector2(100, 100);  //초기에는 멀리 떨어뜨려놓아야 path의 중간에서 글리치 하지 않음.
            monster.InitializeMonster(newMonsterID++, FloorManager.instance.curRoom.pathID, scale);
            monster.gameObject.SetActive(true);
            monsters.Add(monster);

            //1초 후 다시 생성한다.
            yield return new WaitForSeconds(1.0f);
        }
    }

    //웨이브가 종료되었는지 확인한다.
    void CheckWaveOver()
    {
        if (monsters.Count == 0 && newMonsterID == numGenMonster)   //남이있는 몬스터가 없고 생성할 몬스터는 다 생성했으면
        {
            if (wave == 3)     //마지막 웨이브였다면 보상을 주고 전투를 종료한다.
            {
                isBattlePlaying = false;

                //HP가 남아있는 채로 끝났으면 다시 게임 속도를 원래대로 돌린다.
                if (GameManager.instance.playerCurHP > 0) Time.timeScale = 1;

                //현재 있는 방을 다시 로드한다.
                SceneChanger.instance.ChangeScene(ReloadBattleRoom, FloorManager.instance.playerX, FloorManager.instance.playerY, 3); 
            }
            else //다음 웨이브를 바로 시작한다.
            {
                wave++;
                StartWave();
            }
        }
    }

    //현재 있는 전투방을 다시 로드한다.
    void ReloadBattleRoom(int x, int y)
    {
        //게임오버면 로드를 취소한다.
        if (GameManager.instance.playerCurHP == 0) return;

        //다운그레이드 된 링들을 50% 확률로 복구
        for (int i = 0; i < ringDowngrade.Count; i++)
            GameManager.instance.baseRings[ringDowngrade[i]].Upgrade(0.5f);
        ringDowngrade.Clear();

        //보상 아이템을 드랍한다. 아이템이 한 개라도 드랍되었다면 방 타입을 바꿔서 맵 형태를 변경할 수 있도록 한다. 단, 보스방이면 변경하지 않는다.
        if (RewardItemDrop() && FloorManager.instance.curRoom.type != 9) FloorManager.instance.curRoom.type = 6;

        //전투를 완전히 종료한다.
        StopBattleSystem();

        //UI를 셋팅한다.
        UIManager.instance.battleArrangeFail.SetActive(false);
        UIManager.instance.ClosePanel(0);
        UIManager.instance.RevealMapAndMoveMarker(FloorManager.instance.playerX, FloorManager.instance.playerY);
        UIManager.instance.TurnMapOnOff(true);

        //현재 방을 다시 로드한다(포탈을 킨다).
        FloorManager.instance.MoveToRoom(x, y);
    }

    //전투 보상으로 아이템을 드롭한다. 뭐라도 드롭하면 true, 아무것도 드롭하지 않으면 false를 반환한다.
    bool RewardItemDrop()
    {
        bool isItemDrop = false;
        if (FloorManager.instance.curRoom.type != 9)    //일반 전투방인 경우
        {
            if (FloorManager.instance.isNotTutorial)
            {
                //재화와 링 중 어느 것을 획득할지 결정한다. 이 확률은 유물 보유 여부에 의해 조정한다.
                float goldProb = 0.9f;
                if (GameManager.instance.baseRelics[10].have)
                {
                    if (GameManager.instance.baseRelics[10].isPure) goldProb = 0.8f;
                    else goldProb = 2.0f;
                }
                goldProb -= GameManager.instance.spiritEnhanceLevel[7] * 0.02f;
                if (Random.Range(0.0f, 1.0f) < goldProb)    //재화 획득인 경우
                {
                    //골드 드랍(0~2개)
                    int goldGet = Random.Range(0, 3);
                    if (goldGet != 0) isItemDrop = true;
                    for (int i = 0; i < goldGet; i++)
                    {
                        Item item = GameManager.instance.GetItemFromPool();
                        item.InitializeItem(4, FloorManager.instance.itemPos[i], 0, 0);
                        FloorManager.instance.curRoom.AddItem(item);
                    }

                    //다이아몬드 드랍(0~2개)
                    float diamondProb = -1.0f;
                    int diamondGet = 0;
                    for (int i = 0; i < DeckManager.instance.rings.Count; i++)
                        if (DeckManager.instance.rings[i].baseRing.id == 17)
                        {
                            diamondProb = DeckManager.instance.rings[i].curATK * 0.01f + DeckManager.instance.rings[i].curEFF;
                            break;
                        }
                    if (Random.Range(0.0f, 1.0f) <= diamondProb) diamondGet++;
                    if (GameManager.instance.baseRelics[15].have && GameManager.instance.baseRelics[15].isPure && Random.Range(0.0f, 1.0f) <= 0.33f) diamondGet++;
                    if (diamondGet != 0) isItemDrop = true;
                    for (int i = 0; i < diamondGet; i++)
                    {
                        Item item = GameManager.instance.GetItemFromPool();
                        item.InitializeItem(5, FloorManager.instance.itemPos[i + 3], 0, 0);
                        FloorManager.instance.curRoom.AddItem(item);
                    }
                }
                else    //링 획득인 경우
                {
                    int itemID;
                    Item item = GameManager.instance.GetItemFromPool();
                    do itemID = Random.Range(0, GameManager.instance.baseRings.Count);
                    while (DeckManager.instance.deck.Contains(itemID));
                    item.InitializeItem(1000 + itemID, Vector3.forward, 0, 0);
                    FloorManager.instance.curRoom.AddItem(item);
                    isItemDrop = true;
                }
            }
            else
            {
                if (FloorManager.instance.playerX == 5)
                {
                    //골드 드랍(0~2개)
                    isItemDrop = true;
                    for (int i = 0; i < 2; i++)
                    {
                        Item item = GameManager.instance.GetItemFromPool();
                        item.InitializeItem(4, FloorManager.instance.itemPos[i], 0, 0);
                        FloorManager.instance.curRoom.AddItem(item);
                    }
                }
                else
                {
                    isItemDrop = true;
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(1003, Vector3.forward, 0, 0);
                    FloorManager.instance.curRoom.AddItem(item);
                }
            }
        }
        else  //보스방인 경우
        {
            if (FloorManager.instance.isNotTutorial)
            {
                if (isBossKilled && (!GameManager.instance.baseRelics[15].have || GameManager.instance.baseRelics[15].isPure))    //보스를 처치했고 유물에 의한 유물 드랍 불가 상태가 아니면
                {
                    int itemID;
                    Item item = GameManager.instance.GetItemFromPool();
                    do itemID = Random.Range(0, GameManager.instance.baseRelics.Count);
                    while (GameManager.instance.relics.Contains(itemID));
                    item.InitializeItem(2000 + itemID, FloorManager.instance.itemPos[3], 0, 0);
                    FloorManager.instance.curRoom.AddItem(item);

                    item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(5, FloorManager.instance.itemPos[2], 0, 0);
                    FloorManager.instance.curRoom.AddItem(item);

                    int diamondGet = 4 + Random.Range(0, 3);
                    for (int i = 4; i < diamondGet; i++)
                    {
                        item = GameManager.instance.GetItemFromPool();
                        item.InitializeItem(5, FloorManager.instance.itemPos[i], 0, 0);
                        FloorManager.instance.curRoom.AddItem(item);
                    }
                    isItemDrop = true;
                }
            }
            else
            {
                Item item = GameManager.instance.GetItemFromPool();
                item.InitializeItem(2000, FloorManager.instance.itemPos[3], 0, 0);
                FloorManager.instance.curRoom.AddItem(item);
                isItemDrop = true;

                item = GameManager.instance.GetItemFromPool();
                item.InitializeItem(5, FloorManager.instance.itemPos[2], 0, 0);
                FloorManager.instance.curRoom.AddItem(item);
            }
        }
        return isItemDrop;
    }

    //전투를 종료한다. 지금까지 생성된 링의 정수, 링, 몬스터 등을 모두 오브젝트 풀에 되돌리고, 몬스터 경로를 끈다.
    public void StopBattleSystem()
    {
        isBattlePlaying = false;

        StopAllCoroutines();

        //링의 정수 정리
        for (int i = dropRPs.Count - 1; i >= 0; i--)
            GameManager.instance.ReturnDropRPToPool(dropRPs[i]);
        dropRPs.Clear();

        //링 정리
        for (int i = 0; i < DeckManager.instance.rings.Count; i++)
            GameManager.instance.ReturnRingToPool(DeckManager.instance.rings[i]);
        DeckManager.instance.rings.Clear();

        //링 생성/제거 중이었다면 이것도 정리
        if (DeckManager.instance.isEditRing)
        {
            if (DeckManager.instance.genRing != null) GameManager.instance.ReturnRingToPool(DeckManager.instance.genRing);
            else DeckManager.instance.ringRemover.transform.position = new Vector3(100, 100, 0);
        }

        //몬스터 정리
        for (int i = monsters.Count - 1; i >= 0; i--)
            monsters[i].RemoveFromBattle(false);
        monsters.Clear();

        //몬스터 경로 끄기
        GameManager.instance.monsterPaths[FloorManager.instance.curRoom.pathID].gameObject.SetActive(false);
    }

    //현재 RP량을 바꾼다. 변경에 성공하면 true, 실패하면 false를 반환한다.
    public bool ChangePlayerRP(float _rp)
    {
        if (rp + _rp < 0) return false; //바꾼 결과가 음수가 되면 안된다.
        
        //값을 변경하고 UI를 갱신한다.
        rp += _rp;
        UIManager.instance.battleHaveRPText.text = ((int)rp).ToString();

        //RP값 변경 후, 링 생성 덱에서 생성 가능한 링만 버튼 가림막을 비활성화한다.
        int consumeRP;
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)
        {
            if (int.TryParse(UIManager.instance.battleDeckRingRPText[i].text, out consumeRP))   //단순 RP 소모 방식으로 생성하는 경우
            {
                if (consumeRP <= rp) UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(false); //충분한 RP가 있는 경우만 비활성화
                else UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(true);
            }
            else   //최대 생성한도에 도달했거나 특정 조건에 의해 생성하는 경우
            {
                switch (UIManager.instance.battleDeckRingRPText[i].text)
                {
                    case "10.00":
                    case "10/10":
                        UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(false);
                        break;
                    default:
                        UIManager.instance.battleDeckRPNotEnoughCover[i].SetActive(true);
                        break;
                }
            }
        }

        //RP값 변경 후, 충분한 RP가 있는 경우만 링 제거 버튼 가림막을 비활성화한다.
        UIManager.instance.battleDeckRPNotEnoughCover[DeckManager.instance.maxDeckLength].SetActive(rp < 10);

        return true;
    }
}
