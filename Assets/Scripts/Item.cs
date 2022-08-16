using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer highlightRenderer;
    public GameObject shadow;
    public SpriteRenderer costTypeImage;
    public TextMesh costText;
    public Animator animator;

    public int itemType;
    public Vector3 pos;

    public bool debugFlag;
    public int debugType;
    public int debugCostType;
    public int debugCost;

    int costType;
    int cost;
    public void InitializeItem(int _type, Vector3 _pos, int _costType, int _cost)
    {
        pos = _pos;
        itemType = _type;
        costType = _costType;
        cost = _cost;

        if (itemType < 1000) spriteRenderer.sprite = GameManager.instance.itemSprites[itemType];
        else if (itemType < 2000) spriteRenderer.sprite = GameManager.instance.ringSprites[itemType - 1000];
        else if (itemType < 3000) spriteRenderer.sprite = GameManager.instance.relicSprites[itemType - 2000];

        if (costType == 0)
        {
            costTypeImage.gameObject.SetActive(false);
            costText.gameObject.SetActive(false);
        }
        else
        {
            costTypeImage.sprite = GameManager.instance.itemSprites[costType + 3];
            costText.text = cost.ToString();
            costTypeImage.gameObject.SetActive(true);
            costText.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (debugFlag)
        {
            debugFlag = false;
            InitializeItem(debugType, new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 1), debugCostType, debugCost);
        }
        if (Input.GetMouseButtonUp(0) && !UIManager.instance.playerStatusPanel.activeSelf && !UIManager.instance.relicInfoPanel.activeSelf && !UIManager.instance.ringInfoPanel.activeSelf && !UIManager.instance.ringSelectionPanel.activeSelf && FloorManager.instance.curRoom.items.Contains(this))
        {
            Vector2 touchPos;
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.gameObject == gameObject) GiveThisToPlayer();
        }
    }

    //재화가 부족하거나 사용 불가라면 false 반환
    bool GiveThisToPlayer()
    {
        if (costType == 1)
        {
            if (GameManager.instance.gold < cost) return false;
        }
        else if (costType == 2)
        {
            if (GameManager.instance.diamond < cost) return false;
        }

        if (itemType < 1000)
        {
            FloorManager.instance.lastTouchItem = this;
            switch (itemType)
            {
                case 0:
                    UIManager.instance.OpenRingSelectionPanel(1);
                    break;
                case 1:
                    UIManager.instance.OpenRingSelectionPanel(0);
                    break;
                case 2:
                    if (GameManager.instance.ChangePlayerCurHP((int)(GameManager.instance.playerMaxHP * Random.Range(0.15f, 0.3f))))
                        FloorManager.instance.RemoveItem(this, false);
                    else return false;
                    break;
                case 3:
                    GameManager.instance.ChangeGold(100);
                    FloorManager.instance.RemoveItem(this, false);
                    break;
                case 4:
                    GameManager.instance.ChangeGold(100);
                    FloorManager.instance.RemoveItem(this, false);
                    break;
                case 5:
                    GameManager.instance.ChangeDiamond(1);
                    FloorManager.instance.RemoveItem(this, false);
                    break;
                case 6:
                    if (GameManager.instance.cursedRelics.Count != 0)
                    {
                        int targetIdx = Random.Range(0, GameManager.instance.cursedRelics.Count);
                        GameManager.instance.relicDB[GameManager.instance.cursedRelics[targetIdx]].isPure = true;
                        GameManager.instance.cursedRelics.RemoveAt(targetIdx);
                        FloorManager.instance.RemoveItem(this, false);
                    }
                    else return false;
                    break;
            }
        }
        else if (itemType < 2000)
        {
            UIManager.instance.OpenRingInfoPanel(itemType - 1000);
        }
        else if (itemType < 3000)
        {
            UIManager.instance.OpenRelicInfoPanel(itemType - 2000);
        }

        return true;
    }

    public void Pay()
    {
        if (costType == 1) GameManager.instance.ChangeGold(-cost);
        else if (costType == 2) GameManager.instance.ChangeDiamond(-cost);
    }

    void InvokeReturnItemToPool()
    {
        GameManager.instance.ReturnItemToPool(this);
    }
}
