//���� ������ ����. ��� Monster�� BaseMonster�� ������ �� Ŭ������ ������� ���Ȱ��� ��������.
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

    //��Ÿ
    public char tier;       //������ óġ ���̵�
    public string description;  //����

    //DB �����ͷ� ���� ������ ���� �ʱ�ȭ(����, �÷��̾� ���ȵ��� ������� ���� ���� ��)
    public void Init() 
    {
        baseATK = csvATK;
        baseMaxHP = csvHP;
        baseSPD = csvSPD;
    }
}