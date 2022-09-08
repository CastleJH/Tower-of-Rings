using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public static FloorManager instance;

    public bool debugFlag;
    public int debugInt;

    //그래픽 관련 변수
    bool isPortalOn;
    public Animator[] portals;
    public GameObject endPortal;
    public SpriteRenderer roomImage;
    float portalScale;

    //층 관련 변수
    public Floor floor;
    public Room curRoom;
    public int playerX, playerY;
    public Item lastTouchItem;

    //상하좌우 체크용
    int[] dx = { 0, 0, -1, 1 };
    int[] dy = { -1, 1, 0, 0 };

    //아이템 위치
    public Vector3[] itemPos =
    {
        new Vector3(-2, 1, 1), new Vector3(2, -1, 1),
        new Vector3(-2, 5, 1), new Vector3(2, 3, 1),
        new Vector3(-2, -3, 1), new Vector3(2, -5, 1)
    };

    void Awake()
    {
        instance = this;
        floor = new Floor();
        portalScale = 0;
    }

    void Update()
    {
        if (debugFlag)
        {
            floor.Generate(debugInt);
            MoveToRoom(floor.startX, floor.startY);
            for (int i = 1; i <= 9; i++)
                for (int j = 1; j <= 9; j++)
                {
                    if (floor.rooms[i, j].type != -1 && floor.rooms[i, j].type != 10)
                    {
                        //Debug.Log(GameManager.instance.mapRoomSprites[floor.rooms[i, j].type]);
                        //Debug.Log(UIManager.instance.maps[i][j]);
                        UIManager.instance.maps[i][j].sprite = GameManager.instance.mapRoomSprites[floor.rooms[i, j].type];
                        UIManager.instance.maps[i][j].color = Color.white;
                    }
                    else UIManager.instance.maps[i][j].color = new Color(0, 0, 0, 0);
                }
            debugFlag = false;
        }
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

    //해당 층을 새로 생성하고 이동한다.
    public void CreateAndMoveToFloor(int f)
    {
        floor.Generate(f); 
        UIManager.instance.InitializeMap();

        for (int i = 1; i <= 9; i++)
            for (int j = 1; j <= 9; j++)
                floor.rooms[i, j].visited = false;

        SceneChanger.instance.ChangeScene(MoveToRoom, floor.startX, floor.startY);
        if (GameManager.instance.baseRelics[0].have)
        {
            if (GameManager.instance.baseRelics[0].isPure) GameManager.instance.ChangePlayerCurHP(10); 
            else GameManager.instance.ChangePlayerCurHP(-10);
        }
        if (f == 1)
        {
            if (GameManager.instance.spiritEnhanceLevel[9] == 1)
            {
                Item item = GameManager.instance.GetItemFromPool();
                int ringID;
                do ringID = Random.Range(0, GameManager.instance.baseRings.Count);
                while (DeckManager.instance.deck.Contains(ringID));
                if (GameManager.instance.spiritEnhanceLevel[10] == 0) item.InitializeItem(1000 + ringID, Vector3.forward, 0, 0);
                else item.InitializeItem(1000 + ringID, itemPos[0], 0, 0);
                floor.rooms[floor.startX, floor.startY].AddItem(item);
            }
            if (GameManager.instance.spiritEnhanceLevel[10] == 1)
            {
                Item item = GameManager.instance.GetItemFromPool();
                int relicID;
                do relicID = Random.Range(0, GameManager.instance.baseRelics.Count);
                while (GameManager.instance.relics.Contains(relicID));
                if (GameManager.instance.spiritEnhanceLevel[9] == 0) item.InitializeItem(2000 + relicID, Vector3.forward, 0, 0);
                else item.InitializeItem(2000 + relicID, itemPos[1], 0, 0);
                floor.rooms[floor.startX, floor.startY].AddItem(item);
            }
        }
    }

    //해당 방으로 이동한다.
    public void MoveToRoom(int x, int y)
    {
        UIManager.instance.lobbyPanel.SetActive(false);

        if (curRoom != null) HideItems();

        //현재 위치/방 관련 변수를 바꾼다.
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
                if (!curRoom.visited)
                {
                    curRoom.AddSinkhole();
                    BattleManager.instance.StartBattle();
                }
                TurnPortalsOnOff(curRoom.visited);
                break;
            case 2:
                if (!curRoom.visited)
                {
                    if (GameManager.instance.baseRelics[6].have && GameManager.instance.baseRelics[6].isPure && Random.Range(0.0f, 1.0f) <= 0.33f)
                    {
                        Item item = GameManager.instance.GetItemFromPool();
                        item.InitializeItem(0, itemPos[0], 0, 0);
                        curRoom.AddItem(item);
                        item = GameManager.instance.GetItemFromPool();
                        item.InitializeItem(0, itemPos[1], 0, 0);
                        curRoom.AddItem(item);
                    }
                    else
                    {
                        Item item = GameManager.instance.GetItemFromPool();
                        item.InitializeItem(0, Vector3.forward, 0, 0);
                        curRoom.AddItem(item);
                    }
                }
                //ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 3:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    int ringID;
                    do ringID = Random.Range(0, GameManager.instance.baseRings.Count);
                    while (DeckManager.instance.deck.Contains(ringID));
                    item.InitializeItem(1000 + ringID, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                //ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 4:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    int relicID;
                    do relicID = Random.Range(0, GameManager.instance.baseRelics.Count);
                    while (GameManager.instance.relics.Contains(relicID));
                    item.InitializeItem(2000 + relicID, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                //ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 5:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(1, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                //ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 6:
                if (!curRoom.visited)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        if (GameManager.instance.baseRelics[7].have && !GameManager.instance.baseRelics[7].isPure)
                        {
                            if (Random.Range(0, 2) == 1) continue;
                            Item item = GameManager.instance.GetItemFromPool();
                            item.InitializeItem(5, itemPos[i], 0, 0);
                            curRoom.AddItem(item);
                        }
                        else
                        {
                            Item item = GameManager.instance.GetItemFromPool();
                            if (Random.Range(0, 2) == 1) item.InitializeItem(3, itemPos[i], 0, 0);
                            else item.InitializeItem(5, itemPos[i], 0, 0);
                            curRoom.AddItem(item);
                        }
                    }
                }
                //ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 7:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(2, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                //ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 8:
                if (!curRoom.visited)
                {
                    float price = 1.0f;
                    if (GameManager.instance.baseRelics[11].have)
                    {
                        if (GameManager.instance.baseRelics[11].isPure) price = 0.5f;
                        else price = 1.3f;
                    }
                    if (!GameManager.instance.isNormalMode) price *= 2.0f;
                    Item item;
                    int itemID;

                    //유물
                    item = GameManager.instance.GetItemFromPool();
                    do itemID = Random.Range(0, GameManager.instance.baseRelics.Count);
                    while (GameManager.instance.relics.Contains(itemID));
                    item.InitializeItem(2000 + itemID, itemPos[2], 1, (int)(10 * price));
                    curRoom.AddItem(item);

                    //유물 정화
                    item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(6, itemPos[3], 1, (int)(10 * price));
                    curRoom.AddItem(item);

                    //링
                    item = GameManager.instance.GetItemFromPool();
                    do itemID = Random.Range(0, GameManager.instance.baseRings.Count);
                    while (DeckManager.instance.deck.Contains(itemID));
                    item.InitializeItem(1000 + itemID, itemPos[0], 1, (int)(5 * price));
                    curRoom.AddItem(item);

                    //회복
                    item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(2, itemPos[1], 1, (int)(5 * price));
                    curRoom.AddItem(item);

                    //제련 망치
                    item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(0, itemPos[4], 1, (int)(5 * price));
                    curRoom.AddItem(item);

                    //파괴 망치
                    if (!GameManager.instance.baseRelics[13].have || GameManager.instance.baseRelics[13].isPure)
                    {
                        item = GameManager.instance.GetItemFromPool();
                        int itemPrice = (int)(5 * price);
                        if (GameManager.instance.baseRelics[13].have) itemPrice = 0;
                        item.InitializeItem(1, itemPos[5], 1, itemPrice);
                        curRoom.AddItem(item);
                    }
                }
                //ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 9:
                roomImage.transform.position = new Vector3(GameManager.instance.monsterPaths[curRoom.pathID].transform.position.x, GameManager.instance.monsterPaths[curRoom.pathID].transform.position.y - 2.5f, roomImage.transform.position.z);
                if (!curRoom.visited)
                {
                    curRoom.AddSinkhole();
                    BattleManager.instance.StartBattle();
                }
                //else ShowItems();
                TurnPortalsOnOff(curRoom.visited);
                break;
        }
        ShowItems();
        curRoom.visited = true;

        //지도에 변화를 준다.
        UIManager.instance.RevealMapAndMoveMarker(playerX, playerY);

        //스프라이트를 올바른 모양으로 바꾼다.
        roomImage.sprite = GameManager.instance.sceneRoomSprites[floor.floorNum];

        curRoom.ShowSinkhole();

        //카메라를 해당하는 전장으로 이동한다.
        Camera.main.transform.position = roomImage.transform.position + new Vector3(0.0f, 1.5f, -15f);
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
            if (floor.floorNum < 7)
            {
                portals[4].gameObject.transform.localScale = Vector3.zero;
                portals[4].gameObject.SetActive(true);
                portals[4].SetFloat("portalColor", 0.8f);
            }
            else
            {
                for (int i = 0; i < 4; i++) portals[i].gameObject.SetActive(false);
                endPortal.SetActive(true);
            }
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
                else
                {
                    if (floor.floorNum < 7) CreateAndMoveToFloor(floor.floorNum + 1);
                    else SceneChanger.instance.ChangeScene(GameManager.instance.OnGameClear, 0, 0);
                }
            }
        }
    }

    public void RemoveItem(Item item, bool isImme)
    {
        item.Pay();
        curRoom.items.Remove(item);
        UIManager.instance.RevealMapAndMoveMarker(playerX, playerY);

        if (item.itemType >= 2000)
        {
            if (GameManager.instance.baseRelics[item.itemType - 2000].isPure) item.highlightRenderer.color = Color.white;
            else item.highlightRenderer.color = Color.black;
        }
        else item.highlightRenderer.color = Color.white;

        if (isImme) GameManager.instance.ReturnItemToPool(item);
        else
        {
            item.animator.SetTrigger("isTake");
            item.Invoke("InvokeReturnItemToPool", 1.51f);
        }
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
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Item");
        for (int i = 0; i < objs.Length; i++) objs[i].SetActive(false);
    }
}
