//���� ������
public class BaseRelic
{
    public int id;          //ID
    public string name;     //�̸�
    public bool have;       //���� ����
    public bool isPure;     //���� ����
    public string pureDescription;      //�⺻ ����
    public string cursedDescription;    //���� ����

    //������ �������� ���� ���·� �ʱ�ȭ
    public void Init()   
    {
        have = false;
        isPure = true;
    }
}