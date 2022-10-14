using UnityEngine;

public class DropRP : MonoBehaviour
{
    int rpLayerMask;    //���̾� ����ũ
    public Vector3 vel; //�ϰ� �ӵ�

    public void Awake()
    {
        rpLayerMask = 1 << LayerMask.NameToLayer("RP");
    }

    void Update()
    {
        if (Time.timeScale == 0) return;    //�Ͻ����� ���̶�� �۵����� �ʴ´�.

        if (Input.GetMouseButtonDown(0))    //��ġ ������ �ڽ��� �ִٸ� RP�� �ش�.
        {
            Vector2 touchPos;
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f, rpLayerMask);
            if (hit.collider != null && hit.collider.gameObject == gameObject) GiveRP();
        }

        //�ϰ��Ѵ�. �ʹ� �Ʒ��� �������� �������.
        transform.Translate(vel * Time.unscaledDeltaTime);
        if (transform.position.y < Camera.main.transform.position.y - 16.0f)
        {
            BattleManager.instance.dropRPs.Remove(this);
            GameManager.instance.ReturnDropRPToPool(this);
        }
    }


    //�ʱ� ��ġ�� ������ ���� �ϰ��ӵ��� ���Ѵ�.
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

    //RP�� �ְ� ���ӿ��� �����Ѵ�.
    void GiveRP()
    {
        DeckManager.instance.audioSource.PlayOneShot(GameManager.instance.ringAttackAudios[7]);
        BattleManager.instance.ChangePlayerRP(Random.Range(3, 6));
        BattleManager.instance.dropRPs.Remove(this);
        GameManager.instance.ReturnDropRPToPool(this);
    }
}
