using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject gameStartPanel;
    public Image sceneChanger;

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

    public GameObject ringSelectionPanel;
    public TextMeshProUGUI ringSelectionTypeText;
    public Image[] ringSelectionRingImage;
    public Image[] ringSelectionRingUpgradeImage;
    public GameObject[] ringSelectionRP;
    public TextMeshProUGUI[] ringSelectionRPText;
    public Image[] ringSelectionButtonImage;
    public TextMeshProUGUI[] ringSelectionButtonText;

    public GameObject playerStatusPanel;
    public Image[] playerStatusRingImage;
    public Image[] playerStatusRingUpgradeImage;
    public GameObject[] playerStatusRP;
    public TextMeshProUGUI[] playerStatusRPText;
    public Image[] playerStatusRelicImage;

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
    public GameObject relicInfoCursedNotify;
    public GameObject relicInfoTakeButton;


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
        //checkBattleRingDetailOn = false;
        //battleRingDetailLongClickTime = 0.0f;
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
    public void RevealMapArea(int x, int y)
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
        for (i = 0; i < DeckManager.instance.deck.Count; i++)
        {
            type = DeckManager.instance.deck[i];
            battleDeckRingImage[i].sprite = GameManager.instance.ringSprites[type];
            battleDeckRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[GameManager.instance.ringDB[type].level];
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

    public void OpenRingSelectionPanel(int isUpgrade)
    {
        Time.timeScale = 0;
        int i;
        int type;
        if (isUpgrade == 0) ringSelectionTypeText.text = "파괴할 링을 선택하세요.";
        else ringSelectionTypeText.text = "제련할 링을 선택하세요.";
        for (i = 0; i < DeckManager.instance.deck.Count; i++)
        {
            type = DeckManager.instance.deck[i];
            ringSelectionRingImage[i].sprite = GameManager.instance.ringSprites[type];
            ringSelectionRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[GameManager.instance.ringDB[type].level];
            ringSelectionRPText[i].text = GameManager.instance.ringDB[type].baseRP.ToString();
            ringSelectionRingUpgradeImage[i].gameObject.SetActive(true);
            ringSelectionRP[i].SetActive(true);

            ringSelectionButtonImage[i].sprite = GameManager.instance.buttonSprites[isUpgrade];
            if (isUpgrade == 0) ringSelectionButtonText[i].text = "파괴";
            else ringSelectionButtonText[i].text = "제련";
            ringSelectionButtonImage[i].gameObject.SetActive(true);
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
            playerStatusRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[GameManager.instance.ringDB[type].level];
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
            playerStatusRelicImage[i].gameObject.SetActive(true);
        }
        for (; i < playerStatusRelicImage.Length; i++)
            playerStatusRelicImage[i].gameObject.SetActive(false);

        playerStatusPanel.SetActive(true);
    }

    public void OpenRingInfoPanel(int id)
    {
        Time.timeScale = 0;
        BaseRing baseRing = GameManager.instance.ringDB[id];
        ringInfoRingImage.sprite = GameManager.instance.ringSprites[id];
        ringInfoRingUpgradeImage.sprite = GameManager.instance.ringUpgradeSprites[baseRing.level];
        ringInfoRPText.text = ((int)baseRing.baseRP).ToString();
        ringInfoNameText.text = baseRing.name;
        ringInfoATKText.text = baseRing.baseATK.ToString();
        ringInfoSPDText.text = baseRing.baseSPD.ToString();
        ringInfoRNGText.text = (baseRing.range - 2).ToString();
        ringInfoTARText.text = baseRing.baseNumTarget.ToString();
        ringInfoBaseText.text = baseRing.description;
        ringInfoSameSynergyText.text = baseRing.toSame;
        ringInfoAllSynergyText.text = baseRing.toAll;

        ringInfoTakeButton.gameObject.SetActive(!playerStatusPanel.activeSelf);

        ringInfoPanel.SetActive(true);
    }

    public void OpenRelicInfoPanel(int id)
    {
        Time.timeScale = 0;
        BaseRelic baseRelic = GameManager.instance.relicDB[id];

        relicInfoRelicImage.sprite = GameManager.instance.relicSprites[id];
        relicInfoNameText.text = baseRelic.name;
        relicInfoBaseEffectText.text = baseRelic.pureDescription;
        relicInfoCursedEffectText.text = baseRelic.cursedDescription;

        relicInfoCursedNotify.gameObject.SetActive(baseRelic.isCursed);
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
                Time.timeScale = 1;
                ringSelectionPanel.SetActive(false);
                break;
            case 2:
                if (!BattleManager.instance.isBattlePlaying) Time.timeScale = 1;
                playerStatusPanel.SetActive(false);
                break;
            case 3:
                if (!playerStatusPanel.activeSelf && !ringSelectionPanel.activeSelf) Time.timeScale = 1;
                ringInfoPanel.SetActive(false);
                break;
            case 4:
                if (!playerStatusPanel.activeSelf) Time.timeScale = 1;
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
            if (ringSelectionButtonText[0].text == "파괴")
            {
                DeckManager.instance.RemoveRingFromDeck(DeckManager.instance.deck[deckIdx]);
                for (int i = 0; i < FloorManager.instance.curRoom.items.Count; i++)
                    if (FloorManager.instance.curRoom.items[i].itemType == 1)
                    {
                        FloorManager.instance.curRoom.RemoveItem(FloorManager.instance.curRoom.items[i]);
                        break;
                    }
            }
            else
            {
                GameManager.instance.ringDB[DeckManager.instance.deck[deckIdx]].Upgrade();
                for (int i = 0; i < FloorManager.instance.curRoom.items.Count; i++)
                    if (FloorManager.instance.curRoom.items[i].itemType == 0)
                    {
                        FloorManager.instance.curRoom.RemoveItem(FloorManager.instance.curRoom.items[i]);
                        break;
                    }
            }
            ClosePanel(1);
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
                FloorManager.instance.curRoom.RemoveItem(FloorManager.instance.curRoom.items[i]);
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


        for (int i = 0; i < FloorManager.instance.curRoom.items.Count; i++)
            if (FloorManager.instance.curRoom.items[i].itemType == 2000 + type)
            {
                FloorManager.instance.curRoom.RemoveItem(FloorManager.instance.curRoom.items[i]);
                break;
            }
        ClosePanel(4);
    }

    public void ButtonFasterSpeed()
    {
        if (BattleManager.instance.isBattlePlaying && Time.timeScale < 3.5f) Time.timeScale++;
    }

    public void ButtonNormalSpeed()
    {
        Time.timeScale = 1.0f;
    }

    public void ButtonGameStart()
    {
        GameManager.instance.GameStart();
    }
}
