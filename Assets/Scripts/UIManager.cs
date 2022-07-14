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
    public Image[] battleDeckRingImages;  //�� �׸�
    public TextMeshProUGUI[] battleDeckRingRPText;
    public GameObject[] battleRPNotEnough;
    public TextMeshProUGUI battleRPText;
    public GameObject battleArrangeFail;
    public TextMeshProUGUI battleArrangeFailText;
    public Image[] mapRow1, mapRow2, mapRow3, mapRow4, mapRow5, mapRow6, mapRow7, mapRow8, mapRow9;
    public Image[][] maps;
    public RectTransform playerMarker;

    //bool checkBattleRingDetailOn;
    //float battleRingDetailLongClickTime;
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

    //�������� �� ���� ��ư�� ���� ��쿡 �Ҹ���.
    public void ButtonGenerateRing(int index)
    {
        if (Input.touchCount > 1) return;
        if (BattleManager.instance.isBattlePlaying)
        {
            if (battleRPNotEnough[index].activeSelf) return;
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

    //�������� �� ���� ��ư �����⸦ �����ϸ� �Ҹ���.
    public void ButtonGenerateRingUp()
    {
        //checkBattleRingDetailOn = false;
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

    //���� UI���� ���� �ִ� ���� �̹����� �����Ѵ�.
    public void SetBattleDeckRingImage(int index)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingImages[index].sprite = GameManager.instance.emptyRingSprite;
        else battleDeckRingImages[index].sprite = GameManager.instance.ringSprites[DeckManager.instance.deck[index]];
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

    //���� UI���� �� �κ��� ��ü������ �����Ѵ�.(�� ��������Ʈ �� �Ҹ� RP)
    public void SetBattleDeckRingImageAndRPAll()
    {
        for (int i = 0; i < DeckManager.instance.maxDeckLength; i++)
        {
            SetBattleDeckRingImage(i);
            if (i < DeckManager.instance.deck.Count) SetBattleDeckRingRPText(i, (int)GameManager.instance.ringstoneDB[DeckManager.instance.deck[i]].baseRP);
            else SetBattleDeckRingRPText(i, 0);
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
                if (FloorManager.instance.floor.rooms[nx, ny].type < 8) maps[nx][ny].sprite = GameManager.instance.mapRoomSprites[0];
                else maps[nx][ny].sprite = GameManager.instance.mapRoomSprites[FloorManager.instance.floor.rooms[nx, ny].type];
                maps[nx][ny].color = Color.gray;
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
}
