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
        if (isPortalOn) GetInput();
    }

    //�ش� ���� ���� �����ϰ� �̵��Ѵ�.
    public void CreateAndMoveToFloor(int f)
    {
        UIManager.instance.nextFloorButton.SetActive(false);
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
        UIManager.instance.gameStartPanel.SetActive(false);

        if (curRoom != null) curRoom.HideItems();

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
                Debug.Log("Battle Start");
                TurnPortalsOnOff(false);
                roomImage.transform.position = new Vector3(GameManager.instance.monsterPaths[curRoom.pathID].transform.position.x, GameManager.instance.monsterPaths[curRoom.pathID].transform.position.y - 2.5f, roomImage.transform.position.z);
                BattleManager.instance.StartBattle();
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
                Debug.Log("Battle Start");
                TurnPortalsOnOff(false);
                roomImage.transform.position = new Vector3(GameManager.instance.monsterPaths[curRoom.pathID].transform.position.x, GameManager.instance.monsterPaths[curRoom.pathID].transform.position.y - 2.5f, roomImage.transform.position.z);
                BattleManager.instance.StartBattle();
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
                SceneChanger.instance.ChangeScene(MoveToRoom, playerX + dx[dir], playerY + dy[dir]);
            }

        }
    }
}
