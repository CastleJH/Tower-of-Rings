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

    bool checkBattleRingDetailOn;
    float battleRingDetailLongClickTime;
    void Awake()
    {
        instance = this;

        checkBattleRingDetailOn = false;
        battleRingDetailLongClickTime = 0.0f;
    }

    //�� ���� ��ư�� ���� ��쿡 �Ҹ���.
    public void ButtonGenerateRing(int index)
    {
        if (Input.touchCount > 1) return;
        if (BattleManager.instance.isBattlePlaying)
        {
            DeckManager.instance.isGenRing = true;
            if (index == DeckManager.instance.maxDeckLength) //���� ��ư�̶��
            {
                return;
            }
            else if (index < DeckManager.instance.deck.Count) //�� ���� �ƴ϶��
            {
                DeckManager.instance.genRing = GameManager.instance.GetRingFromPool();
                DeckManager.instance.genRing.InitializeRing(DeckManager.instance.deck[index]);
                DeckManager.instance.genRing.transform.localScale = Vector3.one;
                DeckManager.instance.genRing.collider.enabled = false;
                DeckManager.instance.genRing.gameObject.SetActive(true);
                battleRingDetailLongClickTime = 0.0f;
                checkBattleRingDetailOn = true;
            }
            else DeckManager.instance.isGenRing = false;
        }
    }

    //�� ���� ��ư �����⸦ �����ϸ� �Ҹ���.
    public void ButtonGenerateRingUp()
    {
        checkBattleRingDetailOn = false;
    }

    //��Ʋ UI�� �� �� RP ��� �ؽ�Ʈ�� �����Ѵ�.
    public void SetBattleDeckRingRPText(int index)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingRPText[index].text = " ";
        else battleDeckRingRPText[index].text = GameManager.instance.ringstoneDB[DeckManager.instance.deck[index]].baseRP.ToString();
    }

    //��Ʋ UI�� �� �� �̹����� �����Ѵ�.
    public void SetBattleDeckRingImage(int index)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingImages[index].sprite = GameManager.instance.emptyRingSprite;
        else battleDeckRingImages[index].sprite = GameManager.instance.ringSprites[DeckManager.instance.deck[index]];

    }
}
