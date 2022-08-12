using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public static FloorManager instance;

    public bool debugFlag;
    public int debugInt;

    //�׷��� ���� ����
    bool isPortalOn;
    public Animator[] portals;
    public SpriteRenderer roomImage;
    float portalScale;
    Item lastTouchedItem;

    //�� ���� ����
    public Floor floor;
    public Room curRoom;
    public int playerX, playerY;
    public Item lastTouchItem;

    //�����¿� üũ��
    int[] dx = { 0, 0, -1, 1 };
    int[] dy = { -1, 1, 0, 0 };

    void Awake()
    {
        instance = this;
        floor = new Floor();
        portalScale = 0;
    }

    void Update()
    {
        if (isPortalOn)
        {
            if (portalScale < 1)
            {
                if (portalScale < 0.1f) portalScale += 0.003f;
                else if (portalScale < 0.3f) portalScale += 0.015f;
                else portalScale += 0.06f;
                portalScale = Mathf.Clamp01(portalScale);
                for (int i = 0; i < 5; i++) portals[i].gameObject.transform.localScale = new Vector3(portalScale, portalScale, 1);
            }
            if (!SceneChanger.instance.image.raycastTarget 
                && !UIManager.instance.relicInfoPanel.activeSelf 
                && !UIManager.instance.ringInfoPanel.activeSelf 
                && !UIManager.instance.playerStatusPanel.activeSelf 
                && !UIManager.instance.ringSelectionPanel.activeSelf) GetInput();
        }
    }

    //�ش� ���� ���� �����ϰ� �̵��Ѵ�.
    public void CreateAndMoveToFloor(int f)
    {
        floor.Generate(f); 
        UIManager.instance.InitializeMap();

        for (int i = 1; i <= 9; i++)
            for (int j = 1; j <= 9; j++)
                floor.rooms[i, j].visited = false;

        SceneChanger.instance.ChangeScene(MoveToRoom, floor.startX, floor.startY);
    }

    //�ش� ������ �̵��Ѵ�.
    public void MoveToRoom(int x, int y)
    {
        UIManager.instance.lobbyPanel.SetActive(false);

        if (curRoom != null) HideItems();

        //���� ��ġ/�� ���� ������ �ٲ۴�.
        playerX = x;
        playerY = y;
        curRoom = floor.rooms[playerX, playerY];

        switch (curRoom.type)
        {
            case 0:
                TurnPortalsOnOff(true);
                break;
            case 1:
                roomImage.transform.position = new Vector3(GameManager.instance.monsterPaths[curRoom.pathID].transform.position.x, GameManager.instance.monsterPaths[curRoom.pathID].transform.position.y - 2.5f, roomImage.transform.position.z);
                if (!curRoom.visited) BattleManager.instance.StartBattle();
                TurnPortalsOnOff(curRoom.visited);
                break;
            case 2:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(0, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 3:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    int ringID;
                    do ringID = Random.Range(0, GameManager.instance.ringDB.Count);
                    while (DeckManager.instance.deck.Contains(ringID));
                    item.InitializeItem(1000 + ringID, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 4:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    int relicID;
                    do relicID = Random.Range(0, GameManager.instance.relicDB.Count);
                    while (GameManager.instance.relics.Contains(relicID));
                    item.InitializeItem(2000 + relicID, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 5:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(1, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 6:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(3, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 7:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(2, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 8:
                TurnPortalsOnOff(true);
                break;
            case 9:
                roomImage.transform.position = new Vector3(GameManager.instance.monsterPaths[curRoom.pathID].transform.position.x, GameManager.instance.monsterPaths[curRoom.pathID].transform.position.y - 2.5f, roomImage.transform.position.z);
                if (!curRoom.visited) BattleManager.instance.StartBattle();
                TurnPortalsOnOff(curRoom.visited);
                break;
        }
        curRoom.visited = true;

        //������ ��ȭ�� �ش�.
        UIManager.instance.RevealMapArea(playerX, playerY);

        //��������Ʈ�� �ùٸ� ������� �ٲ۴�.
        roomImage.sprite = GameManager.instance.sceneRoomSprites[floor.floorNum];

        //ī�޶� �ش��ϴ� �������� �̵��Ѵ�.
        Camera.main.transform.position = roomImage.transform.position;
        Camera.main.transform.Translate(0, 1.5f, -15);
    } 

    public void ChangeCurRoomToIdle()
    {
        curRoom.type = 0;
    }

    private void TurnPortalsOnOff(bool isOn)
    {
        isPortalOn = isOn;
        if (isOn)
        {
            portalScale = 0.0f;
            Room adjRoom;
            for (int i = 0; i < 4; i++)
            {
                adjRoom = floor.rooms[playerX + dx[i], playerY + dy[i]];
                if (adjRoom.type != -1 && adjRoom.type != 10)
                {
                    portals[i].gameObject.transform.localScale = Vector3.forward;
                    portals[i].gameObject.SetActive(true);
                    if (adjRoom.type == 0 || ((adjRoom.type == 1 || adjRoom.type == 9) && adjRoom.visited)) portals[i].SetFloat("portalColor", 0);
                    else if (adjRoom.type == 1) portals[i].SetFloat("portalColor", 0.2f);
                    else if (adjRoom.type == 9) portals[i].SetFloat("portalColor", 0.6f);
                    else if (adjRoom.type != 8 && adjRoom.visited && adjRoom.items.Count == 0) portals[i].SetFloat("portalColor", 0);
                    else portals[i].SetFloat("portalColor", 0.4f);
                }
                else
                {
                    portals[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++) portals[i].gameObject.SetActive(false);
        }
        if (curRoom.visited && curRoom.type == 9)
        {
            portals[4].gameObject.transform.localScale = Vector3.zero;
            portals[4].gameObject.SetActive(true);
            portals[4].SetFloat("portalColor", 0.8f);
        }
        else portals[4].gameObject.SetActive(false);
    }

    void GetInput()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 touchPos;

            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;
            touchPos = Camera.main.ScreenToWorldPoint(touchPos);

            RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero, 0f);

            if (hit.collider != null)
            {
                if (hit.collider.tag != "Portal" || Time.timeScale == 0) return;
                int dir = hit.collider.name[hit.collider.name.Length - 1] - '0'; 
                if (hit.collider.gameObject != portals[4].gameObject) SceneChanger.instance.ChangeScene(MoveToRoom, playerX + dx[dir], playerY + dy[dir]);
                else CreateAndMoveToFloor(floor.floorNum + 1);
            }
        }
    }

    public void RemoveItem(Item item, bool isImme)
    {
        lastTouchedItem = item;
        curRoom.items.Remove(item);
        UIManager.instance.RevealMapArea(playerX, playerY);
        if (isImme) GameManager.instance.ReturnItemToPool(item);
        else
        {
            item.animator.SetTrigger("isTake");
            Invoke("InvokeReturnItemToPool", 1.51f);
        }
    }

    void InvokeReturnItemToPool()
    {
        GameManager.instance.ReturnItemToPool(lastTouchedItem);
    }

    public void ShowItems()
    {
        for (int i = 0; i < curRoom.items.Count; i++)
        {
            curRoom.items[i].transform.position = Camera.main.transform.position + curRoom.items[i].pos;
            curRoom.items[i].gameObject.SetActive(true);
        }
    }

    public void HideItems()
    {
        for (int i = 0; i < curRoom.items.Count; i++) curRoom.items[i].gameObject.SetActive(false);
    }
}
