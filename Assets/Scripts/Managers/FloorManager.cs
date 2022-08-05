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
    public SpriteRenderer roomImage;

    //층 관련 변수
    public Floor floor;
    public Room curRoom;
    public int playerX, playerY;

    //상하좌우 체크용
    int[] dx = { 0, 0, -1, 1 };
    int[] dy = { -1, 1, 0, 0 };

    void Awake()
    {
        instance = this;
        floor = new Floor();
    }

    void Update()
    {
        if (isPortalOn && !SceneChanger.instance.image.raycastTarget) GetInput();
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
    }

    //해당 방으로 이동한다.
    public void MoveToRoom(int x, int y)
    {
        UIManager.instance.gameStartPanel.SetActive(false);

        if (curRoom != null) curRoom.HideItems();

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
                curRoom.ShowItems();
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
                curRoom.ShowItems();
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
                curRoom.ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 5:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(1, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                curRoom.ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 6:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(3, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                curRoom.ShowItems();
                TurnPortalsOnOff(true);
                break;
            case 7:
                if (!curRoom.visited)
                {
                    Item item = GameManager.instance.GetItemFromPool();
                    item.InitializeItem(2, Vector3.forward, 0, 0);
                    curRoom.AddItem(item);
                }
                curRoom.ShowItems();
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

        //지도에 변화를 준다.
        UIManager.instance.RevealMapArea(playerX, playerY);

        //스프라이트를 올바른 모양으로 바꾼다.
        roomImage.sprite = GameManager.instance.sceneRoomSprites[floor.floorNum];

        //카메라를 해당하는 전장으로 이동한다.
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
            Room adjRoom;
            for (int i = 0; i < 4; i++)
            {
                adjRoom = floor.rooms[playerX + dx[i], playerY + dy[i]];
                if (adjRoom.type != -1 && adjRoom.type != 10)
                {
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
            portals[4].gameObject.SetActive(true);
            portals[4].SetFloat("portalColor", 0.8f);
        }
        else portals[4].gameObject.SetActive(false);
    }

    public void GetInput()
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
}
