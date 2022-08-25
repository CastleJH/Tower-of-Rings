//몬스터 원형의 정보. 모든 Monster는 BaseMonster를 가지며 이 클래스의 변수들로 스탯값이 정해진다.
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
    public float initMaxHP;
    public float baseMaxHP;
    public float initSPD;
    public float baseSPD;       

    //기타
    public char tier;       //몬스터의 처치 난이도
    public string description;  //설명

    //DB 데이터로 몬스터 원형의 스탯 초기화(유물, 플레이어 스탯등이 적용되지 않은 최초 값)
    public void Init(float mult) 
    {
        baseATK = csvATK;
        initMaxHP = csvHP * mult;
        baseMaxHP = initMaxHP;
        initSPD = csvSPD;
        baseSPD = initSPD;
    }
}