#include <iostream>
#include <algorithm>
#include <queue>
#include <vector>
#include <ctime>
using namespace std;
typedef pair<int, int> pii;

int room[11][11];	//0:시작		1:일반	 2~7:미스터리		8:상점	9:보스	10:벽을 의미한다.
vector<pii> specials;
int dr[] = { 0, 0, -1, 1 };
int dc[] = { -1, 1, 0, 0 };
int roomNum, genNum, minSpecial, store;

void GenerateStage() {
	do {
		genNum = 0;

		//방 초기화
		for (int i = 1; i <= 9; i++)
			for (int j = 1; j <= 9; j++) {
				room[i][j] = -1;
			}
		for (int i = 0; i < 10; i++) room[0][i] = room[i][0] = room[10][i] = room[i][10] = 10;
		specials.clear();

		queue<pii> q;
		pii cur, next;

		//시작점 랜덤 선택(시작점의 row col은 각각 4~6이다)
		cur = { rand() % 3 + 4, rand() % 3 + 4 };
		room[cur.first][cur.second] = 0;
		q.push(cur);
		genNum++;

		while (!q.empty()) {
			bool isSpecial = true;
			cur = q.front();
			q.pop();
			for (int i = 0; i < 4; i++) {
				//이번에 검사할 방
				next = { cur.first + dr[i], cur.second + dc[i] };

				//이미 채워졌으면 포기
				if (room[next.first][next.second] != -1) continue;

				//다시 해당 방에서 인접한 방들 중 두 개 이상 채워졌다면 포기
				int adj = 0;
				for (int i = 0; i < 4; i++)
					if (room[next.first + dr[i]][next.second + dc[i]] != -1) adj++;
				if (adj > 1) continue;

				//이미 충분히 생성했으면 포기(이 조건을 반복문 내부에서 체크하지 않으면 special방들이 제대로 저장되지 않는다)
				if (genNum == roomNum) continue;

				//50% 확률로 포기
				if (rand() & 1) continue;

				//이 방은 선택되엇다.
				room[next.first][next.second] = true;
				genNum++;
				q.push(next);

				//기존 방을 매개로 이 방이 생성되었으므로 기존 방은 일반 방이다.
				isSpecial = false;
			}
			if (isSpecial) specials.push_back(cur);
		}
	} while (roomNum != genNum || specials.size() != minSpecial);


	vector<bool> occupied(minSpecial, false);
	//특수 방들 중 가장 먼 방을 보스 방으로 만듦
	room[specials[minSpecial - 1].first][specials[minSpecial - 1].second] = 9;
	occupied[minSpecial - 1] = true;

	//상점을 만들어야 하는 경우 가장 시작점과 가까운 특수 방을 상점으로 만듦
	if (store) {
		room[specials[0].first][specials[0].second] = 8;
		occupied[0] = true;
	}

	//특수 방들 중 마지막을 빼고 랜덤한 미스터리 방으로 만듦. 유물 방은 반드시 1개만, 제련/링의 방은 반드시 1개 이상 생성함.
	for (int i = 2; i <= 4; i++) {
		int idx;
		do {
			idx = rand() % minSpecial;
		} while (occupied[idx]);
		room[specials[idx].first][specials[idx].second] = i;
		occupied[idx] = true;
	}
	for (int i = 0; i < minSpecial; i++) {
		if (occupied[i]) continue;
		do {
			room[specials[i].first][specials[i].second] = 2 + rand() % 6;
		} while (room[specials[i].first][specials[i].second] == 4);
	}


}

bool CheckValidInput() {
	if (roomNum > 32) {
		cout << "방 개수가 너무 많습니다.\n\n방 수(<=32) | 미스터리 수(3<=x<=min(방/2, 9(상점포함이면 8))) | 상점 여부(0, 1)? (공백 구분 입력)\n";
		return false;
	}
	if (minSpecial > min(9 - store, roomNum / 2) || minSpecial < 3) {
		cout << "미스터리 방 수가 올바르지 않습니다.\n\n방 수(<=32) | 미스터리 수(3<=x<=min(방/2, 9(상점포함이면 8))) | 상점 여부(0, 1)? (공백 구분 입력)\n";
		return false;
	}
	return true;
}

void PrintStage() {
	//생성된 스테이지 정보 출력
	for (int i = 1; i <= 9; i++) {
		for (int j = 1; j <= 9; j++) {
			switch (room[i][j]) {
			case -1: cout << "  "; break;
			case 0: cout << "ⓢ"; break;
			case 1: cout << "○"; break;
			case 2: cout << "②"; break;
			case 3: cout << "③"; break;
			case 4: cout << "④"; break;
			case 5: cout << "⑤"; break;
			case 6: cout << "⑥"; break;
			case 7: cout << "⑦"; break;
			case 8: cout << "ⓖ"; break;
			case 9: cout << "ⓑ"; break;
			}
		}
		cout << '\n';
	}
}

int main() {
	srand(time(0));

	cout << "방 수(<=32) | 미스터리 수(3<=x<=min(방/2, 9(상점포함이면 8))) | 상점 여부(0, 1)? (공백 구분 입력)\n";

	//생성할 방 개수, 미스터리 방 개수(상점포함), 상점 생성 여부를 입력받는다.
	while (cin >> roomNum >> minSpecial >> store) {
		if (!CheckValidInput()) continue;

		//보스 방은 specials에 포함된 상태로 생성할것이므로 1개 늘려준다. 그리고 상점도 생성해야 하면 1개 더 늘려준다.
		if (store) minSpecial += 2;
		else minSpecial++;

		GenerateStage();

		PrintStage();
		cout << "\n---------------------------------\n방 수(<=32) | 미스터리 수(3<=x<=min(방/2, 9(상점포함이면 8))) | 상점 여부(0, 1)? (공백 구분 입력)\n";
	}
	return 0;
}