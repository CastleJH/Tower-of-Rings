using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;


    public Image[] battleDeckRingImages;  //덱 그림
    public TextMeshProUGUI[] battleDeckRingRPText;

    bool checkBattleRingDetailOn;
    float battleRingDetailLongClickTime;
    void Awake()
    {
        instance = this;

        checkBattleRingDetailOn = false;
        battleRingDetailLongClickTime = 0.0f;
    }

    //링 생성 버튼이 눌린 경우에 불린다.
    public void ButtonGenerateRing(int index)
    {
        if (Input.touchCount > 1) return;
        if (BattleManager.instance.isBattlePlaying)
        {
            DeckManager.instance.isGenRing = true;
            if (index == DeckManager.instance.maxDeckLength) //제거 버튼이라면
            {
                return;
            }
            else if (index < DeckManager.instance.deck.Count) //빈 링이 아니라면
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

    //링 생성 버튼 누르기를 중지하면 불린다.
    public void ButtonGenerateRingUp()
    {
        checkBattleRingDetailOn = false;
    }

    //배틀 UI의 덱 링 RP 비용 텍스트를 갱신한다.
    public void SetBattleDeckRingRPText(int index)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingRPText[index].text = " ";
        else battleDeckRingRPText[index].text = GameManager.instance.ringstoneDB[DeckManager.instance.deck[index]].baseRP.ToString();
    }

    //배틀 UI의 덱 링 이미지를 변경한다.
    public void SetBattleDeckRingImage(int index)
    {
        if (index >= DeckManager.instance.maxDeckLength) return;
        if (index >= DeckManager.instance.deck.Count) battleDeckRingImages[index].sprite = GameManager.instance.emptyRingSprite;
        else battleDeckRingImages[index].sprite = GameManager.instance.ringSprites[DeckManager.instance.deck[index]];

    }
}
