public class BaseRelic
{
    public int id;          //ID
    public string name;     //이름
    public bool have;       //보유 여부
    public bool isPure;     //저주 여부
    public string pureDescription;      //원형 설명
    public string cursedDescription;    //저주 설명

    public void Init()
    {
        have = false;
        isPure = true;
    }
}