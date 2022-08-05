using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SceneChanger : MonoBehaviour
{
    public Image image;
    bool isDarker;
    float alpha;

    public static SceneChanger instance;

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
        if (image.raycastTarget)
        {
            if (isDarker) alpha += 0.012f;
            else alpha -= 0.012f;
            alpha = Mathf.Clamp01(alpha);
            image.color = new Color(0, 0, 0, alpha);
            if (alpha == 0) image.raycastTarget = false;
            else if (alpha == 1)
            {
                changeFunc(moveX, moveY);
                changeFunc = null;
                isDarker = false;
            }
        }
    }

    public void ChangeScene(ChangeFunc func, int x, int y)
    {
        image.raycastTarget = true;
        isDarker = true;
        moveX = x;
        moveY = y;
        changeFunc += func;
    }
}
