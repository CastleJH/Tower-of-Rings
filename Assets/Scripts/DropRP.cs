using UnityEngine;

public class DropRP : MonoBehaviour
{
    int rpLayerMask;    //레이어 마스크
    public Vector3 vel; //하강 속도

    public void Awake()
    {
        rpLayerMask = 1 << LayerMask.NameToLayer("RP");
    }

    void Update()
    {
        if (Time.timeScale == 0) return;    //일시정지 중이라면 작동하지 않는다.

        if (Input.GetMouseButtonDown(0))    //터치 지점에 자신이 있다면 RP를 준다.
        {
            if (TutorialManager.instance.isTutorial)
            {
                if (TutorialManager.instance.step < 16) return;
            }

            Vector2 touchPos;
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f, rpLayerMask);
            if (hit.collider != null && hit.collider.gameObject == gameObject) GiveRP();
        }

        //하강한다. 너무 아래로 내려오면 사라진다.
        transform.Translate(vel * Time.unscaledDeltaTime);
        if (transform.position.y < Camera.main.transform.position.y - 16.0f)
        {
            BattleManager.instance.dropRPs.Remove(this);
            GameManager.instance.ReturnDropRPToPool(this);
        }
    }


    //초기 위치와 유물에 의한 하강속도를 정한다.
    public void InitializeDropRP()
    {
        transform.position = Camera.main.transform.position + new Vector3(Random.Range(-5, 5), 16, 2);
        if (GameManager.instance.baseRelics[18].have)
        {
            if (GameManager.instance.baseRelics[18].isPure) vel = new Vector3(0, -1.5f, 0);
            else vel = new Vector3(0, -3.0f, 0);
        }
        else vel = new Vector3(0, -2.0f, 0);
    }

    //튜토리얼 용으로 수정한다.
    public void InitializeDropRPForTutorial()
    {
        transform.position = Camera.main.transform.position + new Vector3(2.8f, 16, 2);
        vel = new Vector3(0, -2.0f, 0);
    }

    //RP를 주고 게임에서 제거한다.
    void GiveRP()
    {
        DeckManager.instance.audioSource.PlayOneShot(GameManager.instance.ringAttackAudios[7]);
        BattleManager.instance.ChangePlayerRP(Random.Range(3, 6));
        BattleManager.instance.dropRPs.Remove(this);
        GameManager.instance.ReturnDropRPToPool(this);
    }
}
