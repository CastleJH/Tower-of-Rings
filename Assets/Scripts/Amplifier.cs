using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amplifier : MonoBehaviour
{
    List<Monster> monsters; //������ ������ �޴� ���͵�
    Ring parent;        //�θ�
    float coolTime;     //��ƼŬ ��Ÿ��

    void Awake()
    {
        monsters = new List<Monster>();
    }

    void Update()
    {
        coolTime += Time.deltaTime;
        //�������� ������ ������ �� �ϳ��� �����Ǹ� �Ͻ������� isInAmplify�� false�� ����� �� �ִ�. ���� �� ������ ��� true�� �ٲ���� ��.
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].isInAmplify = true;
        if (coolTime > 0.5f)    //0.5�ʸ��� ��ƼŬ ���
        {
            coolTime = 0.0f;
            for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].PlayParticleCollision(parent.ringBase.id, 0.0f);
        }
        //�θ��� �������� ���ŵǸ� ���ε� �����Ѵ�.
        if (!parent.gameObject.activeSelf) RemoveFromBattle();
    }

    //������ �ʱ�ȭ�Ѵ�.
    public void InitializeAmplifier(Ring par)
    {
        monsters.Clear();
        parent = par;
        transform.position = new Vector3(par.transform.position.x, par.transform.position.y, -0.002f);
        transform.localScale = new Vector3(par.ringBase.range * 2, par.ringBase.range * 2, 1);
    }

    //������ �������� �����Ѵ�. �����ϸ鼭 ������ �޴� ��� ���͵��� �߰� �ǰ� ���¸� �����Ѵ�.
    public void RemoveFromBattle()
    {
        for (int i = monsters.Count - 1; i >= 0; i--) monsters[i].isInAmplify = false;
        monsters.Clear();
        GameManager.instance.ReturnAmplifierToPool(this);
    }

    //������ ���Ͱ� ������ �߰� �ǰ� ���·� �����.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monsters.Add(monster);
        }
    }

    //���Ͱ� ������ ������ �߰� �ǰ� ���¸� �����Ѵ�.
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Monster")
        {
            Monster monster = collision.GetComponent<Monster>();
            monster.isInAmplify = false;
            monsters.Remove(monster);
        }
    }
}
