using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;

    public Image image;     //가림막
    bool isDarker;          //어두워지는 단계인가?
    float alpha;            //불투명도
    int bgmID;

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
            if (isDarker)
            {
                alpha += 0.012f;  //불투명도와 사운드 볼륨을 조정한다.
                GameManager.instance.audioSource.volume -= 0.010f;
            }
            else alpha -= 0.012f;
            alpha = Mathf.Clamp01(alpha);
            GameManager.instance.audioSource.volume = Mathf.Clamp(GameManager.instance.audioSource.volume, 0.0f, 0.8f);
            image.color = new Color(0, 0, 0, alpha);

            if (alpha == 1)    //완전히 불투명해지면 등록한 함수를 실행하고 투명화한다.
            {
                changeFunc(moveX, moveY);
                changeFunc = null;
                isDarker = false;
                GameManager.instance.audioSource.Stop();
            }
            else if (alpha == 0)
            {
                image.raycastTarget = false;    //완전히 투명해지면 가림막을 비활성화한다.
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

    //장면 전환하고, 전환 도중 화면이 완전히 불투명해졌을 때 실행할 함수를 등록한다.
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
