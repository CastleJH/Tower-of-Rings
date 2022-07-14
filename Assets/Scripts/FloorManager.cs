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
    public SpriteRenderer[] portals;
    public SpriteRenderer roomImage;

    //�� ���� ����
    public Floor floor;
    public Room curRoom;
    public int playerX, playerY;

    //�����¿� üũ��
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

    //�ش� ���� ���� �����ϰ� �̵��Ѵ�.
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

    //�ش� ������ �̵��Ѵ�.
    void MoveToRoom(int x, int y)
    {
        //���� ��ġ/�� ���� ������ �ٲ۴�.
        playerX = x;
        playerY = y;
        curRoom = floor.rooms[playerX, playerY];
        curRoom.visited = true;

        //������ ��ȭ�� �ش�.
        UIManager.instance.RevealMapArea(playerX, playerY);

        switch (curRoom.type)
        {
            case 0:
                TurnPortalsOnOff(true);
                break;
            case 1:
                TurnPortalsOnOff(false);
                roomImage.transform.position = new Vector3(GameManager.instance.monsterPaths[curRoom.pathID].transform.position.x, GameManager.instance.monsterPaths[curRoom.pathID].transform.position.y, roomImage.transform.position.z);
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
                roomImage.transform.position = GameManager.instance.monsterPaths[curRoom.pathID].transform.position;
                BattleManager.instance.StartBattle();
                break;
        }
        //ī�޶� �ش��ϴ� �������� �̵��Ѵ�.
        Camera.main.transform.position = roomImage.transform.position;
        Camera.main.transform.Translate(0, -2, -15);
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
                    if (adjRoom.visited) portals[i].sprite = GameManager.instance.portalSprites[adjRoom.type];
                    else if (adjRoom.type < 8) portals[i].sprite = GameManager.instance.portalSprites[1];
                    else portals[i].sprite = GameManager.instance.portalSprites[adjRoom.type];
                    portals[i].gameObject.SetActive(true);
                }
                else portals[i].gameObject.SetActive(false);
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
            Debug.Log("touched");
            Vector2 touchPos;

            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;
            touchPos = Camera.main.ScreenToWorldPoint(touchPos);

            RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero, 0f);

            if (hit.collider != null)
            {
                if (hit.collider.tag != "Portal")
                {
                    Debug.Log(hit.collider.tag);
                    return;
                }
                int dir = hit.collider.name[hit.collider.name.Length - 1] - '0';
                MoveToRoom(playerX + dx[dir], playerY + dy[dir]);
            }

        }
    }
}
