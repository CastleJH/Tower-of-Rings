public class BaseMonster
{
    public int type;        //종류
    public string name;     //이름

    //db상 스탯
    public int csvATK;
    public float csvHP;
    public float csvSPD;

    //플레이어 스탯 & 유물 등에 의해 조율된 스탯
    public int baseATK;
    public float baseMaxHP;
    public float baseSPD;       


    public char tier;       //몬스터의 난이도
    public string description;  //설명


    public void Init()
    {
        baseATK = csvATK;
        baseMaxHP = csvHP;
        baseSPD = csvSPD;
    }
}