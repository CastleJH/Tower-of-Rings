using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;

    public Image image;     //가림막
    bool isDarker;          //어두워지는 단계인가?
    float alpha;            //불투명도

    //델리게이트
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
        if (image.raycastTarget) //가림막 활성화인 경우
        {
            if (isDarker) alpha += 0.012f;  //불투명도를 조정한다.
            else alpha -= 0.012f;
            alpha = Mathf.Clamp01(alpha);
            image.color = new Color(0, 0, 0, alpha);

            if (alpha == 1)    //완전히 불투명해지면 등록한 함수를 실행하고 투명화한다.
            {
                changeFunc(moveX, moveY);
                changeFunc = null;
                isDarker = false;
            }
            else if (alpha == 0) image.raycastTarget = false;    //완전히 투명해지면 가림막을 비활성화한다.
        }
    }

    //장면 전환하고, 전환 도중 화면이 완전히 불투명해졌을 때 실행할 함수를 등록한다.
    public void ChangeScene(ChangeFunc func, int x, int y)
    {
        image.raycastTarget = true;
        isDarker = true;
        moveX = x;
        moveY = y;
        changeFunc += func;
    }
}
