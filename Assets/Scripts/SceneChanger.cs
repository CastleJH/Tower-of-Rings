using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;

    public Image image;     //������
    bool isDarker;          //��ο����� �ܰ��ΰ�?
    float alpha;            //������
    int bgmID;

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
            if (isDarker)
            {
                alpha += 0.012f;  //�������� ���� ������ �����Ѵ�.
                GameManager.instance.audioSource.volume -= 0.010f;
            }
            else alpha -= 0.012f;
            alpha = Mathf.Clamp01(alpha);
            GameManager.instance.audioSource.volume = Mathf.Clamp(GameManager.instance.audioSource.volume, 0.0f, 0.8f);
            image.color = new Color(0, 0, 0, alpha);

            if (alpha == 1)    //������ ������������ ����� �Լ��� �����ϰ� ����ȭ�Ѵ�.
            {
                changeFunc(moveX, moveY);
                changeFunc = null;
                isDarker = false;
                GameManager.instance.audioSource.Stop();
            }
            else if (alpha == 0)
            {
                image.raycastTarget = false;    //������ ���������� �������� ��Ȱ��ȭ�Ѵ�.
                GameManager.instance.audioSource.volume = 0.8f;
                if (bgmID == -1)
                    switch (FloorManager.instance.curRoom.type)
                    {
                        case 1:
                        case 9:
                            if (BattleManager.instance.isBattlePlaying) bgmID = 1;
                            else bgmID = 3;
                            break;
                        case 8:
                            bgmID = 2;
                            break;
                        default:
                            bgmID = 3;
                            break;
                    }
                GameManager.instance.audioSource.clip = GameManager.instance.bgms[bgmID];
                GameManager.instance.audioSource.Play();
            }
        }
    }

    //��� ��ȯ�ϰ�, ��ȯ ���� ȭ���� ������ ������������ �� ������ �Լ��� ����Ѵ�.
    public void ChangeScene(ChangeFunc func, int x, int y, int _bgmID)
    {
        image.raycastTarget = true;
        isDarker = true;
        moveX = x;
        moveY = y;
        bgmID = _bgmID;
        changeFunc += func;
    }
}
