using System.Collections.Generic;
using UnityEngine;

//방 정보. 게임 도중 동시에 여러 개 존재한다.
public class Room
{
    public int type;			//방 타입(0:평화 1:전투 2:제련 3:링 4:유물 5:파괴 6:탐욕 7:회복 8:상점 9:보스)
	public bool visited;		//방 이전에 방문 여부
	public int pathID;			//방에 있는 몬스터 경로 타입(방 타입이 1, 9인 경우만 유효함)
	public List<Item> items;	//방에 있는 아이템들
	float sinkholeScale;		//싱크홀 크기
	Vector3 sinkholeNorthPos;	//북쪽 싱크홀 위치(y좌표가 100이면 싱크홀이 없다는 의미임)
	Vector3 sinkholeSouthPos;	//남쪽 싱크홀 위치(y좌표가 100이면 싱크홀이 없다는 의미임)

    public Room()
    {
		items = new List<Item>();
        sinkholeNorthPos = new Vector3(0, 100, 0);
        sinkholeSouthPos = new Vector3(0, 100, 0);
    }
	
	//방에 아이템을 추가한다.
	public void AddItem(Item item)
	{
		items.Add(item);
	}

	//방에 있는 모든 아이템을 제거하고 오브젝트 풀에 되돌린다.
	public void RemoveAllItems()
	{
		for (int i = items.Count - 1; i >= 0; i--) GameManager.instance.ReturnItemToPool(items[i]);
		items.Clear();
	}

	//방에 싱크홀을 추가한다.
	public void AddSinkhole()
	{
        sinkholeScale = Random.Range(0.5f, 1.0f);
        if (Random.Range(0, 5) < 2) sinkholeNorthPos = new Vector3(Random.Range(-6.0f, 6.0f), 2.5f + Random.Range(2.5f + 4.0f * (sinkholeScale - 0.5f), 12.0f), 5);
        else sinkholeNorthPos = new Vector3(0, 100, 0);
        if (Random.Range(0, 5) < 2) sinkholeSouthPos = new Vector3(Random.Range(-6.0f, 6.0f), 2.5f - Random.Range(2.5f + 4.0f * (sinkholeScale - 0.5f), 12.0f), 5);
        else sinkholeSouthPos = new Vector3(0, 100, 0);
    }

	//싱크홀을 보여준다.
	public void ShowSinkhole()
    {
        GameManager.instance.sinkholeNorth.transform.localScale = Vector2.one * sinkholeScale;
        GameManager.instance.sinkholeNorth.transform.position = FloorManager.instance.roomImage.transform.position + sinkholeNorthPos;

        GameManager.instance.sinkholeSouth.transform.localScale = Vector2.one * (1.5f - sinkholeScale);
        GameManager.instance.sinkholeSouth.transform.position = FloorManager.instance.roomImage.transform.position + sinkholeSouthPos;
    }
}


//층 정보. 게임 도중 동시에 하나만 존재한다.
public class Floor
{
    public int startX, startY;					//시작 지점
	public int floorNum;						//현재 층 수
	public Room[,] rooms = new Room[11, 11];	//층을 구성하는 방들

	public Floor()
    {
		for (int i = 0; i < 11; i++)
			for (int j = 0; j < 11; j++)
				rooms[i, j] = new Room();
    }

