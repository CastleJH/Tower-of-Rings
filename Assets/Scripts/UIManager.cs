using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject battleDeckPanel;
    public GameObject mapPanel;
    public Image[] battleDeckRingImages;  //덱 그림
    public TextMeshProUGUI[] battleDeckRingRPText;
    public GameObject[] battleRPNotEnough;
    public TextMeshProUGUI battleRPText;
    public GameObject battleArrangeFail;
    public TextMeshProUGUI battleArrangeFailText;
    public Image[] mapRow1, mapRow2, mapRow3, mapRow4, mapRow5, mapRow6, mapRow7, mapRow8, mapRow9;
    public Image[][] maps;
    public RectTransform playerMarker;

    public GameObject playerStatusPanel;
    public Image[] playerStatusRingImage;
    public Image[] playerStatusRingUpgradeImage;
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
    public TextMeshProUGUI relicInfoBaseEffectText;
    public TextMeshProUGUI relicInfoCursedEffectText;
    public GameObject relicInfoCursedNotify;
    public GameObject relicInfoTakeButton;

    public GameObject ringSelectionPanel;
    public Image[] ringSelectionRingImage;
    public Image[] ringSelectionRingUpgradeImage;
    public TextMeshProUGUI[] ringSelectionRPText;
    public Image[] ringSelectionButtonImage;
    public TextMeshProUGUI[] ringSelectionButtonText;

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
        if (BattleManager.instance.isBattlePlaying)
        {
            if (battleRPNotEnough[index].activeSelf) return;
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

    //전투에서 링 생성 버튼 누르기를 중지하면 불린다.
    public void ButtonGenerateRingUp()
    {
        //checkBattleRingDetailOn = false;
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

    //전투 UI에서 덱에 있는 링의 이미지를 갱신한다.
    public void SetBattleDeckRingImage(int index)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingImages[index].sprite = GameManager.instance.emptyRingSprite;
        else battleDeckRingImages[index].sprite = GameManager.instance.ringSprites[DeckManager.instance.deck[index]];
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

    //전투 UI에서 덱 부분을 전체적으로 변경한다.(링 스프라이트 및 소모 RP)
    public void SetBattleDeckRingImageAndRPAll()
    {
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)
        {
            SetBattleDeckRingImage(i);
            if (i < DeckManager.instance.deck.Count) SetBattleDeckRingRPText(i, (int)GameManager.instance.ringDB[DeckManager.instance.deck[i]].baseRP);
            else SetBattleDeckRingRPText(i, 0);
        }
    }

    //맵을 초기화한다.
    public void InitializeMap()
    {
        for (int i = 1; i <= 9; i++)
            for (int j = 1; j <= 9; j++)
            {
                if (FloorManager.instance.floor.rooms[i, j].type != -1) maps[i][j].sprite = GameManager.instance.mapRoomSprites[FloorManager.instance.floor.rooms[i, j].type];
                maps[i][j].color = new Color32(0, 0, 0, 0);
            }
    }

    //맵의 일부분을 밝히고, 마커를 해당 위치로 이동한다.
    public void RevealMapArea(int x, int y)
    {
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };
        int nx, ny;

        maps[x][y].sprite = GameManager.instance.mapRoomSprites[FloorManager.instance.floor.rooms[x, y].type];
        maps[x][y].color = Color.white;
        playerMarker.anchoredPosition = maps[x][y].rectTransform.anchoredPosition;

        for (int i = 0; i < 4; i++)
        {
            nx = x + dx[i];
            ny = y + dy[i];
            if (FloorManager.instance.floor.rooms[nx, ny].type != -1 && FloorManager.instance.floor.rooms[nx, ny].type != 10 && !FloorManager.instance.floor.rooms[nx, ny].visited)
            {
                //if (FloorManager.instance.floor.rooms[nx, ny].type < 8) maps[nx][ny].sprite = GameManager.instance.mapRoomSprites[0];
                //else maps[nx][ny].sprite = GameManager.instance.mapRoomSprites[FloorManager.instance.floor.rooms[nx, ny].type];
                //maps[nx][ny].sprite = GameManager.instance.mapRoomSprites[FloorManager.instance.floor.rooms[nx, ny].type];
                maps[nx][ny].color = Color.white;
            }
        }
    }

    public void TurnDeckOnOff(bool isOn)
    {
        battleDeckPanel.SetActive(isOn);
    }
    public void TurnMapOnOff(bool isOn)
    {
        mapPanel.SetActive(isOn);
    }

    public void OpenPlayerStatusPanel()
    {
        int i;
        int type;
        for (i = 0; i < DeckManager.instance.deck.Count; i++)
        {
            type = DeckManager.instance.deck[i];
            playerStatusRingImage[i].sprite = GameManager.instance.ringSprites[type];
            playerStatusRingUpgradeImage[i].sprite = GameManager.instance.ringUpgradeSprites[GameManager.instance.ringDB[type].level];
            playerStatusRPText[i].text = GameManager.instance.ringDB[type].baseRP.ToString();
            playerStatusRingUpgradeImage[i].gameObject.SetActive(true);
            playerStatusRPText[i].gameObject.SetActive(true);
        }
        for (;i < playerStatusRingImage.Length; i++)
        {
            playerStatusRingImage[i].sprite = GameManager.instance.emptyRingSprite;
            playerStatusRingUpgradeImage[i].gameObject.SetActive(false);
            playerStatusRPText[i].gameObject.SetActive(false);
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
        BaseRing baseRing = GameManager.instance.ringDB[id];
        ringInfoRingImage.sprite = GameManager.instance.ringSprites[id];
        ringInfoRingUpgradeImage.sprite = GameManager.instance.ringUpgradeSprites[baseRing.level];
        ringInfoRPText.text = ((int)baseRing.baseRP).ToString();
        ringInfoNameText.text = baseRing.name;
        ringInfoATKText.text = baseRing.baseATK.ToString();
        ringInfoSPDText.text = baseRing.baseSPD.ToString();
        ringInfoRNGText.text = baseRing.range.ToString();
        ringInfoTARText.text = baseRing.baseNumTarget.ToString();
        ringInfoBaseText.text = baseRing.description;
        ringInfoSameSynergyText.text = baseRing.toSame;
        ringInfoAllSynergyText.text = baseRing.toAll;

        ringInfoTakeButton.gameObject.SetActive(!playerStatusPanel.activeSelf);

        ringInfoPanel.SetActive(true);
    }

    public void OpenRelicInfoPanel(int id, bool takeButtonOn)
    {
        BaseRelic baseRelic = GameManager.instance.relicDB[id];


        relicInfoTakeButton.gameObject.SetActive(!playerStatusPanel.activeSelf);

        relicInfoPanel.SetActive(true);
    }
}
