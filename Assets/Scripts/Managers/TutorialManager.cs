using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Configuration;
using Unity.Profiling;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    public GameObject tutorialPanel;
    public RectTransform textBox;
    public Button textBoxButton;
    public TextMeshProUGUI tutorialText;
    public Image highlightBox;
    public GameObject[] highlightArrow;
    public RectTransform[] highlightHider;

    public bool showHelp;
    public bool isTutorial;
    public int step;

    void Awake()
    {
        instance = this;
        step = 0;

        tutorialPanel.SetActive(false);
        isTutorial = false;
    }

    void Update()
    {
        if (showHelp)
        {
            Invoke("Step" + step.ToString(), 0.0f);
            showHelp = false;
        }
    }

    public void PlayNextTutorialStep()
    {
        ++step;
        showHelp = true;
        Debug.Log(step);
    }

    void MoveHighlightBox(float posX, float posY, float width, float height)
    {
        highlightBox.rectTransform.anchoredPosition = new Vector2(posX, posY);
        highlightBox.rectTransform.sizeDelta = new Vector2(width, height);

        highlightHider[0].sizeDelta = new Vector2(4000, height);

        highlightHider[1].anchoredPosition = new Vector2(4000 + width, 0);
        highlightHider[1].sizeDelta = new Vector2(4000, height);

        highlightHider[2].anchoredPosition = new Vector2(0, 1500 + height / 2);
        highlightHider[2].sizeDelta = new Vector2(8000 + width, 3000);

        highlightHider[3].anchoredPosition = new Vector2(0, -(1500 + height / 2));
        highlightHider[3].sizeDelta = new Vector2(8000 + width, 3000);
    }

    void Step1()
    {
        isTutorial = true;
        Time.timeScale = 0;
        tutorialText.text = "튜토리얼을 시작합니다.";
        textBox.gameObject.SetActive(true);

        MoveHighlightBox(0, 0, 0, 0);

        for (int i = 0; i < highlightArrow.Length; i++) highlightArrow[i].SetActive(false);

        tutorialPanel.SetActive(true);
    }

    void Step2()
    {
        Time.timeScale = 1;
        textBox.anchoredPosition = new Vector2(0, 535);
        tutorialText.text = "포탈을 통해서 다음 방으로 이동할 수 있습니다.\n포탈을 눌러 이동하세요.";
        textBoxButton.gameObject.SetActive(false);
        MoveHighlightBox(0, 860, 180, 300);
    }

    void Step3()
    {
        Time.timeScale = 0;
        tutorialText.text = "전투방에 진입했습니다. 3번의 웨이브에 걸쳐 적이 등장합니다.";
        textBoxButton.gameObject.SetActive(true);

        MoveHighlightBox(0, 0, 0, 0);
    }

    void Step4()
    {
        textBox.anchoredPosition = new Vector2(0, 867);
        tutorialText.text = 
            "적은 빨간 길을 따라 이동하며,\n" +
            "적이 길 끝의 하트에 도달하면 플레이어는 HP를 잃습니다.\n" +
            "HP가 0이 되면 게임 오버이니 주의하세요.";
        MoveHighlightBox(8, 70, 830, 1540);
    }

    void Step5()
    {
        textBox.anchoredPosition = new Vector2(0, -650);
        tutorialText.text = 
            "플레이어는 RP를 소모하여 링을 전장에 소환할 수 있습니다.\n" +
            "각 링마다 필요한 RP 값이 다릅니다.";
        MoveHighlightBox(392, -912, 294, 128);
    }

    void Step6()
    {
        tutorialText.text =
            "이 링은 생산 링입니다.\n" +
            "이 링을 전장에 소환하는 RP 비용은 20입니다.\n" +
            "생산 링은 일정 시간마다 RP를 생산합니다. ";
        MoveHighlightBox(-313, -1023, 140, 150);
        highlightBox.raycastTarget = true;
    }

    void Step7()
    {
        textBox.anchoredPosition = new Vector2(0, 733);
        tutorialText.text =
            "드래그하여 전장에 배치하세요.\n" +
            "빨간 길과 구덩이에는 배치할 수 없습니다.";
        MoveHighlightBox(-313, -418, 140, 1359);
        highlightBox.raycastTarget = false;
        highlightArrow[0].SetActive(true);
        textBoxButton.gameObject.SetActive(false);
    }

    void Step8()
    {
        textBox.anchoredPosition = new Vector2(0, -650);
        tutorialText.text =
            "이 링은 공격 링입니다.\n" +
            "이 링을 전장에 소환하는 RP 비용은 10입니다.\n" +
            "공격 링은 쿨타임마다 사정거리 안에 있는 적을 자동으로 공격합니다.";
        MoveHighlightBox(-446, -1023, 140, 150);
        highlightArrow[0].SetActive(false);
        highlightBox.raycastTarget = true;
        textBoxButton.gameObject.SetActive(true);
    }

    void Step9()
    {
        textBox.anchoredPosition = new Vector2(0, 733);
        tutorialText.text =
            "드래그하여 적이 지나가는 길 근처에 배치하세요.\n" +
            "마찬가지로 빨간 길과 구덩이에는 배치할 수 없습니다.";
        highlightArrow[1].SetActive(true);
        highlightBox.raycastTarget = false;
        textBoxButton.gameObject.SetActive(false);
    }

    void Step10()
    {
        tutorialPanel.SetActive(false);
    }

    void Step11()
    {
        tutorialText.text = 
            "대부분의 링은 사정거리 안의 다른 링들에게 시너지 효과를 줍니다.\n" +
            "사정거리는 원을 배치할 때 주위에 표시되는 초록색 원입니다.";
        textBoxButton.gameObject.SetActive(true);

        highlightArrow[1].SetActive(false);
        MoveHighlightBox(-115, 67, 589, 441);

        tutorialPanel.SetActive(true);
    }

    void Step12()
    {
        tutorialText.text =
            "시너지 효과는 같은 종류의 링일 수록 강력합니다.\n" +
            "되도록 많은 링을 서로의 사정거리 안에 배치하세요.";
    }

    void Step13()
    {
        textBox.anchoredPosition = new Vector2(0, 733);
        tutorialText.text =
            "드래그하여 같은 링 근처에 배치하세요.";

        highlightArrow[2].SetActive(true);
        MoveHighlightBox(-446, -1023, 140, 150);
        highlightBox.raycastTarget = false;
        textBoxButton.gameObject.SetActive(false);
    }

    void Step14()
    {
        highlightArrow[2].SetActive(false);
        tutorialPanel.SetActive(false);
        Time.timeScale = 1.0f;
        Invoke("PlayNextTutorialStep", 6.0f);
    }

    void Step15()
    {
        Time.timeScale = 0.0f;

        textBox.anchoredPosition = new Vector2(0, 852);
        tutorialText.text =
            "일정 시간마다 RP가 하늘에서 떨어집니다.\n" +
            "타이밍에 맞춰 터치하여 보너스 RP를 획득하세요.";

        MoveHighlightBox(219, 1087, 140, 140);
        highlightBox.raycastTarget = true;
        textBoxButton.gameObject.SetActive(true);
        tutorialPanel.SetActive(true);
    }

    void Step16()
    {
        tutorialPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }

    void Step17()
    {
        Time.timeScale = 0.0f;
        textBox.anchoredPosition = new Vector2(0, 535);
        tutorialText.text = "두번째 적 웨이브가 시작되었습니다.\n" +
            "적 웨이브는 3번까지 있으며, 매 웨이브마다 적의 HP가 증가합니다.\n" +
            "링을 배치하여 더 강력해진 적을 막으세요.";
        MoveHighlightBox(0, 0, 0, 0);
        tutorialPanel.SetActive(true);
    }

    void Step18()
    {
        tutorialPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }
    
    void Step19()
    {
        Time.timeScale = 0.0f;

        tutorialText.text = "세번째 적 웨이브가 시작되었습니다.\n" +
            "세번째 웨이브에서는 특수 능력을 가진 엘리트 적이 등장합니다.\n" +
            "링을 배치하여 엘리트 적을 막으세요.";
        tutorialPanel.SetActive(true);
    }

    void Step20()
    {
        tutorialPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }

    void Step21()
    {
        Time.timeScale = 0.0f;

        tutorialText.text = "모든 적이 사라지면 전투가 끝나고 일정 확률로 보상을 받을 수 있습니다.";
        tutorialPanel.SetActive(true);
    }

    void Step22()
    {
        Time.timeScale = 1.0f;
        tutorialText.text =
            "골드가 드랍되었습니다.\n" +
            "골드는 탑 내에 있는 상점에서 아이템을 살때 사용합니다.\n" +
            "터치하여 획득하세요.";
        textBoxButton.gameObject.SetActive(false);
        MoveHighlightBox(0, 43, 600, 430);
        highlightBox.raycastTarget = false;
    }

    void Step23()
    {

    }

    void Step24()
    {
        tutorialText.text =
            "포탈을 눌러서 다음 방으로 이동하세요.";
        MoveHighlightBox(388, 75, 180, 300);
    }
}
