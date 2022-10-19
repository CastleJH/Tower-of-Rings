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
        tutorialText.text = "Ʃ�丮���� �����մϴ�.";
        textBox.gameObject.SetActive(true);

        MoveHighlightBox(0, 0, 0, 0);

        for (int i = 0; i < highlightArrow.Length; i++) highlightArrow[i].SetActive(false);

        tutorialPanel.SetActive(true);
    }

    void Step2()
    {
        Time.timeScale = 1;
        textBox.anchoredPosition = new Vector2(0, 535);
        tutorialText.text = "��Ż�� ���ؼ� ���� ������ �̵��� �� �ֽ��ϴ�.\n��Ż�� ���� �̵��ϼ���.";
        textBoxButton.gameObject.SetActive(false);
        MoveHighlightBox(0, 860, 180, 300);
    }

    void Step3()
    {
        Time.timeScale = 0;
        tutorialText.text = "�����濡 �����߽��ϴ�. 3���� ���̺꿡 ���� ���� �����մϴ�.";
        textBoxButton.gameObject.SetActive(true);

        MoveHighlightBox(0, 0, 0, 0);
    }

    void Step4()
    {
        textBox.anchoredPosition = new Vector2(0, 867);
        tutorialText.text = 
            "���� ���� ���� ���� �̵��ϸ�,\n" +
            "���� �� ���� ��Ʈ�� �����ϸ� �÷��̾�� HP�� �ҽ��ϴ�.\n" +
            "HP�� 0�� �Ǹ� ���� �����̴� �����ϼ���.";
        MoveHighlightBox(8, 70, 830, 1540);
    }

    void Step5()
    {
        textBox.anchoredPosition = new Vector2(0, -650);
        tutorialText.text = 
            "�÷��̾�� RP�� �Ҹ��Ͽ� ���� ���忡 ��ȯ�� �� �ֽ��ϴ�.\n" +
            "�� ������ �ʿ��� RP ���� �ٸ��ϴ�.";
        MoveHighlightBox(392, -912, 294, 128);
    }

    void Step6()
    {
        tutorialText.text =
            "�� ���� ���� ���Դϴ�.\n" +
            "�� ���� ���忡 ��ȯ�ϴ� RP ����� 20�Դϴ�.\n" +
            "���� ���� ���� �ð����� RP�� �����մϴ�. ";
        MoveHighlightBox(-313, -1023, 140, 150);
        highlightBox.raycastTarget = true;
    }

    void Step7()
    {
        textBox.anchoredPosition = new Vector2(0, 733);
        tutorialText.text =
            "�巡���Ͽ� ���忡 ��ġ�ϼ���.\n" +
            "���� ��� �����̿��� ��ġ�� �� �����ϴ�.";
        MoveHighlightBox(-313, -418, 140, 1359);
        highlightBox.raycastTarget = false;
        highlightArrow[0].SetActive(true);
        textBoxButton.gameObject.SetActive(false);
    }

    void Step8()
    {
        textBox.anchoredPosition = new Vector2(0, -650);
        tutorialText.text =
            "�� ���� ���� ���Դϴ�.\n" +
            "�� ���� ���忡 ��ȯ�ϴ� RP ����� 10�Դϴ�.\n" +
            "���� ���� ��Ÿ�Ӹ��� �����Ÿ� �ȿ� �ִ� ���� �ڵ����� �����մϴ�.";
        MoveHighlightBox(-446, -1023, 140, 150);
        highlightArrow[0].SetActive(false);
        highlightBox.raycastTarget = true;
        textBoxButton.gameObject.SetActive(true);
    }

    void Step9()
    {
        textBox.anchoredPosition = new Vector2(0, 733);
        tutorialText.text =
            "�巡���Ͽ� ���� �������� �� ��ó�� ��ġ�ϼ���.\n" +
            "���������� ���� ��� �����̿��� ��ġ�� �� �����ϴ�.";
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
            "��κ��� ���� �����Ÿ� ���� �ٸ� ���鿡�� �ó��� ȿ���� �ݴϴ�.\n" +
            "�����Ÿ��� ���� ��ġ�� �� ������ ǥ�õǴ� �ʷϻ� ���Դϴ�.";
        textBoxButton.gameObject.SetActive(true);

        highlightArrow[1].SetActive(false);
        MoveHighlightBox(-115, 67, 589, 441);

        tutorialPanel.SetActive(true);
    }

    void Step12()
    {
        tutorialText.text =
            "�ó��� ȿ���� ���� ������ ���� ���� �����մϴ�.\n" +
            "�ǵ��� ���� ���� ������ �����Ÿ� �ȿ� ��ġ�ϼ���.";
    }

    void Step13()
    {
        textBox.anchoredPosition = new Vector2(0, 733);
        tutorialText.text =
            "�巡���Ͽ� ���� �� ��ó�� ��ġ�ϼ���.";

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
            "���� �ð����� RP�� �ϴÿ��� �������ϴ�.\n" +
            "Ÿ�ֿ̹� ���� ��ġ�Ͽ� ���ʽ� RP�� ȹ���ϼ���.";

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
        tutorialText.text = "�ι�° �� ���̺갡 ���۵Ǿ����ϴ�.\n" +
            "�� ���̺�� 3������ ������, �� ���̺긶�� ���� HP�� �����մϴ�.\n" +
            "���� ��ġ�Ͽ� �� �������� ���� ��������.";
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

        tutorialText.text = "����° �� ���̺갡 ���۵Ǿ����ϴ�.\n" +
            "����° ���̺꿡���� Ư�� �ɷ��� ���� ����Ʈ ���� �����մϴ�.\n" +
            "���� ��ġ�Ͽ� ����Ʈ ���� ��������.";
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

        tutorialText.text = "��� ���� ������� ������ ������ ���� Ȯ���� ������ ���� �� �ֽ��ϴ�.";
        tutorialPanel.SetActive(true);
    }

    void Step22()
    {
        Time.timeScale = 1.0f;
        tutorialText.text =
            "��尡 ����Ǿ����ϴ�.\n" +
            "���� ž ���� �ִ� �������� �������� �춧 ����մϴ�.\n" +
            "��ġ�Ͽ� ȹ���ϼ���.";
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
            "��Ż�� ������ ���� ������ �̵��ϼ���.";
        MoveHighlightBox(388, 75, 180, 300);
    }
}
