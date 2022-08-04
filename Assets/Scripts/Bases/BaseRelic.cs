using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRelic        //유물
{
    public int id;        //종류
    public string name;     //이름
    public bool have;
    public bool isCursed;   //저주 여부
    public string pureDescription;  //일반 설명(도감에서 나타날 정보)
    public string cursedDescription;  //저주 설명(도감에서 나타날 정보)
}