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

    //������Ʈ Ǯ�� �ڽ��� �ǵ����ش�.
    void DestroyText()
    {
        GameManager.instance.ReturnDamageTextToPool(this);
    }

    //���� ������ �ؽ�Ʈ/��ġ/������ �����Ѵ�.
    public void InitializeDamageText(string dmgText, Vector2 pos, Color32 color)
    {
        text.text = dmgText;
        text.color = color;
        transform.position = pos;
        transform.Translate(Vector3.back * 9);
    }
}