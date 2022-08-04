using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GameObject shadow;
    public SpriteRenderer costTypeImage;
    public TextMesh costText;

    public int itemType;
    private Vector3 pos;
    private float animationTime;

    public bool debugFlag;
    public int debugType;
    public int debugCostType;
    public int debugCost;

    public void InitializeItem(int _type, Vector3 _pos, int costType, int cost)
    {
        pos = _pos;
        transform.position = pos + Camera.main.transform.position;
        itemType = _type;
        animationTime = 0.0f;
        if (itemType < 1000) spriteRenderer.sprite = GameManager.instance.itemSprites[itemType];
        else if (itemType < 2000) spriteRenderer.sprite = GameManager.instance.ringSprites[itemType - 1000];
        else if (itemType < 3000) spriteRenderer.sprite = GameManager.instance.relicSprites[itemType - 2000];

        if (costType == 0)
        {
            costTypeImage.gameObject.SetActive(false);
            costText.gameObject.SetActive(false);
        }
        else
        {
            costTypeImage.sprite = GameManager.instance.itemSprites[costType + 3];
            costText.text = cost.ToString();
            costTypeImage.gameObject.SetActive(true);
            costText.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (debugFlag)
        {
            debugFlag = false;
            InitializeItem(debugType, new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 1), debugCostType, debugCost);
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 touchPos;
            if (Input.touchCount > 0) touchPos = Input.touches[0].position;
            else touchPos = Input.mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPos), Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.gameObject == gameObject) GiveThisToPlayer();
        }

        animationTime += Time.deltaTime;
        if (animationTime > 1.0f)
        {
            animationTime = 0.0f;
        }
        else
        {
            float change = Mathf.Abs(animationTime - 0.5f);
            spriteRenderer.transform.localPosition = new Vector3(0, 1.0f - change, 0);
            shadow.transform.localScale = new Vector3(0.125f + change * 0.25f, 0.025f + change * 0.135f, 1.0f);
        }
    }

    void GiveThisToPlayer()
    {
        Debug.Log("To Player!");
        if (itemType < 1000)
        {

        }
        else if (itemType < 2000)
        {
            DeckManager.instance.AddRingToDeck(itemType - 1000);
            gameObject.SetActive(false);
        }
        else if (itemType < 3000)
        {
            GameManager.instance.AddRelicToDeck(itemType - 2000);
            gameObject.SetActive(false);
        }
    }
}
