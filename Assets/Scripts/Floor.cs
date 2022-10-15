using System.Collections.Generic;
using UnityEngine;

//�� ����. ���� ���� ���ÿ� ���� �� �����Ѵ�.
public class Room
{
    public int type;			//�� Ÿ��(0:��ȭ 1:���� 2:���� 3:�� 4:���� 5:�ı� 6:Ž�� 7:ȸ�� 8:���� 9:����)
	public bool visited;		//�� ������ �湮 ����
	public int pathID;			//�濡 �ִ� ���� ��� Ÿ��(�� Ÿ���� 1, 9�� ��츸 ��ȿ��)
	public List<Item> items;	//�濡 �ִ� �����۵�
	float sinkholeScale;		//��ũȦ ũ��
	Vector3 sinkholeNorthPos;	//���� ��ũȦ ��ġ(y��ǥ�� 100�̸� ��ũȦ�� ���ٴ� �ǹ���)
	Vector3 sinkholeSouthPos;	//���� ��ũȦ ��ġ(y��ǥ�� 100�̸� ��ũȦ�� ���ٴ� �ǹ���)

    public Room()
    {
		items = new List<Item>();
        sinkholeNorthPos = new Vector3(0, 100, 0);
        sinkholeSouthPos = new Vector3(0, 100, 0);
    }
	
	//�濡 �������� �߰��Ѵ�.
	public void AddItem(Item item)
	{
		items.Add(item);
	}

	//�濡 �ִ� ��� �������� �����ϰ� ������Ʈ Ǯ�� �ǵ�����.
	public void RemoveAllItems()
	{
		for (int i = items.Count - 1; i >= 0; i--) GameManager.instance.ReturnItemToPool(items[i]);
		items.Clear();
	}

	//�濡 ��ũȦ�� �߰��Ѵ�.
	public void AddSinkhole()
	{
        sinkholeScale = Random.Range(0.5f, 1.0f);
        if (Random.Range(0, 5) < 2) sinkholeNorthPos = new Vector3(Random.Range(-6.0f, 6.0f), 2.5f + Random.Range(2.5f + 4.0f * (sinkholeScale - 0.5f), 12.0f), 5);
        else sinkholeNorthPos = new Vector3(0, 100, 0);
        if (Random.Range(0, 5) < 2) sinkholeSouthPos = new Vector3(Random.Range(-6.0f, 6.0f), 2.5f - Random.Range(2.5f + 4.0f * (sinkholeScale - 0.5f), 12.0f), 5);
        else sinkholeSouthPos = new Vector3(0, 100, 0);
    }

	//��ũȦ�� �����ش�.
	public void ShowSinkhole()
    {
        GameManager.instance.sinkholeNorth.transform.localScale = Vector2.one * sinkholeScale;
        GameManager.instance.sinkholeNorth.transform.position = FloorManager.instance.roomImage.transform.position + sinkholeNorthPos;

        GameManager.instance.sinkholeSouth.transform.localScale = Vector2.one * (1.5f - sinkholeScale);
        GameManager.instance.sinkholeSouth.transform.position = FloorManager.instance.roomImage.transform.position + sinkholeSouthPos;
    }
}


//�� ����. ���� ���� ���ÿ� �ϳ��� �����Ѵ�.
public class Floor
{
    public int startX, startY;					//���� ����
	public int floorNum;						//���� �� ��
	public Room[,] rooms = new Room[11, 11];	//���� �����ϴ� ���

	public Floor()
    {
		for (int i = 0; i < 11; i++)
			for (int j = 0; j < 11; j++)
				rooms[i, j] = new Room();
    }

	//�˸��� ������ ���� �����Ѵ�.
	public void Generate(int _floorNum)
	{
		FloorManager.instance.isNotTutorial = true;
        floorNum = _floorNum;	//�� ���� �����Ѵ�.

		int[] dr = { 0, 0, -1, 1 };		//������ ���� Ȯ���ϱ� ���� ��ǥ�̴�.
		int[] dc = { -1, 1, 0, 0 };
		int roomNum, specialNum, genNum;		//���� ������ �� �� ����, ������ Ư���� ����, ���ݱ��� ������ �� �����̴�. 
		List<KeyValuePair<int, int>> specials = new List<KeyValuePair<int, int>>();

		//���� �����ϴ� ����� ������ ���Ѵ�.
		roomNum = 7 + (floorNum + 1) / 2 * 3 + Random.Range(0, 3);  //10, 10, 13, 13, 16, 16, 19 + (0 ~ 2) //Ȯ��
		specialNum = 4 + (floorNum + 1) / 2 + Random.Range(0, 2); //4 4 5 5 6 6 7 + (0 ~ 1) Ȯ��
		if (floorNum == 7) specialNum++;
		
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
		if (floorNum % 2 == 0 || floorNum == 7)
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
        }

		//��� ���� ����� ���� ���¸� ������.
		for (int i = 1; i <= 9; i++)
			for (int j = 1; j <= 9; j++)
				if (rooms[i, j].type == 1 || rooms[i, j].type == 9) 
					rooms[i, j].pathID = Random.Range(0, GameManager.instance.monsterPaths.Length);
	}

	public void GenerateTutorial()
	{
		FloorManager.instance.isNotTutorial = false;
        floorNum = 0;

		startX = 6;
		startY = 5;

        for (int i = 1; i <= 9; i++)
            for (int j = 1; j <= 9; j++)
                rooms[i, j].type = -1;
        
		rooms[6, 5].type = 0;
		rooms[5, 5].type = rooms[4, 5].type = 1;
		rooms[5, 6].type = 3;
		rooms[4, 4].type = 2;
		rooms[3, 5].type = 9;
    }
}
