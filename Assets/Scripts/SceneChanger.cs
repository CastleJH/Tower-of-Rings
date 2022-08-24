using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;

    public Image image;     //������
    bool isDarker;          //��ο����� �ܰ��ΰ�?
    float alpha;            //������

    //��������Ʈ
    public delegate void ChangeFunc(int x, int y);
    public event ChangeFunc changeFunc;
    private int moveX;
    private int moveY;

    void Awake()
    {
        instance = this;
        isDarker = false;
    }

    void Update()
    {
        if (image.raycastTarget) //������ Ȱ��ȭ�� ���
        {
            if (isDarker) alpha += 0.012f;  //�������� �����Ѵ�.
            else alpha -= 0.012f;
            alpha = Mathf.Clamp01(alpha);
            image.color = new Color(0, 0, 0, alpha);

            if (alpha == 1)    //������ ������������ ����� �Լ��� �����ϰ� ����ȭ�Ѵ�.
            {
                changeFunc(moveX, moveY);
                changeFunc = null;
                isDarker = false;
            }
            else if (alpha == 0) image.raycastTarget = false;    //������ ���������� �������� ��Ȱ��ȭ�Ѵ�.
        }
    }

    //��� ��ȯ�ϰ�, ��ȯ ���� ȭ���� ������ ������������ �� ������ �Լ��� ����Ѵ�.
    public void ChangeScene(ChangeFunc func, int x, int y)
    {
        image.raycastTarget = true;
        isDarker = true;
        moveX = x;
        moveY = y;
        changeFunc += func;
    }
}
