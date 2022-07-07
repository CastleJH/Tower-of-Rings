using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;


    public Image[] battleDeckRingImages;  //�� �׸�
    public TextMeshProUGUI[] battleDeckRingRPText;
    public GameObject[] battleRPNotEnough;
    public TextMeshProUGUI battleRPText;
    public GameObject battleArrangeFail;
    public TextMeshProUGUI battleArrangeFailText;

    //bool checkBattleRingDetailOn;
    //float battleRingDetailLongClickTime;
    void Awake()
    {
        instance = this;

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

    //���� UI���� ���� �ִ� ���� RP ��� �ؽ�Ʈ�� �����Ѵ�. rp�� -1�� �ָ� MAX�� �ٲ۴�.
    public void SetBattleDeckRingRPText(int index, int rp)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingRPText[index].text = " ";
        else if (rp == -1) battleDeckRingRPText[index].text = "MAX";
        else battleDeckRingRPText[index].text = rp.ToString();
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
}
