using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    int type;
    int upgrade;
    int ring;
    int relic;
    int destroy;
    int gold;
    int emerald;
    float recover;
}

public class Floor
{
    int startX, startY;
    Room[,] floor = new Room[11, 11];
}

public class FloorGenerator
{
    Floor GenerateFloor(int roomNum, int special, int store)
    {
        Floor floor = new Floor();

        return floor;
    }
}
