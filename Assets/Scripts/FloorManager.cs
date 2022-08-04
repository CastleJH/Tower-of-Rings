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
        if (debugFlag)
        {
            debugFlag = false;
            CreateAndMoveToFloor(debugInt);
        }
        if (isPortalOn) GetInput();
    }

    //해당 층을 새로 생성하고 이동한다.
    void CreateAndMoveToFloor(int f)
    {
        floor.Generate(f);
        UIManager.instance.InitializeMap();

        for (int i = 1; i <= 9; i++)
            for (int j = 1; j <= 9; j++)
                floor.rooms[i, j].visited = false;

        MoveToRoom(floor.startX, floor.startY);
        TurnPortalsOnOff(true);
    }

    //해당 방으로 이동한다.
    void MoveToRoom(int x, int y)
    {
        //현재 위치/방 관련 변수를 바꾼다.
        playerX = x;
        playerY = y;
        curRoom = floor.rooms[playerX, playerY];
        curRoom.visited = true;

        //지도에 변화를 준다.
        UIManager.instance.RevealMapArea(playerX, playerY);

        //스프라이트를 올바른 모양으로 바꾼다.
        roomImage.sprite = GameManager.instance.sceneRoomSprites[floor.floorNum];

        switch (curRoom.type)
        {
            case 0:
                TurnPortalsOnOff(true);
                break;
            case 1:
                TurnPortalsOnOff(false);
                roomImage.transform.position = new Vector3(GameManager.instance.monsterPaths[curRoom.pathID].transform.position.x, GameManager.instance.monsterPaths[curRoom.pathID].transform.position.y - 2.5f, roomImage.transform.position.z);
                BattleManager.instance.StartBattle();
                break;
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
                TurnPortalsOnOff(true);
                break;
            case 8:
                TurnPortalsOnOff(true);
                break;
            case 9:
                TurnPortalsOnOff(false);
                roomImage.transform.position = new Vector3(GameManager.instance.monsterPaths[curRoom.pathID].transform.position.x, GameManager.instance.monsterPaths[curRoom.pathID].transform.position.y - 2.5f, roomImage.transform.position.z);
                BattleManager.instance.StartBattle();
                break;
        }
        //카메라를 해당하는 전장으로 이동한다.
        Camera.main.transform.position = roomImage.transform.position;
        Camera.main.transform.Translate(0, 1.5f, -15);
    } 

    public void ChangeCurRoomToIdle()
    {
        curRoom.type = 0;
    }

    public void TurnPortalsOnOff(bool isOn)
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
                    if (adjRoom.type < 2 || adjRoom.type == 9) portals[i].SetInteger("portalType", adjRoom.type);
                    else portals[i].SetInteger("portalType", 2);
                }
                else
                {
                    portals[i].SetInteger("portalType", -1);
                    portals[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                portals[i].gameObject.SetActive(false);
            }
        }
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
                MoveToRoom(playerX + dx[dir], playerY + dy[dir]);
            }

        }
    }
}
