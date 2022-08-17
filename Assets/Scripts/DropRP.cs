using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropRP : MonoBehaviour
{
    int click;
    int rpLayerMask;
    public Vector3 vel;

    public void Awake()
    {
        rpLayerMask = 1 << LayerMask.NameToLayer("RP");
    }
    public void InitializeDropRP()
    {
        click = 0;
        transform.position = Camera.main.transform.position + new Vector3(Random.Range(-5, 5), 16, 2);
        if (GameManager.instance.baseRelics[18].have)
        {
            if (GameManager.instance.baseRelics[18].isPure) vel = new Vector3(0, -1.5f, 0);
            else vel = new Vector3(0, -3.0f, 0);
        }
        else vel = new Vector3(0, -2.0f, 0);
    }

    void Update()
    {
        if (Time.timeScale == 0) return;
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 touchPos;
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f, rpLayerMask);
            if (hit.collider != null && hit.collider.gameObject == gameObject) UpdateClick();
        }

        transform.Translate(vel * Time.unscaledDeltaTime);
        if (transform.position.y < Camera.main.transform.position.y - 16.0f)
        {
            BattleManager.instance.dropRPs.Remove(this);
            GameManager.instance.ReturnDropRPToPool(this);
        }
    }

    public void UpdateClick()
    {
        if (++click > 0)
        {
            BattleManager.instance.ChangePlayerRP(Random.Range(3, 6));
            BattleManager.instance.dropRPs.Remove(this);
            GameManager.instance.ReturnDropRPToPool(this);
        }
    }
}
