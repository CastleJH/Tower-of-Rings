using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public ParticleSystem touchParticle;

    public GameObject gameStartPanel;
    public GameObject gameStartText;

    public GameObject lobbyPanel;
    public Toggle lobbyHardModeToggleButton;
    public GameObject[] lobbyCollectionDiamonds;

    public GameObject lobbyRingCollectionPanel;
    public GameObject[] lobbyRingCollectionSelectCircle;
    public GameObject[] lobbyRingCollectionRingDiamonds;
    public TextMeshProUGUI lobbyRingCollectionRingNameText;
    public GameObject[] lobbyRingCollectionQuestDiamonds;
    public TextMeshProUGUI[] lobbyRingCollectionQuestDiamondsAmountText;
    public TextMeshProUGUI[] lobbyRingCollectionProgressText;
    public RectTransform[] lobbyRingCollectionProgressBar;

    public GameObject lobbyRelicCollectionPanel;
    public GameObject[] lobbyRelicCollectionSelectSquare;
    public GameObject[] lobbyRelicCollectionRelicDiamonds;
    public TextMeshProUGUI lobbyRelicCollectionRelicNameText;
    public GameObject[] lobbyRelicCollectionQuestDiamonds;
    public TextMeshProUGUI[] lobbyRelicCollectionQuestDiamondsAmountText;
    public TextMeshProUGUI[] lobbyRelicCollectionProgressText;
    public RectTransform[] lobbyRelicCollectionProgressBar;

    public GameObject spiritEnhancePanel;
    public TextMeshProUGUI[] spiritEnhanceLevelText;
    public TextMeshProUGUI[] spiritEnhanceCostText;

    public Image gameEndPanel;
    public Sprite[] gameEndSprites;
    
    public GameObject mapPanel;
    public Image[] mapRow1, mapRow2, mapRow3, mapRow4, mapRow5, mapRow6, mapRow7, mapRow8, mapRow9;
    public Image[][] maps;
    public RectTransform playerMarker;

    public TextMeshProUGUI playerGoldText;
    public TextMeshProUGUI playerDiamondText;
    public TextMeshProUGUI playerHPText;

    public GameObject battleArrangeFail;
    public TextMeshProUGUI battleArrangeFailText;

    public GameObject battleDeckPanel;
    public TextMeshProUGUI battleHaveRPText;
    public Image[] battleDeckRingImage;
    public Image[] battleDeckRingUpgradeImage;
    public GameObject[] battleDeckRingRP;
    public TextMeshProUGUI[] battleDeckRingRPText;
    public GameObject[] battleDeckRPNotEnoughCover;
    public Image battleDeckSpeedButtonImage;

    public GameObject ringSelectionPanel;
    public TextMeshProUGUI ringSelectionTypeText;
    public Image[] ringSelectionRingImage;
    public Image[] ringSelectionRingUpgradeImage;
    public GameObject[] ringSelectionRP;
    public TextMeshProUGUI[] ringSelectionRPText;
    public Image[] ringSelectionButtonImage;
    public TextMeshProUGUI[] ringSelectionButtonText;
    public Image ringSelectionEffectImage;
    public Animation ringSelectionEffectAnimation;

    public GameObject playerStatusPanel;
    public Image[] playerStatusRingImage;
    public Image[] playerStatusRingUpgradeImage;
    public GameObject[] playerStatusRP;
    public TextMeshProUGUI[] playerStatusRPText;
    public Image[] playerStatusRelicImage;
    public GameObject[] playerStatusRelicCursedImage;

    public GameObject ringInfoPanel;
    public Image ringInfoRingImage;
    public Image ringInfoRingUpgradeImage;
    public TextMeshProUGUI ringInfoRPText;
    public TextMeshProUGUI ringInfoNameText;
    public TextMeshProUGUI ringInfoATKText;
    public TextMeshProUGUI ringInfoSPDText;
    public TextMeshProUGUI ringInfoRNGText;
    public TextMeshProUGUI ringInfoTARText;
    public TextMeshProUGUI ringInfoBaseText;
    public TextMeshProUGUI ringInfoSameSynergyText;
    public TextMeshProUGUI ringInfoAllSynergyText;
    public GameObject ringInfoTakeButton;
    public TextMeshProUGUI ringInfoTakeText;
    public GameObject ringInfoCannotTake;
    public TextMeshProUGUI ringInfoCannotTakeText;

    public GameObject relicInfoPanel;
    public Image relicInfoRelicImage;
    public TextMeshProUGUI relicInfoNameText;
    public TextMeshProUGUI relicInfoBaseEffectText;
    public TextMeshProUGUI relicInfoCursedEffectText;
    public GameObject relicInfoCursedImage;
    public GameObject relicInfoCursedNotify;
    public GameObject relicInfoTakeButton;
    public TextMeshProUGUI relicInfoTakeText;
    public GameObject relicInfoCannotTake;

    float titleTextBlinkTime;

    void Awake()
    {
        instance = this;
        maps = new Image[10][];
        maps[1] = mapRow1;
        maps[2] = mapRow2;
        maps[3] = mapRow3;
        maps[4] = mapRow4;
        maps[5] = mapRow5;
        maps[6] = mapRow6;
        maps[7] = mapRow7;
        maps[8] = mapRow8;
        maps[9] = mapRow9;

        titleTextBlinkTime = 0.0f;
        gameStartPanel.SetActive(true);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))    //항상 터치 지점에 작은 파티클을 띄운다.
        {
            Vector2 touchPos;

            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;
            touchParticle.gameObject.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
            touchParticle.transform.Translate(Vector3.forward);
            touchParticle.Play();
        }
    
        if (gameStartPanel.activeSelf)  //타이틀 화면에서 터치하라는 텍스트를 반복 점멸한다.
        {
            titleTextBlinkTime += Time.deltaTime;
            if (titleTextBlinkTime > 0.5f)
            {
                titleTextBlinkTime = 0;
                gameStartText.SetActive(!gameStartText.activeSelf);
            }
        }
    }

    //전투에서 링 생성 버튼이 눌린 경우에 불린다.
    public void ButtonGenerateRing(int index)
    {
        if (Input.touchCount > 1) return;
        if (BattleManager.instance.isBattlePlaying && Time.timeScale != 0)
        {
            if (battleDeckRPNotEnoughCover[index].activeSelf) return;
            DeckManager.instance.isEditRing = true;
            if (index == DeckManager.instance.maxDeckLength) //제거 버튼이라면
            {
                return;
            }
            else if (index < DeckManager.instance.deck.Count) //빈 링이 아니라면
            {
                Ring tmpRing = GameManager.instance.GetRingFromPool();
                tmpRing.InitializeRing(DeckManager.instance.deck[index]);
                DeckManager.instance.genRing = tmpRing;
                DeckManager.instance.genRing.gameObject.SetActive(true);
                //battleRingDetailLongClickTime = 0.0f;
                //checkBattleRingDetailOn = true;
            }
            else DeckManager.instance.isEditRing = false;
        }
    }

    //전투 시 링 배치 실패 이유에 알맞은 UI를 보여준다.
    public void SetBattleArrangeFail(string str)
    {
        if (str == null) battleArrangeFail.SetActive(false);
        else
        {
            battleArrangeFailText.text = str;
            battleArrangeFail.SetActive(true);
        }
    }

    //맵을 초기화한다.
    public void InitializeMap()
    {
        for (int i = 1; i <= 9; i++)
            for (int j = 1; j <= 9; j++)
                maps[i][j].color = new Color32(0, 0, 0, 0);
    }

    //맵의 일부분을 밝히고, 마커를 해당 위치로 이동한다.
    public void RevealMapAndMoveMarker(int x, int y)
    {
        int[] dx = { 0, 0, 0, -1, 1 };
        int[] dy = { 0, -1, 1, 0, 0 };
        int nx, ny;
        Room room;

        playerMarker.anchoredPosition = maps[x][y].rectTransform.anchoredPosition;

        for (int i = 0; i < 5; i++)
        {
            nx = x + dx[i];
            ny = y + dy[i];
            room = FloorManager.instance.floor.rooms[nx, ny];
            if (room.type != -1 && room.type != 10)
            {
                maps[nx][ny].sprite = GameManager.instance.mapRoomSprites[room.type];
                if (room.visited && room.type == 1) maps[nx][ny].sprite = GameManager.instance.mapRoomSprites[0];
                else if (room.type != 9 && room.type != 8 && room.visited && room.items.Count == 0) maps[nx][ny].sprite = GameManager.instance.mapRoomSprites[0];
                maps[nx][ny].color = Color.white;
            }
        }
    }

    //맵을 끈다.
    public void TurnMapOnOff(bool isOn)
    {
        mapPanel.SetActive(isOn);
    }

    //하단의 전투 덱을 연다.
    public void OpenBattleDeckPanel()
    {
        int i;
        int type;
        battleDeckSpeedButtonImage.sprite = GameManager.instance.speedSprites[0];
        for (i = 0; i < DeckManager.instance.deck.Count; i++)
        {
            type = DeckManager.instance.deck[i];
            battleDeckRingImage[i].sprite = GameManager.instance.ringSprites[type];
            if (GameManager.instance.baseRings[type].level == GameManager.instance.baseRings[type].maxlvl) battleDeckRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[0];
            else battleDeckRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[GameManager.instance.baseRings[type].level];
            battleDeckRingRPText[i].text = GameManager.instance.baseRings[type].baseRP.ToString();
            battleDeckRingUpgradeImage[i].gameObject.SetActive(true);
            battleDeckRingRP[i].SetActive(true);
        }
        for (; i < battleDeckRingImage.Length; i++)
        {
            battleDeckRingImage[i].sprite = GameManager.instance.emptyRingSprite;
            battleDeckRingUpgradeImage[i].gameObject.SetActive(false);
            battleDeckRingRP[i].SetActive(false);
        }
        battleDeckPanel.SetActive(true);
    }

    //전투 UI에서 덱에 있는 링의 RP 비용 텍스트를 갱신한다.
    public void SetBattleDeckRingRPText(int index, int rp)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingRPText[index].text = " ";
        else battleDeckRingRPText[index].text = rp.ToString();
    }

    //전투 UI에서 덱에 있는 링의 RP 비용 텍스트를 문자열로 갱신한다.
    public void SetBattleDeckRingRPText(int index, string str)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingRPText[index].text = " ";
        battleDeckRingRPText[index].text = str;
    }

    //링 선택 패널을 연다.
    public void OpenRingSelectionPanel(int mode)
    {

        ringSelectionEffectAnimation.gameObject.SetActive(false);

        int i;
        int type;
        if (mode == 0) ringSelectionTypeText.text = "파괴할 링을 선택하세요.";
        else if (mode == 1) ringSelectionTypeText.text = "제련할 링을 선택하세요.";
        for (i = 0; i < DeckManager.instance.deck.Count; i++)
        {
            type = DeckManager.instance.deck[i];
            ringSelectionRingImage[i].sprite = GameManager.instance.ringSprites[type];
            ringSelectionButtonImage[i].gameObject.SetActive(true);
            if (GameManager.instance.baseRings[type].level == GameManager.instance.baseRings[type].maxlvl)
            {
                ringSelectionRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[0];
                if (mode == 1) ringSelectionButtonImage[i].gameObject.SetActive(false);
            }
            else ringSelectionRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[GameManager.instance.baseRings[type].level];
            ringSelectionRPText[i].text = GameManager.instance.baseRings[type].baseRP.ToString();
            ringSelectionRingUpgradeImage[i].gameObject.SetActive(true);
            ringSelectionRP[i].SetActive(true);

            if (mode != -1) ringSelectionButtonImage[i].sprite = GameManager.instance.buttonSprites[mode];
            else ringSelectionButtonImage[i].gameObject.SetActive(false);
            if (mode == 0) ringSelectionButtonText[i].text = "파괴";
            else if (mode == 1) ringSelectionButtonText[i].text = "제련";
        }
        for (; i < ringSelectionRingImage.Length; i++)
        {
            ringSelectionRingImage[i].sprite = GameManager.instance.emptyRingSprite;
            ringSelectionRingUpgradeImage[i].gameObject.SetActive(false);
            ringSelectionRP[i].SetActive(false);

            ringSelectionButtonImage[i].gameObject.SetActive(false);
        }

        ringSelectionPanel.SetActive(true);
    }

    //링 정보 패널을 연다.
    public void OpenRingInfoPanel(int id)
    {
        BaseRing baseRing = GameManager.instance.baseRings[id];
        ringInfoRingImage.sprite = GameManager.instance.ringSprites[id];
        if (baseRing.level == baseRing.maxlvl) ringInfoRingUpgradeImage.sprite = GameManager.instance.ringInfoUpgradeSprites[0];
        else ringInfoRingUpgradeImage.sprite = GameManager.instance.ringInfoUpgradeSprites[baseRing.level];
        ringInfoRPText.text = ((int)baseRing.baseRP).ToString();
        ringInfoNameText.text = baseRing.name + " 링";
        ringInfoATKText.text = (Mathf.Round(baseRing.baseATK * 100) * 0.01f).ToString();
        ringInfoSPDText.text = (Mathf.Round(baseRing.baseSPD * 100) * 0.01f).ToString();
        ringInfoRNGText.text = (baseRing.range - 2).ToString();
        ringInfoTARText.text = baseRing.baseNumTarget.ToString();
        ringInfoBaseText.text = baseRing.description;
        ringInfoSameSynergyText.text = baseRing.toSame;
        ringInfoAllSynergyText.text = baseRing.toAll;

        ringInfoTakeButton.gameObject.SetActive(!playerStatusPanel.activeSelf && !ringSelectionPanel.activeSelf && !lobbyRingCollectionPanel.activeSelf);
        ringInfoCannotTake.SetActive(false);

        ringInfoPanel.SetActive(true);
    }

    //유물 정보 패널을 연다.
    public void OpenRelicInfoPanel(int id)
    {
        BaseRelic baseRelic = GameManager.instance.baseRelics[id];

        relicInfoRelicImage.sprite = GameManager.instance.relicSprites[id];
        relicInfoNameText.text = baseRelic.name;
        relicInfoBaseEffectText.text = baseRelic.pureDescription;
        relicInfoCursedEffectText.text = baseRelic.cursedDescription;

        relicInfoCursedNotify.gameObject.SetActive(!baseRelic.isPure);
        relicInfoCursedImage.gameObject.SetActive(!baseRelic.isPure);
        if (!lobbyRelicCollectionPanel.activeSelf)
        {
            if (baseRelic.isPure)
            {
                relicInfoBaseEffectText.color = new Color32(200, 200, 200, 255);
                relicInfoCursedEffectText.color = new Color32(70, 70, 70, 255);
            }
            else
            {
                relicInfoBaseEffectText.color = new Color32(70, 70, 70, 255);
                relicInfoCursedEffectText.color = new Color32(200, 200, 200, 255);
            }
        }
        else
        {
            relicInfoBaseEffectText.color = new Color32(200, 200, 200, 255);
            relicInfoCursedEffectText.color = new Color32(200, 200, 200, 255);
        }

        relicInfoTakeButton.gameObject.SetActive(!playerStatusPanel.activeSelf && !lobbyRelicCollectionPanel.activeSelf);
        relicInfoCannotTake.SetActive(false);

        relicInfoPanel.SetActive(true);
    }

    //엔딩 패널을 연다.
    public void OpenEndingPanel(int endingState)
    {
        Camera.main.transform.position = new Vector2(-100, -100);
        gameEndPanel.sprite = gameEndSprites[endingState];
        gameEndPanel.gameObject.SetActive(true);
    }

    //패널을 닫는다.
    public void ClosePanel(int panelNum)
    {
        switch (panelNum)
        {
            case 0:
                battleDeckPanel.SetActive(false);
                break;
            case 1:
                ringSelectionPanel.SetActive(false);
                break;
            case 2:
                if (!BattleManager.instance.isBattlePlaying) Time.timeScale = 1;
                else
                {
                    Time.timeScale = 1;
                    for (int i = GameManager.instance.speedSprites.Length - 1; i > 0; i--)
                        if (battleDeckSpeedButtonImage.sprite == GameManager.instance.speedSprites[i])
                        {
                            Time.timeScale = i * 2;
                            break;
                        }
                }
                playerStatusPanel.SetActive(false);
                break;
            case 3:
                ringInfoPanel.SetActive(false);
                break;
            case 4:
                relicInfoPanel.SetActive(false);
                break;
            case 5:
                spiritEnhancePanel.SetActive(false);
                break;
            case 6:
                //"링" 콜렉션에서 다이아몬드를 획득 가능한 경우 표시한다.
                lobbyCollectionDiamonds[0].SetActive(false);
                for (int i = 0; i < lobbyRingCollectionRingDiamonds.Length; i++)
                    for (int j = 0; j < lobbyRingCollectionQuestDiamonds.Length; j++)
                        if (GameManager.instance.ringCollectionProgress[i, j] == GameManager.instance.ringCollectionMaxProgress[i, j])
                        {
                            lobbyCollectionDiamonds[0].SetActive(true);
                            break;
                        }
                lobbyRingCollectionPanel.SetActive(false);
                break;
            case 7:
                //"유물" 콜렉션에서 다이아몬드를 획득 가능한 경우 표시한다.
                lobbyCollectionDiamonds[1].SetActive(false);
                for (int i = 0; i < lobbyRelicCollectionRelicDiamonds.Length; i++)
                    for (int j = 0; j < lobbyRelicCollectionQuestDiamonds.Length; j++)
                        if (GameManager.instance.relicCollectionProgress[i, j] == GameManager.instance.relicCollectionMaxProgress[i, j])
                        {
                            lobbyCollectionDiamonds[1].SetActive(true);
                            break;
                        }
                lobbyRelicCollectionPanel.SetActive(false);
                break;
        }
    }
    
    //플레이어 상태(인벤토리)창 버튼이 눌렸을 때 불린다.
    public void ButtonPlayerStatusOpen()
    {
        Time.timeScale = 0;
        int i;
        int type;
        for (i = 0; i < DeckManager.instance.deck.Count; i++)
        {
            type = DeckManager.instance.deck[i];
            playerStatusRingImage[i].sprite = GameManager.instance.ringSprites[type];
            if (GameManager.instance.baseRings[type].level == GameManager.instance.baseRings[type].maxlvl) playerStatusRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[0];
            else playerStatusRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[GameManager.instance.baseRings[type].level];
            playerStatusRPText[i].text = GameManager.instance.baseRings[type].baseRP.ToString();
            playerStatusRingUpgradeImage[i].gameObject.SetActive(true);
            playerStatusRP[i].SetActive(true);
        }
        for (; i < playerStatusRingImage.Length; i++)
        {
            playerStatusRingImage[i].sprite = GameManager.instance.emptyRingSprite;
            playerStatusRingUpgradeImage[i].gameObject.SetActive(false);
            playerStatusRP[i].SetActive(false);
        }

        for (i = 0; i < GameManager.instance.relics.Count; i++)
        {
            playerStatusRelicImage[i].sprite = GameManager.instance.relicSprites[GameManager.instance.relics[i]];
            playerStatusRelicCursedImage[i].SetActive(!GameManager.instance.baseRelics[GameManager.instance.relics[i]].isPure);
            playerStatusRelicImage[i].gameObject.SetActive(true);
        }
        for (; i < playerStatusRelicImage.Length; i++)
            playerStatusRelicImage[i].gameObject.SetActive(false);

        playerStatusPanel.SetActive(true);
    }

    //로비의 링 콜렉션 창을 제외한 곳에서 링 정보창 버튼이 눌렸을 때 불린다.
    public void ButtonRingInfoOpen(int deckIdx)
    {
        if (deckIdx < DeckManager.instance.deck.Count)
        {
            OpenRingInfoPanel(DeckManager.instance.deck[deckIdx]);
        }
    }

    //로비의 유물 콜렉션 창을 제외한 곳에서 유물 정보창 버튼이 눌렸을 때 불린다.
    public void ButtonRelicInfoOpen(int listIdx)
    {
        if (listIdx < GameManager.instance.relics.Count)
        {
            OpenRelicInfoPanel(GameManager.instance.relics[listIdx]);
        }
    }

    //링 선택 창에서 선택 버튼이 눌렸을 때 불린다.
    public void ButtonSelectRing(int deckIdx)
    {
        if (deckIdx < DeckManager.instance.deck.Count)
        {
            int ringID = DeckManager.instance.deck[deckIdx];
            if (ringSelectionButtonText[0].text == "파괴")
            {
                GameManager.instance.RingCollectionProgressUp(ringID, 3);
                DeckManager.instance.RemoveRingFromDeck(deckIdx);
                FloorManager.instance.RemoveItem(FloorManager.instance.lastTouchItem, true);
                ringSelectionEffectImage.sprite = GameManager.instance.itemSprites[1];
            }
            else
            {
                if (GameManager.instance.baseRelics[6].have && !GameManager.instance.baseRelics[6].isPure)
                    GameManager.instance.baseRings[ringID].Upgrade(0.8f);
                else GameManager.instance.baseRings[ringID].Upgrade(2.0f);
                if (GameManager.instance.baseRings[ringID].level == GameManager.instance.baseRings[ringID].maxlvl) GameManager.instance.RingCollectionProgressUp(ringID, 2);
                FloorManager.instance.RemoveItem(FloorManager.instance.lastTouchItem, true);
                ringSelectionEffectImage.sprite = GameManager.instance.itemSprites[0];
            }
            for (int i = 0; i < ringSelectionButtonImage.Length; i++) ringSelectionButtonImage[i].gameObject.SetActive(false);
            ringSelectionEffectAnimation.gameObject.SetActive(true);
            ringSelectionEffectImage.rectTransform.anchoredPosition = new Vector2(ringSelectionRingImage[deckIdx].rectTransform.anchoredPosition.x + 50, 105);
            ringSelectionEffectAnimation.Play();
            Invoke("InvokeReloadAndCloseRingSelectionPanel", 1.0f);
        }
    }

    //링 획득 버튼이 눌렸을 때 불린다. 재화를 소모해야 하면 소모한다. 재화가 부족한 경우거나 이미 있는 링이거나 이미 덱이 다 차있다면 획득하지 않는다.
    public void ButtonTakeRing()
    {
        if (ringInfoTakeText.text[0] != '이') return;
        int type = -1;
        string targetName = ringInfoNameText.text.Substring(0, ringInfoNameText.text.Length - 2);
        for (int i = 0; i < GameManager.instance.baseRings.Count; i++)
            if (GameManager.instance.baseRings[i].name == targetName)
            {
                type = i;
                break;
            }

        if (!DeckManager.instance.AddRingToDeck(type)) return;

        for (int i = 0; i < FloorManager.instance.curRoom.items.Count; i++)
            if (FloorManager.instance.curRoom.items[i].itemType == 1000 + type)
            {
                FloorManager.instance.curRoom.items[i].Pay();
                FloorManager.instance.RemoveItem(FloorManager.instance.curRoom.items[i], false);
                break;
            }
        ClosePanel(3);
    }

    //유물 획득 버튼이 눌렸을 때 불린다. 재화를 소모해야 하면 소모한다. 이미 보유하였거나 재화가 부족한 경우라면 획득하지 않는다.
    public void ButtonTakeRelic()
    {
        if (relicInfoTakeText.text[0] != '이') return;
        int type = -1;
        for (int i = 0; i < GameManager.instance.baseRelics.Count; i++)
            if (GameManager.instance.baseRelics[i].name == relicInfoNameText.text)
            {
                type = i;
                break;
            }

        if (GameManager.instance.relics.Contains(type))
        {
            relicInfoCannotTake.SetActive(true);
            return;
        }

        //유물의 방에서 획득한 것이라면 일정 확률로 저주
        bool isRelicPure = true;
        if (FloorManager.instance.curRoom.type == 4)
        {
            float curseProb = 0.4f;
            if (GameManager.instance.baseRelics[8].have)
            {
                if (GameManager.instance.baseRelics[8].isPure) curseProb = 0.3f;
                else curseProb = 0.5f;
            }
            curseProb -= GameManager.instance.spiritEnhanceLevel[6] * 0.02f;
            if (Random.Range(0.0f, 1.0f) <= curseProb)
            {
                isRelicPure = false;
                GameManager.instance.cursedRelics.Add(type);
            }
        }

        GameManager.instance.AddRelicToPlayer(type, isRelicPure);

        for (int i = 0; i < FloorManager.instance.curRoom.items.Count; i++)
            if (FloorManager.instance.curRoom.items[i].itemType == 2000 + type)
            {
                FloorManager.instance.curRoom.items[i].Pay();
                FloorManager.instance.RemoveItem(FloorManager.instance.curRoom.items[i], false);
                break;
            }

        ClosePanel(4);
    }

    //빨리감기 버튼이 눌렸을 때 불린다.
    public void ButtonFasterSpeed()
    {
        if (BattleManager.instance.isBattlePlaying)
        {
            if (Time.timeScale < 7.5) Time.timeScale += Time.timeScale;
            else Time.timeScale = 1;
            battleDeckSpeedButtonImage.sprite = GameManager.instance.speedSprites[(int)Mathf.Round(Time.timeScale) / 2];
        }
    }

    //1배속 버튼이 눌렸을 때 불린다.
    public void ButtonNormalSpeed()
    {
        if (BattleManager.instance.isBattlePlaying)
        {
            battleDeckSpeedButtonImage.sprite = GameManager.instance.speedSprites[0];
            Time.timeScale = 1;
        }
    }

    //게임 시작(탑에 입장) 버튼이 눌렸을 때 불린다.
    public void ButtonStartGame()
    {
        GameManager.instance.GameStart();
    }

    //로비로 가는 버튼이 눌렸을 때 불린다.
    public void ButtonOpenLobby()
    {
        GameManager.instance.ChangeDiamond(0);
        SceneChanger.instance.ChangeScene(ChangeSceneToLobby, 0, 0);
    }

    //로비의 영혼 강화창 오픈 버튼이 눌렸을 때 불린다.
    public void ButtonSpiritEnhancePanelOpen()
    {
        for (int i = 0; i < GameManager.instance.spiritEnhanceLevel.Length; i++)
        {
            spiritEnhanceLevelText[i].text = "Level\n" + GameManager.instance.spiritEnhanceLevel[i].ToString() + "/" + GameManager.instance.spiritMaxLevel[i].ToString();
            if (GameManager.instance.spiritEnhanceLevel[i] == GameManager.instance.spiritMaxLevel[i]) spiritEnhanceCostText[i].text = "MAX";
            else spiritEnhanceCostText[i].text = ((int)(Mathf.Pow(1.2f, GameManager.instance.spiritEnhanceLevel[i]) * GameManager.instance.spiritBaseEnhanceCost[i])).ToString();
        }
        spiritEnhancePanel.SetActive(true);
    }

    //로비의 영혼 강화창에서 강화 버튼이 눌렸을 때 불린다.
    public void ButtonEnhanceSpirit(int idx)
    {
        if (GameManager.instance.spiritMaxLevel[idx] == GameManager.instance.spiritEnhanceLevel[idx]) return;
        if (!GameManager.instance.ChangeDiamond(-int.Parse(spiritEnhanceCostText[idx].text))) return;
        GameManager.instance.spiritEnhanceLevel[idx]++;
        ButtonSpiritEnhancePanelOpen();
    }

    //로비의 링 콜렉션(탑의 지식) 오픈 버튼이 눌렸을 때 불린다.
    public void ButtonRingCollectionPanelOpen()
    {
        for (int i = 0; i < lobbyRingCollectionRingDiamonds.Length; i++)
        {
            lobbyRingCollectionRingDiamonds[i].SetActive(false);
            for (int j = 0; j < 5; j++)
                if (GameManager.instance.ringCollectionProgress[i, j] == GameManager.instance.ringCollectionMaxProgress[i, j])
                {
                    lobbyRingCollectionRingDiamonds[i].SetActive(true);
                    break;
                }
        }
        ButtonRingCollectionSelectRing(0);
        lobbyRingCollectionPanel.SetActive(true);
    }

    //로비의 링 콜렉션 창에서 특정 링을 선택하는 버튼이 눌렸을 때 불린다.
    public void ButtonRingCollectionSelectRing(int id)
    {
        for (int i = 0; i < lobbyRingCollectionSelectCircle.Length; i++)
            lobbyRingCollectionSelectCircle[i].SetActive(id == i);
        lobbyRingCollectionRingNameText.text = GameManager.instance.baseRings[id].name + " 링";
        for (int i = 0; i < 5; i++)
        {
            int progress = GameManager.instance.ringCollectionProgress[id, i];
            int maxProgress = GameManager.instance.ringCollectionMaxProgress[id, i];
            lobbyRingCollectionQuestDiamonds[i].SetActive(progress == maxProgress);
            if (progress == -1)
            {
                progress = maxProgress;
                lobbyRingCollectionQuestDiamondsAmountText[i].text = "획득\n완료";
            }
            else lobbyRingCollectionQuestDiamondsAmountText[i].text = GameManager.instance.ringCollecionRewardAmount[i].ToString();
            lobbyRingCollectionProgressText[i].text = string.Format("{0}/{1}", progress, maxProgress);
            lobbyRingCollectionProgressBar[i].sizeDelta = new Vector2(400 * (float)progress / maxProgress, lobbyRingCollectionProgressBar[i].rect.height);
        }
    }

    //로비의 링 콜렉션 창에서 링 정보를 오픈하는 버튼이 눌렸을 때 불린다.
    public void ButtonRingCollectionRingInfoOpen()
    {
        int tarRingID = -1;
        for (int i = 0; i < GameManager.instance.baseRings.Count; i++)
            if (GameManager.instance.baseRings[i].name + " 링" == lobbyRingCollectionRingNameText.text)
            {
                tarRingID = i;
                break;
            }
        OpenRingInfoPanel(tarRingID);
    }
    
    //로비의 링 콜렉션 창에서 리워드를 획득하는 버튼이 눌렸을 때 불린다.
    public void ButtonRingCollecionRequestReward(int idx)
    {
        //영향을 받는 링을 찾는다.
        int tarRingID = -1;
        for (int i = 0; i < GameManager.instance.baseRings.Count; i++)
            if (GameManager.instance.baseRings[i].name + " 링" == lobbyRingCollectionRingNameText.text)
            {
                tarRingID = i;
                break;
            }

        //보상을 획득 가능한지 확인한다.
        if (GameManager.instance.ringCollectionProgress[tarRingID, idx] != GameManager.instance.ringCollectionMaxProgress[tarRingID, idx]) return;

        //해당 콜렉션 퀘스트의 다이아몬드를 획득하고 획득 가능 표시를 없앤다.
        GameManager.instance.ChangeDiamond(GameManager.instance.ringCollecionRewardAmount[idx]);
        lobbyRingCollectionQuestDiamondsAmountText[idx].text = "획득\n완료";
        lobbyRingCollectionQuestDiamonds[idx].SetActive(false);
        GameManager.instance.ringCollectionProgress[tarRingID, idx] = -1;

        //더 이상 현재 링에서 다이아몬드를 획득할 일이 없다면 획득 가능 표시를 없앤다.
        lobbyRingCollectionRingDiamonds[tarRingID].SetActive(false);
        for (int j = 0; j < 5; j++)
            if (GameManager.instance.ringCollectionProgress[tarRingID, j] == GameManager.instance.ringCollectionMaxProgress[tarRingID, j])
            {
                lobbyRingCollectionRingDiamonds[tarRingID].SetActive(true);
                break;
            }
    }

    //로비의 유물 콜렉션(탑의 지식) 오픈 버튼이 눌렸을 때 불린다.
    public void ButtonRelicCollectionPanelOpen()
    {
        for (int i = 0; i < lobbyRelicCollectionRelicDiamonds.Length; i++)
        {
            lobbyRelicCollectionRelicDiamonds[i].SetActive(false);
            for (int j = 0; j < 5; j++)
                if (GameManager.instance.relicCollectionProgress[i, j] == GameManager.instance.relicCollectionMaxProgress[i, j])
                {
                    lobbyRelicCollectionRelicDiamonds[i].SetActive(true);
                    break;
                }
        }
        ButtonRelicCollectionSelectRelic(0);
        lobbyRelicCollectionPanel.SetActive(true);
    }

    //로비의 유물 콜렉션 창에서 특정 유물을 선택하는 버튼이 눌렸을 때 불린다.
    public void ButtonRelicCollectionSelectRelic(int id)
    {
        for (int i = 0; i < lobbyRelicCollectionSelectSquare.Length; i++)
            lobbyRelicCollectionSelectSquare[i].SetActive(id == i);
        lobbyRelicCollectionRelicNameText.text = GameManager.instance.baseRelics[id].name;
        for (int i = 0; i < 5; i++)
        {
            int progress = GameManager.instance.relicCollectionProgress[id, i];
            int maxProgress = GameManager.instance.relicCollectionMaxProgress[id, i];
            lobbyRelicCollectionQuestDiamonds[i].SetActive(progress == maxProgress);
            if (progress == -1)
            {
                progress = maxProgress;
                lobbyRelicCollectionQuestDiamondsAmountText[i].text = "획득\n완료";
            }
            else lobbyRelicCollectionQuestDiamondsAmountText[i].text = GameManager.instance.ringCollecionRewardAmount[i].ToString();
            lobbyRelicCollectionProgressText[i].text = string.Format("{0}/{1}", progress, maxProgress);
            lobbyRelicCollectionProgressBar[i].sizeDelta = new Vector2(400 * (float)progress / maxProgress, lobbyRelicCollectionProgressBar[i].rect.height);
        }
    }


    //로비의 유물 콜렉션 창에서 유물 정보를 오픈하는 버튼이 눌렸을 때 불린다.
    public void ButtonRelicCollectionRelicInfoOpen()
    {
        int tarRelicID = -1;
        for (int i = 0; i < GameManager.instance.baseRelics.Count; i++)
            if (GameManager.instance.baseRelics[i].name == lobbyRelicCollectionRelicNameText.text)
            {
                tarRelicID = i;
                break;
            }
        OpenRelicInfoPanel(tarRelicID);
    }

    //로비의 유물 콜렉션 창에서 리워드를 획득하는 버튼이 눌렸을 때 불린다.
    public void ButtonRelicCollecionRequestReward(int idx)
    {
        //영향을 받는 유물을 찾는다.
        int tarRelicID = -1;
        for (int i = 0; i < GameManager.instance.baseRelics.Count; i++)
            if (GameManager.instance.baseRelics[i].name == lobbyRelicCollectionRelicNameText.text)
            {
                tarRelicID = i;
                break;
            }

        //보상을 획득 가능한지 확인한다.
        if (GameManager.instance.relicCollectionProgress[tarRelicID, idx] != GameManager.instance.relicCollectionMaxProgress[tarRelicID, idx]) return;

        //해당 콜렉션 퀘스트의 다이아몬드를 획득하고 획득 가능 표시를 없앤다.
        GameManager.instance.ChangeDiamond(GameManager.instance.ringCollecionRewardAmount[idx]);
        lobbyRelicCollectionQuestDiamondsAmountText[idx].text = "획득\n완료";
        lobbyRelicCollectionQuestDiamonds[idx].SetActive(false);
        GameManager.instance.relicCollectionProgress[tarRelicID, idx] = -1;

        //더 이상 현재 유물에서 다이아몬드를 획득할 일이 없다면 획득 가능 표시를 없앤다.
        lobbyRelicCollectionRelicDiamonds[tarRelicID].SetActive(false);
        for (int j = 0; j < 5; j++)
            if (GameManager.instance.relicCollectionProgress[tarRelicID, j] == GameManager.instance.relicCollectionMaxProgress[tarRelicID, j])
            {
                lobbyRelicCollectionRelicDiamonds[tarRelicID].SetActive(true);
                break;
            }
    }

    public void ButtonMonsterCollectionPanelOpen()
    {

    }


    //로비로 장면 전환 시 불린다.
    public void ChangeSceneToLobby(int a, int b)
    {
        GameManager.instance.ResetBases(true);

        //콜렉션에서 다이아몬드를 획득 가능한 경우 표시한다.
        lobbyCollectionDiamonds[0].SetActive(false);
        for (int i = 0; i < lobbyRingCollectionRingDiamonds.Length; i++)
            for (int j = 0; j < lobbyRingCollectionQuestDiamonds.Length; j++)
                if (GameManager.instance.ringCollectionProgress[i, j] == GameManager.instance.ringCollectionMaxProgress[i, j])
                {
                    lobbyCollectionDiamonds[0].SetActive(true);
                    break;
                }
        lobbyCollectionDiamonds[1].SetActive(false);
        for (int i = 0; i < lobbyRelicCollectionRelicDiamonds.Length; i++)
            for (int j = 0; j < lobbyRelicCollectionQuestDiamonds.Length; j++)
                if (GameManager.instance.relicCollectionProgress[i, j] == GameManager.instance.relicCollectionMaxProgress[i, j])
                {
                    lobbyCollectionDiamonds[1].SetActive(true);
                    break;
                }

        //FloorManager.instance.ResetFloor();

        lobbyPanel.SetActive(true);
        lobbyHardModeToggleButton.isOn = false;

        battleArrangeFail.SetActive(false);
        battleDeckPanel.SetActive(false);
        ringSelectionPanel.SetActive(false);
        playerStatusPanel.SetActive(false);
        ringInfoPanel.SetActive(false);
        relicInfoPanel.SetActive(false);
        gameStartPanel.SetActive(false);
        gameEndPanel.gameObject.SetActive(false);
    }

    void InvokeReloadAndCloseRingSelectionPanel()
    {
        OpenRingSelectionPanel(-1);
        Invoke("InvokeCloseRingSelectionPanel", 0.5f);
    }

    void InvokeCloseRingSelectionPanel()
    {
        ClosePanel(1);
    }
}
