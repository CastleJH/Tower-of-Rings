//유물 데이터
public class BaseRelic
{
    public int id;          //ID
    public string name;     //이름
    public bool have;       //보유 여부
    public bool isPure;     //저주 여부
    public string pureDescription;      //기본 설명
    public string cursedDescription;    //저주 설명

    //유물을 보유하지 않은 상태로 초기화
    public void Init()   
    {
        have = false;
        isPure = true;
    }
}