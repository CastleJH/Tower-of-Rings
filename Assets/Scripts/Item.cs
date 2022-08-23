using UnityEngine;

public class Item : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;       //������ ��������Ʈ
    public SpriteRenderer highlightRenderer;    //������ ȹ�� ���̶���Ʈ
    public SpriteRenderer costTypeImage;        //��� �̹���
    public TextMesh costText;                   //��� �ؽ�Ʈ
    public Animator animator;

    public int itemType;    //������ Ÿ��
    public Vector3 pos;     //������ ��ġ ��ġ

    int costType;   //ȹ�� �� �Ҹ��� ��ȭ Ÿ��
    int cost;       //ȹ�� �� �Ҹ��� ��ȭ��

    void Update()
    {
        //��ġ ������ �ڽ��� �ִٸ� �������� �ش�.
        if (Input.GetMouseButtonUp(0) && !UIManager.instance.playerStatusPanel.activeSelf && !UIManager.instance.relicInfoPanel.activeSelf && !UIManager.instance.ringInfoPanel.activeSelf && !UIManager.instance.ringSelectionPanel.activeSelf && FloorManager.instance.curRoom.items.Contains(this))
        {
            Vector2 touchPos;
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.gameObject == gameObject) GiveThisToPlayer();
        }
    }

    //�������� �ʱ�ȭ�Ѵ�.
    public void InitializeItem(int _type, Vector3 _pos, int _costType, int _cost)
    {
        pos = _pos;
        itemType = _type;
        costType = _costType;
        cost = _cost;

        if (itemType < 1000) spriteRenderer.sprite = GameManager.instance.itemSprites[itemType];                //�Ϲ� ������
        else if (itemType < 2000) spriteRenderer.sprite = GameManager.instance.ringSprites[itemType - 1000];    //��
        else if (itemType < 3000) spriteRenderer.sprite = GameManager.instance.relicSprites[itemType - 2000];   //����

        if (costType == 0)  //ȹ�� �� �Ҹ��� ��ȭ�� ���� ������ ��
        {
            costTypeImage.gameObject.SetActive(false);
            costText.gameObject.SetActive(false);
        }
        else  //���/���̾Ƹ�带 �Ҹ��ϵ��� ��
        {
            costTypeImage.sprite = GameManager.instance.itemSprites[costType + 3];
            costText.text = cost.ToString();
            costTypeImage.gameObject.SetActive(true);
            costText.gameObject.SetActive(true);
        }
    }

    //�������� �÷��̾�� �ش�. ���� ��ȭ�� �����ϰų� ��� �Ұ���� �ƹ��� ȿ���� �������� �ʴ´�(�ٸ� ���� ������ �ʿ� ��ȭ�� ��� ���� ���� �г��� �������� �Ѵ�).
    void GiveThisToPlayer()
    {
        if (itemType < 1000)    //�Ϲ� �������� ��� ��ȭ�� �����ϸ� false�� ��ȯ�ϰ�, �ƴϸ� ����Ѵ�.
        {
            if (costType == 1 && GameManager.instance.gold < cost) return;            //��� ������ ���, ��ȭ�� �����ϴٸ� false ��ȯ.
            else if (costType == 2 && GameManager.instance.diamond < cost) return;    //���̾� ������ ���, ��ȭ�� �����ϴٸ� false ��ȯ.

            FloorManager.instance.lastTouchItem = this;
            switch (itemType)
            {
                case 0:     //���� ��ġ
                    UIManager.instance.OpenRingSelectionPanel(1);
                    break;
                case 1:     //�ı� ��ġ
                    UIManager.instance.OpenRingSelectionPanel(0);
                    break;
                case 2:     //HP ȸ��
                    float healAmount = GameManager.instance.playerMaxHP * Random.Range(0.15f, 0.3f);
                    if (GameManager.instance.baseRelics[9].have)    //���� ���� ���ο� ���� ȸ������ �����Ѵ�.
                    {
                        if (GameManager.instance.baseRelics[9].isPure) healAmount *= 2;
                        else healAmount *= 0.5f;
                    }
                    if (GameManager.instance.ChangePlayerCurHP((int)healAmount)) FloorManager.instance.RemoveItem(this, false); //Ǯ�ǰ� �ƴ� ��츸 ȸ�� & ������ �����Ѵ�.
                    else return;      //Ǯ�ǿ����� ��� �Ұ��� false ��ȯ�Ѵ�.
                    break;
                case 3:     //5��� ȹ��
                    if (GameManager.instance.baseRelics[7].have && GameManager.instance.baseRelics[7].isPure) GameManager.instance.ChangeGold(6);   //���� ���� ���ο� ���� ȹ�淮�� �����Ѵ�.
                    else GameManager.instance.ChangeGold(5);
                    FloorManager.instance.RemoveItem(this, false);
                    break;
                case 4:     //1��� ȹ��
                    if (GameManager.instance.baseRelics[7].have && GameManager.instance.baseRelics[7].isPure) GameManager.instance.ChangeGold(2);   //���� ���� ���ο� ���� ȹ�淮�� �����Ѵ�.
                    else GameManager.instance.ChangeGold(1);
                    FloorManager.instance.RemoveItem(this, false);
                    break;
                case 5:     //1���̾Ƹ�� ȹ��
                    GameManager.instance.ChangeDiamond(1);
                    FloorManager.instance.RemoveItem(this, false);
                    break;
                case 6:     //���� ��ȭ
                    if (GameManager.instance.cursedRelics.Count != 0)   //��ȭ�� ������ �ִ� ��� �����ϰ� �ϳ��� ���� ��ȭ�Ѵ�.
                    {
                        int targetIdx = Random.Range(0, GameManager.instance.cursedRelics.Count);
                        GameManager.instance.AddRelicToPlayer(GameManager.instance.cursedRelics[targetIdx], true);    //��ȭ�ؼ� �ٽ� ���� �ִ´�.
                        GameManager.instance.cursedRelics.RemoveAt(targetIdx);
                        FloorManager.instance.RemoveItem(this, false);
                    }
                    else return;      //��ȭ�� ������ ������ ��� �Ұ��� false ��ȯ�Ѵ�.
                    break;
            }
        }
        else if (itemType < 2000)        //���� ��� ���� �г��� ����.
        {
            if (costType == 1 && GameManager.instance.gold < cost) UIManager.instance.ringInfoTakeText.text = "��尡 �����ϴ�";
            else if (costType == 2 && GameManager.instance.diamond < cost) UIManager.instance.ringInfoTakeText.text = "���̾Ƹ�尡 �����ϴ�";
            else UIManager.instance.ringInfoTakeText.text = "�� ���� ��������";
            UIManager.instance.OpenRingInfoPanel(itemType - 1000);
        }
        else if (itemType < 3000)   //������ ��� ���� �г��� ����.
        {
            if (costType == 1 && GameManager.instance.gold < cost) UIManager.instance.relicInfoTakeText.text = "��尡 �����ϴ�";
            else if (costType == 2 && GameManager.instance.diamond < cost) UIManager.instance.relicInfoTakeText.text = "���̾Ƹ�尡 �����ϴ�";
            else UIManager.instance.relicInfoTakeText.text = "�� ������ ��������";
            UIManager.instance.OpenRelicInfoPanel(itemType - 2000);
        }
    }

    //�������� ���� �����Ѵ�.
    public void Pay()
    {
        if (costType == 1) GameManager.instance.ChangeGold(-cost);
        else if (costType == 2) GameManager.instance.ChangeDiamond(-cost);
    }

    //FloorManager���� ������ ȹ�� �� �ִϸ��̼� �÷��� �Ϸ� �� ������Ʈ Ǯ�� �� �������� �ǵ��� ��(Invoke) �Ҹ���.
    void InvokeReturnItemToPool()
    {
        GameManager.instance.ReturnItemToPool(this);
    }
}
