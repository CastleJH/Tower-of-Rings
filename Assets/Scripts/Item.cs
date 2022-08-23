using UnityEngine;

public class Item : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;       //아이템 스프라이트
    public SpriteRenderer highlightRenderer;    //아이템 획득 하이라이트
    public SpriteRenderer costTypeImage;        //비용 이미지
    public TextMesh costText;                   //비용 텍스트
    public Animator animator;

    public int itemType;    //아이템 타입
    public Vector3 pos;     //아이템 배치 위치

    int costType;   //획득 시 소모할 재화 타입
    int cost;       //획득 시 소모할 재화량

    void Update()
    {
        //터치 지점에 자신이 있다면 아이템을 준다.
        if (Input.GetMouseButtonUp(0) && !UIManager.instance.playerStatusPanel.activeSelf && !UIManager.instance.relicInfoPanel.activeSelf && !UIManager.instance.ringInfoPanel.activeSelf && !UIManager.instance.ringSelectionPanel.activeSelf && FloorManager.instance.curRoom.items.Contains(this))
        {
            Vector2 touchPos;
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.gameObject == gameObject) GiveThisToPlayer();
        }
    }

    //아이템을 초기화한다.
    public void InitializeItem(int _type, Vector3 _pos, int _costType, int _cost)
    {
        pos = _pos;
        itemType = _type;
        costType = _costType;
        cost = _cost;

        if (itemType < 1000) spriteRenderer.sprite = GameManager.instance.itemSprites[itemType];                //일반 아이템
        else if (itemType < 2000) spriteRenderer.sprite = GameManager.instance.ringSprites[itemType - 1000];    //링
        else if (itemType < 3000) spriteRenderer.sprite = GameManager.instance.relicSprites[itemType - 2000];   //유물

        if (costType == 0)  //획득 시 소모할 재화가 따로 없도록 함
        {
            costTypeImage.gameObject.SetActive(false);
            costText.gameObject.SetActive(false);
        }
        else  //골드/다이아몬드를 소모하도록 함
        {
            costTypeImage.sprite = GameManager.instance.itemSprites[costType + 3];
            costText.text = cost.ToString();
            costTypeImage.gameObject.SetActive(true);
            costText.gameObject.SetActive(true);
        }
    }

    //아이템을 플레이어에게 준다. 만일 재화가 부족하거나 사용 불가라면 아무런 효과를 적용하지 않는다(다만 링과 유물은 필요 재화에 상관 없이 정보 패널이 열리도록 한다).
    void GiveThisToPlayer()
    {
        if (itemType < 1000)    //일반 아이템인 경우 재화가 부족하면 false를 반환하고, 아니면 사용한다.
        {
            if (costType == 1 && GameManager.instance.gold < cost) return;            //골드 지불의 경우, 재화가 부족하다면 false 반환.
            else if (costType == 2 && GameManager.instance.diamond < cost) return;    //다이아 지불의 경우, 재화가 부족하다면 false 반환.

            FloorManager.instance.lastTouchItem = this;
            switch (itemType)
            {
                case 0:     //제련 망치
                    UIManager.instance.OpenRingSelectionPanel(1);
                    break;
                case 1:     //파괴 망치
                    UIManager.instance.OpenRingSelectionPanel(0);
                    break;
                case 2:     //HP 회복
                    float healAmount = GameManager.instance.playerMaxHP * Random.Range(0.15f, 0.3f);
                    if (GameManager.instance.baseRelics[9].have)    //유물 보유 여부에 따라 회복량을 조정한다.
                    {
                        if (GameManager.instance.baseRelics[9].isPure) healAmount *= 2;
                        else healAmount *= 0.5f;
                    }
                    if (GameManager.instance.ChangePlayerCurHP((int)healAmount)) FloorManager.instance.RemoveItem(this, false); //풀피가 아닌 경우만 회복 & 아이템 제거한다.
                    else return;      //풀피였으면 사용 불가로 false 반환한다.
                    break;
                case 3:     //5골드 획득
                    if (GameManager.instance.baseRelics[7].have && GameManager.instance.baseRelics[7].isPure) GameManager.instance.ChangeGold(6);   //유물 보유 여부에 따라 획득량을 조정한다.
                    else GameManager.instance.ChangeGold(5);
                    FloorManager.instance.RemoveItem(this, false);
                    break;
                case 4:     //1골드 획득
                    if (GameManager.instance.baseRelics[7].have && GameManager.instance.baseRelics[7].isPure) GameManager.instance.ChangeGold(2);   //유물 보유 여부에 따라 획득량을 조정한다.
                    else GameManager.instance.ChangeGold(1);
                    FloorManager.instance.RemoveItem(this, false);
                    break;
                case 5:     //1다이아몬드 획득
                    GameManager.instance.ChangeDiamond(1);
                    FloorManager.instance.RemoveItem(this, false);
                    break;
                case 6:     //유물 정화
                    if (GameManager.instance.cursedRelics.Count != 0)   //정화할 유물이 있는 경우 랜덤하게 하나를 택해 정화한다.
                    {
                        int targetIdx = Random.Range(0, GameManager.instance.cursedRelics.Count);
                        GameManager.instance.AddRelicToPlayer(GameManager.instance.cursedRelics[targetIdx], true);    //정화해서 다시 덱에 넣는다.
                        GameManager.instance.cursedRelics.RemoveAt(targetIdx);
                        FloorManager.instance.RemoveItem(this, false);
                    }
                    else return;      //정화할 유물이 없으면 사용 불가로 false 반환한다.
                    break;
            }
        }
        else if (itemType < 2000)        //링인 경우 정보 패널을 연다.
        {
            if (costType == 1 && GameManager.instance.gold < cost) UIManager.instance.ringInfoTakeText.text = "골드가 부족하다";
            else if (costType == 2 && GameManager.instance.diamond < cost) UIManager.instance.ringInfoTakeText.text = "다이아몬드가 부족하다";
            else UIManager.instance.ringInfoTakeText.text = "이 링을 가져간다";
            UIManager.instance.OpenRingInfoPanel(itemType - 1000);
        }
        else if (itemType < 3000)   //유물인 경우 정보 패널을 연다.
        {
            if (costType == 1 && GameManager.instance.gold < cost) UIManager.instance.relicInfoTakeText.text = "골드가 부족하다";
            else if (costType == 2 && GameManager.instance.diamond < cost) UIManager.instance.relicInfoTakeText.text = "다이아몬드가 부족하다";
            else UIManager.instance.relicInfoTakeText.text = "이 유물을 가져간다";
            UIManager.instance.OpenRelicInfoPanel(itemType - 2000);
        }
    }

    //아이템의 값을 지불한다.
    public void Pay()
    {
        if (costType == 1) GameManager.instance.ChangeGold(-cost);
        else if (costType == 2) GameManager.instance.ChangeDiamond(-cost);
    }

    //FloorManager에서 아이템 획득 시 애니메이션 플레이 완료 후 오브젝트 풀에 이 아이템을 되돌릴 때(Invoke) 불린다.
    void InvokeReturnItemToPool()
    {
        GameManager.instance.ReturnItemToPool(this);
    }
}
