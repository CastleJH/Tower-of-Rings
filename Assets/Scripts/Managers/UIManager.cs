using System.Collections;
using System.Collections.Generic;
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
    public GameObject gameEndingPanel;
    
    public GameObject mapPanel;
    public Image[] mapRow1, mapRow2, mapRow3, mapRow4, mapRow5, mapRow6, mapRow7, mapRow8, mapRow9;
    public Image[][] maps;
    public RectTransform playerMarker;

    public TextMeshProUGUI playerGoldText;
    public TextMeshProUGUI playerDiamondText;
    public TextMeshProUGUI playerHPText;

    public GameObject battleArrangeFail;
    public TextMeshProUGUI battleArrangeFailText;
    //public GameObject nextFloorButton;

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

    public GameObject relicInfoPanel;
    public Image relicInfoRelicImage;
    public TextMeshProUGUI relicInfoNameText;
    public TextMeshProUGUI relicInfoBaseEffectText;
    public TextMeshProUGUI relicInfoCursedEffectText;
    public GameObject relicInfoCursedImage;
    public GameObject relicInfoCursedNotify;
    public GameObject relicInfoTakeButton;


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
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 touchPos;

            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;
            touchParticle.gameObject.transform.position = Camera.main.ScreenToWorldPoint(touchPos);
            touchParticle.transform.Translate(Vector3.forward);
            touchParticle.Play();
        }
    
        if (gameStartPanel.activeSelf)
        {
            titleTextBlinkTime += Time.deltaTime;
            if (titleTextBlinkTime > 0.5f)
            {
                titleTextBlinkTime = 0;
                gameStartText.SetActive(!gameStartText.activeSelf);
            }
        }
    }

    //�������� �� ���� ��ư�� ���� ��쿡 �Ҹ���.
    public void ButtonGenerateRing(int index)
    {
        if (Input.touchCount > 1) return;
        if (BattleManager.instance.isBattlePlaying && Time.timeScale != 0)
        {
            if (battleDeckRPNotEnoughCover[index].activeSelf) return;
            DeckManager.instance.isEditRing = true;
            if (index == DeckManager.instance.maxDeckLength) //���� ��ư�̶��
            {
                return;
            }
            else if (index < DeckManager.instance.deck.Count) //�� ���� �ƴ϶��
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

    //���� �� �� ��ġ ���� ������ �˸��� UI�� �����ش�.
    public void SetBattleArrangeFail(string str)
    {
        if (str == null) battleArrangeFail.SetActive(false);
        else
        {
            battleArrangeFailText.text = str;
            battleArrangeFail.SetActive(true);
        }
    }

    //���� �ʱ�ȭ�Ѵ�.
    public void InitializeMap()
    {
        for (int i = 1; i <= 9; i++)
            for (int j = 1; j <= 9; j++)
                maps[i][j].color = new Color32(0, 0, 0, 0);
    }

    //���� �Ϻκ��� ������, ��Ŀ�� �ش� ��ġ�� �̵��Ѵ�.
    public void RevealMapAndMove(int x, int y)
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
                if (room.visited && (room.type == 1 || room.type == 9)) maps[nx][ny].sprite = GameManager.instance.mapRoomSprites[0];
                else if (room.type != 8 && room.visited && room.items.Count == 0) maps[nx][ny].sprite = GameManager.instance.mapRoomSprites[0];
                maps[nx][ny].color = Color.white;
            }
        }
    }

    public void TurnMapOnOff(bool isOn)
    {
        mapPanel.SetActive(isOn);
    }

    public void OpenBattleDeckPanel()
    {
        int i;
        int type;
        battleDeckSpeedButtonImage.sprite = GameManager.instance.speedSprites[0];
        for (i = 0; i < DeckManager.instance.deck.Count; i++)
        {
            type = DeckManager.instance.deck[i];
            battleDeckRingImage[i].sprite = GameManager.instance.ringSprites[type];
            if (GameManager.instance.ringDB[type].level == GameManager.instance.ringDB[type].maxlvl) battleDeckRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[0];
            else battleDeckRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[GameManager.instance.ringDB[type].level];
            battleDeckRingRPText[i].text = GameManager.instance.ringDB[type].baseRP.ToString();
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

    //���� UI���� ���� �ִ� ���� RP ��� �ؽ�Ʈ�� �����Ѵ�.
    public void SetBattleDeckRingRPText(int index, int rp)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingRPText[index].text = " ";
        else battleDeckRingRPText[index].text = rp.ToString();
    }

    //���� UI���� ���� �ִ� ���� RP ��� �ؽ�Ʈ�� ���ڿ��� �����Ѵ�.
    public void SetBattleDeckRingRPText(int index, string str)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingRPText[index].text = " ";
        battleDeckRingRPText[index].text = str;
    }

    public void OpenRingSelectionPanel(int mode)
    {

        ringSelectionEffectAnimation.gameObject.SetActive(false);

        int i;
        int type;
        if (mode == 0) ringSelectionTypeText.text = "�ı��� ���� �����ϼ���.";
        else if (mode == 1) ringSelectionTypeText.text = "������ ���� �����ϼ���.";
        for (i = 0; i < DeckManager.instance.deck.Count; i++)
        {
            type = DeckManager.instance.deck[i];
            ringSelectionRingImage[i].sprite = GameManager.instance.ringSprites[type];
            ringSelectionButtonImage[i].gameObject.SetActive(true);
            if (GameManager.instance.ringDB[type].level == GameManager.instance.ringDB[type].maxlvl)
            {
                ringSelectionRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[0];
                if (mode == 1) ringSelectionButtonImage[i].gameObject.SetActive(false);
            }
            else ringSelectionRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[GameManager.instance.ringDB[type].level];
            ringSelectionRPText[i].text = GameManager.instance.ringDB[type].baseRP.ToString();
            ringSelectionRingUpgradeImage[i].gameObject.SetActive(true);
            ringSelectionRP[i].SetActive(true);

            if (mode != -1) ringSelectionButtonImage[i].sprite = GameManager.instance.buttonSprites[mode];
            else ringSelectionButtonImage[i].gameObject.SetActive(false);
            if (mode == 0) ringSelectionButtonText[i].text = "�ı�";
            else if (mode == 1) ringSelectionButtonText[i].text = "����";
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

    public void OpenPlayerStatusPanel()
    {
        Time.timeScale = 0;
        int i;
        int type;
        for (i = 0; i < DeckManager.instance.deck.Count; i++)
        {
            type = DeckManager.instance.deck[i];
            playerStatusRingImage[i].sprite = GameManager.instance.ringSprites[type];
            if (GameManager.instance.ringDB[type].level == GameManager.instance.ringDB[type].maxlvl) playerStatusRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[0];
            else playerStatusRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[GameManager.instance.ringDB[type].level];
            playerStatusRPText[i].text = GameManager.instance.ringDB[type].baseRP.ToString();
            playerStatusRingUpgradeImage[i].gameObject.SetActive(true);
            playerStatusRP[i].SetActive(true);
        }
        for (;i < playerStatusRingImage.Length; i++)
        {
            playerStatusRingImage[i].sprite = GameManager.instance.emptyRingSprite;
            playerStatusRingUpgradeImage[i].gameObject.SetActive(false);
            playerStatusRP[i].SetActive(false);
        }

        for (i = 0; i < GameManager.instance.relics.Count; i++)
        {
            playerStatusRelicImage[i].sprite = GameManager.instance.relicSprites[GameManager.instance.relics[i]];
            playerStatusRelicCursedImage[i].SetActive(GameManager.instance.relicDB[GameManager.instance.relics[i]].isCursed);
            playerStatusRelicImage[i].gameObject.SetActive(true);
        }
        for (; i < playerStatusRelicImage.Length; i++)
            playerStatusRelicImage[i].gameObject.SetActive(false);

        playerStatusPanel.SetActive(true);
    }

    public void OpenRingInfoPanel(int id)
    {
        BaseRing baseRing = GameManager.instance.ringDB[id];
        ringInfoRingImage.sprite = GameManager.instance.ringSprites[id];
        if (baseRing.level == baseRing.maxlvl) ringInfoRingUpgradeImage.sprite = GameManager.instance.ringInfoUpgradeSprites[0];
        else ringInfoRingUpgradeImage.sprite = GameManager.instance.ringInfoUpgradeSprites[baseRing.level];
        ringInfoRPText.text = ((int)baseRing.baseRP).ToString();
        ringInfoNameText.text = baseRing.name;
        ringInfoATKText.text = baseRing.baseATK.ToString();
        ringInfoSPDText.text = baseRing.baseSPD.ToString();
        ringInfoRNGText.text = (baseRing.range - 2).ToString();
        ringInfoTARText.text = baseRing.baseNumTarget.ToString();
        ringInfoBaseText.text = baseRing.description;
        ringInfoSameSynergyText.text = baseRing.toSame;
        ringInfoAllSynergyText.text = baseRing.toAll;

        ringInfoTakeButton.gameObject.SetActive(!playerStatusPanel.activeSelf && !ringSelectionPanel.activeSelf);

        ringInfoPanel.SetActive(true);
    }

    public void OpenRelicInfoPanel(int id)
    {
        BaseRelic baseRelic = GameManager.instance.relicDB[id];

        relicInfoRelicImage.sprite = GameManager.instance.relicSprites[id];
        relicInfoNameText.text = baseRelic.name;
        relicInfoBaseEffectText.text = baseRelic.pureDescription;
        relicInfoCursedEffectText.text = baseRelic.cursedDescription;

        relicInfoCursedNotify.gameObject.SetActive(baseRelic.isCursed);
        relicInfoCursedImage.gameObject.SetActive(baseRelic.isCursed);
        if (baseRelic.isCursed)
        {
            relicInfoBaseEffectText.color = new Color32(70, 70, 70, 255);
            relicInfoCursedEffectText.color = new Color32(200, 200, 200, 255);
        }
        else
        {
            relicInfoBaseEffectText.color = new Color32(200, 200, 200, 255);
            relicInfoCursedEffectText.color = new Color32(70, 70, 70, 255);
        }

        relicInfoTakeButton.gameObject.SetActive(!playerStatusPanel.activeSelf);

        relicInfoPanel.SetActive(true);
    }

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
        }
    }
    
    public void ButtonPlayerStatusOpen()
    {
        OpenPlayerStatusPanel();
    }

    public void ButtonRingInfoOpen(int deckIdx)
    {
        if (deckIdx < DeckManager.instance.deck.Count)
        {
            OpenRingInfoPanel(DeckManager.instance.deck[deckIdx]);
        }
    }

    public void ButtonRelicInfoOpen(int listIdx)
    {
        if (listIdx < GameManager.instance.relics.Count)
        {
            OpenRelicInfoPanel(GameManager.instance.relics[listIdx]);
        }
    }

    public void ButtonSelectRing(int deckIdx)
    {
        if (deckIdx < DeckManager.instance.deck.Count)
        {
            if (ringSelectionButtonText[0].text == "�ı�")
            {
                DeckManager.instance.RemoveRingFromDeck(deckIdx);
                FloorManager.instance.RemoveItem(FloorManager.instance.lastTouchItem, true);
                ringSelectionEffectImage.sprite = GameManager.instance.itemSprites[1];
            }
            else
            {
                GameManager.instance.ringDB[DeckManager.instance.deck[deckIdx]].Upgrade(1.0f);
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


    public void ButtonTakeRing()
    {
        int type = -1;
        for (int i = 0; i < GameManager.instance.ringDB.Count; i++)
            if (GameManager.instance.ringDB[i].name == ringInfoNameText.text)
            {
                type = i;
                break;
            }

        DeckManager.instance.AddRingToDeck(type);

        for (int i = 0; i < FloorManager.instance.curRoom.items.Count; i++)
            if (FloorManager.instance.curRoom.items[i].itemType == 1000 + type)
            {
                FloorManager.instance.RemoveItem(FloorManager.instance.curRoom.items[i], false);
                break;
            }
        ClosePanel(3);
    }

    public void ButtonTakeRelic()
    {
        int type = -1;
        for (int i = 0; i < GameManager.instance.relicDB.Count; i++)
            if (GameManager.instance.relicDB[i].name == relicInfoNameText.text)
            {
                type = i;
                break;
            }

        GameManager.instance.AddRelicToDeck(type);


        //������ �濡�� ȹ���� ���̶�� ���� Ȯ���� ����
        if (FloorManager.instance.curRoom.type == 4)
        {
            if (Random.Range(0.0f, 1.0f) <= 0.2f)
            {
                GameManager.instance.relicDB[type].isCursed = true;
                GameManager.instance.cursedRelics.Add(type);
            }
        }

        for (int i = 0; i < FloorManager.instance.curRoom.items.Count; i++)
            if (FloorManager.instance.curRoom.items[i].itemType == 2000 + type)
            {
                FloorManager.instance.RemoveItem(FloorManager.instance.curRoom.items[i], false);
                break;
            }

        if (type == 1)
        {
            if (!GameManager.instance.relicDB[1].isCursed)
            {
                GameManager.instance.playerMaxHP += 20;
                GameManager.instance.ChangePlayerCurHP(20);
            }
            else
            {
                GameManager.instance.playerMaxHP -= 20;
            }
        }
        ClosePanel(4);
    }

    public void ButtonFasterSpeed()
    {
        if (BattleManager.instance.isBattlePlaying)
        {
            if (Time.timeScale < 7.5) Time.timeScale += Time.timeScale;
            else Time.timeScale = 1;
            battleDeckSpeedButtonImage.sprite = GameManager.instance.speedSprites[(int)Mathf.Round(Time.timeScale) / 2];
        }
    }
    public void ButtonNormalSpeed()
    {
        if (BattleManager.instance.isBattlePlaying)
        {
            battleDeckSpeedButtonImage.sprite = GameManager.instance.speedSprites[0];
            Time.timeScale = 1;
        }
    }

    public void ButtonStartGame()
    {
        SceneChanger.instance.ChangeScene(ChangeSceneFromStartToLobby, 0, 0);
    }

    void ChangeSceneFromStartToLobby(int a, int b)
    {
        gameStartPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public void ButtonEnterTower()
    {
        GameManager.instance.TowerStart();
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