	//알맞은 형태의 층을 생성한다.
	public void Generate(int _floorNum)
	{
		floorNum = _floorNum;	//층 수를 저장한다.

		int[] dr = { 0, 0, -1, 1 };		//인접한 방을 확인하기 위한 좌표이다.
		int[] dc = { -1, 1, 0, 0 };
		int roomNum, specialNum, genNum;		//각각 생성할 방 총 개수, 생성할 특별방 개수, 지금까지 생성한 방 개수이다. 
		List<KeyValuePair<int, int>> specials = new List<KeyValuePair<int, int>>();

		//층을 구성하는 방들의 개수를 정한다.
		roomNum = 7 + (floorNum + 1) / 2 * 3 + Random.Range(0, 3);  //10, 10, 13, 13, 16, 16, 19 + (0 ~ 2) //확정
		specialNum = 4 + (floorNum + 1) / 2 + Random.Range(0, 2); //4 4 5 5 6 6 7 + (0 ~ 1) 확정
		if (floorNum == 7) specialNum++;
		
        do
		{
			genNum = 0;

			//방 초기화
			for (int i = 1; i <= 9; i++)
				for (int j = 1; j <= 9; j++) 
					rooms[i, j].type = -1;

			for (int i = 0; i < 10; i++) rooms[0, i].type = rooms[i, 0].type = rooms[10, i].type = rooms[i, 10].type = 10;
			specials.Clear();

			Queue<KeyValuePair<int, int>> q = new Queue<KeyValuePair<int, int>>();
			KeyValuePair<int, int> cur, next;

			//시작점 랜덤 선택(시작점의 row col은 각각 4~6이다)
			cur = new KeyValuePair<int, int>(Random.Range(4, 7), Random.Range(4, 7));
			startX = cur.Key;
			startY = cur.Value;
			rooms[cur.Key, cur.Value].type = 0;
			q.Enqueue(cur);
			genNum++;

			while (q.Count > 0)
			{
				bool isSpecial = true;
				cur = q.Dequeue();

				for (int i = 0; i < 4; i++)
				{
					//이번에 검사할 방
					next = new KeyValuePair<int, int>(cur.Key + dr[i], cur.Value + dc[i]);

					//이미 채워졌으면 포기
					if (rooms[next.Key, next.Value].type != -1) continue;

					//다시 해당 방에서 인접한 방들 중 두 개 이상 채워졌다면 포기
					int adj = 0;
					for (int j = 0; j < 4; j++)
						if (rooms[next.Key + dr[j], next.Value + dc[j]].type != -1) adj++;
					if (adj > 1) continue;

					//이미 충분히 생성했으면 포기(이 조건을 반복문 내부에서 체크하지 않으면 special방들이 제대로 저장되지 않는다)
					if (genNum == roomNum) continue;

					//50% 확률로 포기
					if (Random.Range(0, 2) == 1) continue;

					//이 방은 선택되었다.
					rooms[next.Key, next.Value].type = 1;
					genNum++;
					q.Enqueue(next);

					//기존 방을 매개로 이 방이 생성되었으므로 기존 방은 일반 방이다.
					isSpecial = false;
				}
				if (isSpecial) specials.Add(cur);
			}
		} while (roomNum != genNum || specials.Count != specialNum);

		List<bool> occupied = new List<bool>();
		for (int i = 0; i < specialNum; i++) occupied.Add(false);

		//특수 방들 중 가장 먼 방을 보스 방으로 만듦
		rooms[specials[specialNum - 1].Key, specials[specialNum - 1].Value].type = 9;
		occupied[specialNum - 1] = true;

		//상점을 만들어야 하는 경우 가장 시작점과 가까운 특수 방을 상점으로 만듦
		if (floorNum % 2 == 0 || floorNum == 7)
		{
			rooms[specials[0].Key, specials[0].Value].type = 8;
			occupied[0] = true;
		}

		//제련/링/유물의 방을 1개씩 생성함.
		for (int i = 2; i <= 4; i++)
		{
			int idx;
			do idx = Random.Range(0, specialNum);
			while (occupied[idx]);
			rooms[specials[idx].Key, specials[idx].Value].type = i;
			occupied[idx] = true;
		}

		//남은 방들을 유물의 방을 제외한 랜덤한 미스터리 방으로 만듦.
		for (int i = 0; i < specialNum; i++)
		{
			if (occupied[i]) continue;
			do rooms[specials[i].Key, specials[i].Value].type = 2 + Random.Range(0, 6); 
			while (rooms[specials[i].Key, specials[i].Value].type == 4);
        }

		//모든 전투 방들의 전장 형태를 결정함.
		for (int i = 1; i <= 9; i++)
			for (int j = 1; j <= 9; j++)
				if (rooms[i, j].type == 1 || rooms[i, j].type == 9) 
					rooms[i, j].pathID = Random.Range(0, GameManager.instance.monsterPaths.Length);
	}
}
