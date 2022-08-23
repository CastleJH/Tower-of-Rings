using UnityEngine;

public class DamageText : MonoBehaviour
{
    float speed;        //���� �������� �ӵ�
    float lifetime;     //���� �������� �ð�
    TextMesh text;      //�ؽ�Ʈ

    void Awake()
    {
        speed = 1.5f;
        lifetime = 0.8f;
        text = GetComponent<TextMesh>();
    }

    //Ȱ��ȭ�Ǹ鼭 �ٷ� ������Ʈ Ǯ�� �ǵ������� �����Ѵ�.
    void OnEnable()
    {
        Invoke("InvokeRemoveFromBattle", lifetime);
    }

    void Update()
    {
        transform.Translate(new Vector2(0, speed) * Time.deltaTime);    //���� ��¦ ��������.
    }

    //������Ʈ Ǯ�� �ڽ��� �ǵ����ش�.
    void InvokeRemoveFromBattle()
    {
        GameManager.instance.ReturnDamageTextToPool(this);
    }

    //���� ������ �ؽ�Ʈ/���� ��ġ/������ �����Ѵ�.
    public void InitializeDamageText(string dmgText, Vector2 pos, Color32 color)
    {
        text.text = dmgText;
        text.color = color;
        transform.position = pos;
        transform.Translate(new Vector3(0.0f, 0.5f, -9.0f));
    }
}