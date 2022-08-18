using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int type;
	public bool visited;
	public int pathID;
	public List<Item> items;
	public float sinkholeScale;
	public bool sinkholeNorthOn;
	public bool sinkholeSouthOn;
	public Vector3 sinkholeNorthPos;
	public Vector3 sinkholeSouthPos;

    public Room()
    {
		items = new List<Item>();
        sinkholeNorthOn = false;
        sinkholeSouthOn = false;
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

	public void AddSinkhole()
	{
        sinkholeScale = Random.Range(0.5f, 1.0f);
        if (Random.Range(0, 5) < 2)
        {
			Debug.Log("North");
			sinkholeNorthOn = true;
            sinkholeNorthPos = new Vector3(Random.Range(-6.0f, 6.0f), 2.5f + Random.Range(2.5f + 4.0f * (sinkholeScale - 0.5f), 12.0f), 5);
        }
		else sinkholeNorthOn = false;
        if (Random.Range(0, 5) < 2)
        {
            Debug.Log("South");
            sinkholeSouthOn = true;
            sinkholeSouthPos = new Vector3(Random.Range(-6.0f, 6.0f), 2.5f - Random.Range(2.5f + 4.0f * (sinkholeScale - 0.5f), 12.0f), 5);
        }
        else sinkholeSouthOn = false;
    }

	public void ShowSinkhole()
    {
        if (sinkholeNorthOn)
        {
            GameManager.instance.sinkholeNorth.transform.localScale = Vector2.one * sinkholeScale;
            GameManager.instance.sinkholeNorth.transform.position = FloorManager.instance.roomImage.transform.position + sinkholeNorthPos;
            GameManager.instance.sinkholeNorth.SetActive(true);
        }
        else GameManager.instance.sinkholeNorth.SetActive(false);
        if (sinkholeSouthOn)
        {
            Debug.Log("South");
            GameManager.instance.sinkholeSouth.transform.localScale = Vector2.one * (1.5f - sinkholeScale);
            GameManager.instance.sinkholeSouth.transform.position = FloorManager.instance.roomImage.transform.position + sinkholeSouthPos;
            GameManager.instance.sinkholeSouth.SetActive(true);
        }
        else GameManager.instance.sinkholeSouth.SetActive(false);
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

		//���� ������ ����
		roomNum = 8 + floorNum * 2 + Random.Range(0, 3);
        specialNum = 3 + (int)((floorNum + 1) * 0.7f); //4 5 5 6 7 7 8
		if (floorNum % 2 == 0) store = 1;
		else store = 0;

		//������ �����ؾ� �ϸ� 1�� �� �÷��ش�.
		if (store == 1) specialNum++;

		do
		{
			genNum = 0;

			//�� �ʱ�ȭ
			for (int i = 1; i <= 9; i++)
				for (int j = 1; j <= 9; j++) 
					rooms[i, j].type = -1;

			for (int i = 0; i < 10; i++) rooms[0, i].type = rooms[i, 0].type = rooms[10, i].type = rooms[i, 10].type = 10;
			specials.Clear();

			Queue<KeyValuePair<int, int>> q = new Queue<KeyValuePair<int, int>>();
			KeyValuePair<int, int> cur, next;

			//������ ���� ����(�������� row col�� ���� 4~6�̴�)
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
					//�̹��� �˻��� ��
					next = new KeyValuePair<int, int>(cur.Key + dr[i], cur.Value + dc[i]);

					//�̹� ä�������� ����
					if (rooms[next.Key, next.Value].type != -1) continue;

					//�ٽ� �ش� �濡�� ������ ��� �� �� �� �̻� ä�����ٸ� ����
					int adj = 0;
					for (int j = 0; j < 4; j++)
						if (rooms[next.Key + dr[j], next.Value + dc[j]].type != -1) adj++;
					if (adj > 1) continue;

					//�̹� ����� ���������� ����(�� ������ �ݺ��� ���ο��� üũ���� ������ special����� ����� ������� �ʴ´�)
					if (genNum == roomNum) continue;

					//50% Ȯ���� ����
					if (Random.Range(0, 2) == 1) continue;

					//�� ���� ���õǾ���.
					rooms[next.Key, next.Value].type = 1;
					genNum++;
					q.Enqueue(next);

					//���� ���� �Ű��� �� ���� �����Ǿ����Ƿ� ���� ���� �Ϲ� ���̴�.
					isSpecial = false;
				}
				if (isSpecial) specials.Add(cur);
			}
		} while (roomNum != genNum || specials.Count != specialNum);

		List<bool> occupied = new List<bool>();
		for (int i = 0; i < specialNum; i++) occupied.Add(false);

		//Ư�� ��� �� ���� �� ���� ���� ������ ����
		rooms[specials[specialNum - 1].Key, specials[specialNum - 1].Value].type = 9;
		occupied[specialNum - 1] = true;

		//������ ������ �ϴ� ��� ���� �������� ����� Ư�� ���� �������� ����
		if (store == 1)
		{
			rooms[specials[0].Key, specials[0].Value].type = 8;
			occupied[0] = true;
		}

		//����/��/������ ���� 1���� ������.
		for (int i = 2; i <= 4; i++)
		{
			int idx;
			do idx = Random.Range(0, specialNum);
			while (occupied[idx]);
			rooms[specials[idx].Key, specials[idx].Value].type = i;
			occupied[idx] = true;
		}

		//���� ����� ������ ���� ������ ������ �̽��͸� ������ ����.
		for (int i = 0; i < specialNum; i++)
		{
			if (occupied[i]) continue;
			do rooms[specials[i].Key, specials[i].Value].type = 2 + Random.Range(0, 6); 
			while (rooms[specials[i].Key, specials[i].Value].type == 4);
			//���켼��!
			//rooms[specials[i].Key, specials[i].Value].type = 2;
        }

		//��� ���� ����� ���� ���¸� ������.
		for (int i = 1; i <= 9; i++)
			for (int j = 1; j <= 9; j++)
				if (rooms[i, j].type == 1 || rooms[i, j].type == 9) 
					rooms[i, j].pathID = Random.Range(0, GameManager.instance.monsterPaths.Length);
	}
}
