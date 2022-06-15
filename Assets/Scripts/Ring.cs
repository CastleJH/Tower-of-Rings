using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public Ringstone ringBase;  //���� �⺻ ����

    //����
    public float curNumTarget;  //Ÿ�� ��
    public float curDMG;   //���� ������
    public float curSPD;   //���� ���� �ӵ�
    public float curEFF;   //���� ȿ�� ���� �ð�

    //Ÿ��
    public List<Monster> targets; //���ݹ��� ���� ���͵�

    //�׷���
    public SpriteRenderer spriteRenderer;   //�� ��ü ������
    public SpriteRenderer rangeRenderer;    //���� ���� ǥ�� ������

    //��Ÿ ������
    public bool isInBattle;         //���� ������ üũ��
    public float shootCoolTime;    //�߻� ��Ÿ�� üũ��
    public CircleCollider2D collider; //�ڽ��� �ݶ��̴�

    void Awake()
    {
        targets = new List<Monster>();
        collider = GetComponent<CircleCollider2D>(); 
    }

    void Update()
    {
        if (isInBattle)
        {
            shootCoolTime += Time.deltaTime;
            if (shootCoolTime > ringBase.baseSPD)
            {
                Attack();
                shootCoolTime = 0.0f;
            }
        }
    }

    //id�� �ش��ϴ� ������ ���Ѵ�.
    public void InitializeRing(int id)
    {
        //���� ����
        ringBase = GameManager.instance.ringstoneDB[id];

        //�׷���
        spriteRenderer.sprite = GameManager.instance.ringSprites[id];
        rangeRenderer.transform.localScale = new Vector2(ringBase.range * 2, ringBase.range * 2);
        rangeRenderer.color = new Color(0, 0, 0, 0);

        //��Ÿ ���� �ʱ�ȭ
        isInBattle = false;
        shootCoolTime = ringBase.baseSPD - 0.2f;
    }

    public void Attack()
    {
        Debug.Log("Attack!");
    }

    //��ġ ���� �������� Ȯ���Ѵ�.
    public bool CheckArragePossible()
    {
        bool ret = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.75f);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "Monster Path" || colliders[i].tag == "Ring") break;
            else if (colliders[i].tag == "Land") ret = true;
        }
        if (ret) rangeRenderer.color = new Color32(0, 255, 0, 50);
        else rangeRenderer.color = new Color32(255, 0, 0, 50);
        return ret;
    }

    //���� Ÿ�ٵ��� ��´�.
    public void GetTargets()
    {
        targets.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ringBase.range);

        Monster monster;
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i].tag == "Monster")
            {
                monster = colliders[i].GetComponent<Monster>();
                if (monster.curHP > 0) targets.Add(monster);
            }
    }
}
