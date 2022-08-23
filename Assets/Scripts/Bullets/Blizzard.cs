using System.Collections.Generic;
using UnityEngine;

public class Blizzard : MonoBehaviour
{
    List<Monster> monsters; //������ ������ �޴� ���͵�
    Ring parent;            //�θ�
    float coolTime;         //���� ��Ÿ��

    void Awake()
    {
        monsters = new List<Monster>();
    }

    void Update()
    {
        coolTime += Time.deltaTime;
        
        //�����󳢸� ������ ������ �� �ϳ��� �����Ǹ� �Ͻ������� isInBlizzard�� false�� ����� �� �ִ�. ���� �� ������ ��� true�� �ٲ���� ��.
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].isInBlizzard = true; 
        
        if (coolTime > 1.0f)    //1�ʸ��� ����
        {
            coolTime = 0.0f;
            for (int i = monsters.Count - 1; i>= 0; i--) monsters[i].AE_DecreaseHP(parent.curATK, new Color32(50, 50, 255, 255));
        }
        
        //�θ��� �������� ���ŵǸ� ���ε� �����Ѵ�.
        if (!parent.gameObject.activeSelf) RemoveFromBattle();
    }

    //������ �ʱ�ȭ�Ѵ�.
    public void InitializeBlizzard(Ring par)
    {
        monsters.Clear();
        parent = par;
        transform.position = new Vector3(par.transform.position.x, par.transform.position.y, -0.2f);
        transform.localScale = new Vector3(par.baseRing.range * 2, par.baseRing.range * 2, 1);
        coolTime = 0.0f;
    }

    //������ �������� �����Ѵ�. �����ϸ鼭 ������ �޴� ��� ���͵��� ��ȭ�� �����Ѵ�.
    public void RemoveFromBattle()
    {
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].isInBlizzard = false;
        monsters.Clear();
        GameManager.instance.ReturnBlizzardToPool(this);
    }

    //������ ���Ͱ� ������ ��ȭ ���·� �����.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monsters.Add(monster);
        }
    }

    //���Ͱ� ������ ������ ��ȭ ���¸� �����Ѵ�.
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monster.isInBlizzard = false;
            monsters.Remove(monster);
        }
    }
}
