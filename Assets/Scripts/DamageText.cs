using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    float speed;
    float lifetime;
    TextMesh text;
    public Color color;

    void Awake()
    {
        speed = 1f;
        lifetime = 0.5f;
        text = GetComponent<TextMesh>();
    }
    void OnEnable()
    {
        Invoke("DestroyText", lifetime);
    }

    void Update()
    {
        transform.Translate(new Vector2(0, speed) * Time.deltaTime);
    }

    //오브젝트 풀에 자신을 되돌려준다.
    void DestroyText()
    {
        GameManager.instance.ReturnDamageTextToPool(this);
    }

    //인자 값으로 텍스트/위치를 수정한다.
    public void InitializeDamageText(string dmgText, Vector2 pos)
    {
        text.text = dmgText;
        transform.position = pos;
        transform.Translate(Vector3.back * 9);
    }
}