using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    float speed;
    float lifetime;
    TextMesh text;

    void Awake()
    {
        speed = 1.5f;
        lifetime = 0.8f;
        text = GetComponent<TextMesh>();
    }
    void OnEnable()
    {
        Invoke("InvokeRemoveFromBattle", lifetime);
    }

    void Update()
    {
        transform.Translate(new Vector2(0, speed) * Time.deltaTime);    //위로 살짝 떠오른다.
    }

    //오브젝트 풀에 자신을 되돌려준다.
    void InvokeRemoveFromBattle()
    {
        GameManager.instance.ReturnDamageTextToPool(this);
    }

    //인자 값으로 텍스트/위치/색깔을 수정한다.
    public void InitializeDamageText(string dmgText, Vector2 pos, Color32 color)
    {
        text.text = dmgText;
        text.color = color;
        transform.position = pos;
        transform.Translate(new Vector3(0.0f, 0.5f, -9.0f));
    }
}