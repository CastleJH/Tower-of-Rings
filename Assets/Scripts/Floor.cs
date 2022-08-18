using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int type;
	public bool visited;
	public int pathID;
	public List<Item> items;

    public Room()
    {
		items = new List<Item>();
    }

	public void AddItem(Item item)
	{
		items.Add(item);
	}

	public void RemoveAllItems()
	{
		for (int i = items.Count - 1; i >= 0; i--) GameManager.instance.ReturnItemToPool(items[i]);
		items.Clear();
	}
}

public class Floor
{
    public int startX, startY;
	public int floorNum;
	public Room[,] rooms = new Room[11, 11];

	public Floor()
    {
		for (int i = 0; i < 11; i++)
			for (int j = 0; j < 11; j++)
				rooms[i, j] = new Room();
    }

	public void Generate(int _floorNum)
	{
		floorNum = _floorNum;

		int[] dr = { 0, 0, -1, 1 };
		int[] dc = { -1, 1, 0, 0 };
		int roomNum, specialNum, store, genNum;
		List<KeyValuePair<int, int>> specials = new List<KeyValuePair<int, int>>();

		//방의 구성을 정함
		roomNum = 8 + floorNum * 2 + Random.Range(0, 3);
        specialNum = 3 + (int)((floorNum + 1) * 0.7f); //4 5 5 6 7 7 8
		if (floorNum % 2 == 0) store = 1;
		else store = 0;

		//상점도 생성해야 하면 1개 더 늘려준다.
		if (store == 1) specialNum++;

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

					//이 방은 선택되엇다.
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
		if (store == 1)
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
			//지우세요!
			//rooms[specials[i].Key, specials[i].Value].type = 2;
        }

		//모든 전투 방들의 전장 형태를 결정함.
		for (int i = 1; i <= 9; i++)
			for (int j = 1; j <= 9; j++)
				if (rooms[i, j].type == 1 || rooms[i, j].type == 9) 
					rooms[i, j].pathID = Random.Range(0, GameManager.instance.monsterPaths.Length);
	}
}
