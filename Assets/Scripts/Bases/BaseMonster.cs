using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMonster        //몬스터의 원형
{
    public int type;        //종류
    public string name;     //이름
    public float hp;        //기본 HP
    public float spd;       //이동속도
    public string description;  //설명(도감에서 나타날 정보)
    public int atk;     //공격력
    public char tier;
}