public class BaseMonster
{
    public int type;        //����
    public string name;     //�̸�

    //db�� ����
    public int csvATK;
    public float csvHP;
    public float csvSPD;

    //�÷��̾� ���� & ���� � ���� ������ ����
    public int baseATK;
    public float baseMaxHP;
    public float baseSPD;       


    public char tier;       //������ ���̵�
    public string description;  //����


    public void Init()
    {
        baseATK = csvATK;
        baseMaxHP = csvHP;
        baseSPD = csvSPD;
    }
}