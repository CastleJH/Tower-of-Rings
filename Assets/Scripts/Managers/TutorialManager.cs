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

    void Step25()
    {
        tutorialText.text =
            "���ο� ���� �߰��߽��ϴ�.\n" +
            "ž�� �ö󰡸� ���� ���� �߰��� �� �ֽ��ϴ�.\n" +
            "���� ��ġ�Ͽ� ������ ������.";
        MoveHighlightBox(0, 52, 280, 280);
    }

    void Step26()
    {
        textBox.anchoredPosition = new Vector2(0, 781);
        tutorialText.text =
            "�� ���� ������ ������ �ɷ��� ������ �ֽ��ϴ�.";
        textBoxButton.gameObject.SetActive(true);
        highlightBox.raycastTarget = true;
        MoveHighlightBox(0, 170, 720, 850);
    }

    void Step27()
    {
        tutorialText.text =
            "��ư�� Ŭ���Ͽ� ���� ����������.";
        textBoxButton.gameObject.SetActive(false);
        highlightBox.raycastTarget = false;
        MoveHighlightBox(0, -308, 358, 109);
    }

    void Step28()
    {
        tutorialText.text =
            "���� ���ÿ� �ִ� 6���� ���� �� �ֽ��ϴ�.\n" +
            "�ѹ� ������ ���� ž���� �߰��� �� �ִ� [�ı� ��ġ]�� ����ؾ߸� ������ �� �ֽ��ϴ�.\n" +
            "�����ϰ� ���� ����������.";
        textBoxButton.gameObject.SetActive(true);
        MoveHighlightBox(0, 0, 0, 0);
    }

    void Step29()
    {
        tutorialText.text =
            "�� ��ư�� ������ ���� �÷��̾ ������ �ִ� ������ ������ �� �� �ֽ��ϴ�.\n" +
            "���� ��Ȳ�� �ñ��� �� ������ ������ �� �� �ֽ��ϴ�.";
        MoveHighlightBox(-441, 1073, 115, 115);
        textBoxButton.gameObject.SetActive(true);
        highlightBox.raycastTarget = true;
    }

    void Step30()
    {
        tutorialText.text =
            "��Ż�� ������ ���� ������ ���ư�����.";
        textBoxButton.gameObject.SetActive(false);
        MoveHighlightBox(-388, 75, 180, 300);
    }

    void Step31()
    {
        textBox.anchoredPosition = new Vector2(0, 535);
        tutorialText.text =
            "��Ż�� ������ ���� ������ �̵��ϼ���.";
        MoveHighlightBox(0, 860, 180, 300);
    }

    void Step32()
    {
        Time.timeScale = 0;
        tutorialText.text =
            "�����濡 �����߽��ϴ�. 3���� ���̺긦 ��������.";
        textBoxButton.gameObject.SetActive(true);
        MoveHighlightBox(0, 0, 0, 0);
    }
    void Step33()
    {
        tutorialPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }
    void Step34()
    {
        tutorialText.text = 
            "���� Ȯ���� ��� ��� ���� ����� �� �ֽ��ϴ�.\n" +
            "��ġ�Ͽ� ������ ������.";
        MoveHighlightBox(0, 52, 280, 280);
        textBoxButton.gameObject.SetActive(false);
        tutorialPanel.SetActive(true);
    }

    void Step35()
    {
        textBox.anchoredPosition = new Vector2(0, 791);
        tutorialText.text =
            "�������ų� â�� �ݰ� �Ѿ����.\n" +
            "���� ���� �������� �ʴ��� �ٽ� �� �濡 ���� ������ �� �ֽ��ϴ�.";
        MoveHighlightBox(0, 123, 730, 970);
        highlightBox.raycastTarget = false;
    }

    void Step36()
    {
        tutorialText.text =
            "��Ż�� ������ ���� ������ �̵��ϼ���.";
        textBoxButton.gameObject.SetActive(false);
        MoveHighlightBox(-388, 75, 180, 300);
    }

    void Step37()
    {
        textBox.anchoredPosition = new Vector2(0, 401);
        tutorialText.text =
            "[���� ��ġ]�� �߰��߽��ϴ�.\n" +
            "��ġ�Ͽ� ����ϼ���.";
        MoveHighlightBox(0, 52, 280, 280);
    }

    void Step38()
    {
        textBox.anchoredPosition = new Vector2(0, 590);
        tutorialText.text =
            "������ ���� ���ݷ��� �����ϰ� ���� ��Ÿ���� �����մϴ�.\n" +
            "���� ���� �ִ� 5���� ���� ������ �� �ֽ��ϴ�.\n" +
            "������ ���� �����ϼ���.";
        MoveHighlightBox(0, 71, 962, 81);
    }

    void Step39()
    {
        tutorialPanel.SetActive(false);
        Invoke("PlayNextTutorialStep", 1.3f);
    }

    void Step40()
    {
        tutorialPanel.SetActive(true);
        tutorialText.text =
            "��Ż�� ������ ���� ������ ���ư�����.";
        MoveHighlightBox(388, 75, 180, 300);
    }

    void Step41()
    {
        textBox.anchoredPosition = new Vector2(0, 535);
        tutorialText.text =
            "���� �� ��Ż�� �������Դϴ�.\n" +
            "��Ż�� ���� ���������� �̵��ϼ���.";
        MoveHighlightBox(0, 860, 180, 300);
    }

    void Step42()
    {
        Time.timeScale = 0.0f;
        tutorialText.text =
            "�����濡 �����߽��ϴ�.\n" +
            "�����濡���� ������ 3��° ���̺꿡�� ������ ������ �����մϴ�.\n" +
            "�������� Ŭ�����ϼ���.";
        textBoxButton.gameObject.SetActive(true);
        MoveHighlightBox(0, 0, 0, 0);
    }

    void Step43()
    {
        tutorialPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }

    void Step44()
    {
        textBox.anchoredPosition = new Vector2(0, 794);
        tutorialText.text =
            "�������� 'Ŭ����'�ϸ� ���̾Ƹ�尡 ����˴ϴ�.\n" +
            "���̾Ƹ��� �κ񿡼� ��ȥ�� ��ȭ�� �� ����մϴ�.";
        MoveHighlightBox(-155, 457, 280, 280);
        textBoxButton.gameObject.SetActive(true);
        tutorialPanel.SetActive(true);
    }

    void Step45()
    {
        tutorialText.text =
            "��ġ�Ͽ� ȹ���ϼ���.";
        textBoxButton.gameObject.SetActive(false);
    }

    void Step46()
    {
        tutorialText.text =
            "�����濡�� ������ 'óġ'�ϸ� ������ ����˴ϴ�.\n" +
            "(Ʃ�丮�󿡼��� óġ ���ο� ��� ���� ����Ͽ����ϴ�)\n" +
            "��ġ�Ͽ� ������ ������ Ȯ���ϼ���.";
        highlightBox.raycastTarget = true;
        MoveHighlightBox(165, 295, 280, 280);
    }

    void Step47()
    {
        textBox.anchoredPosition = new Vector2(0, 649);
        textBoxButton.gameObject.SetActive(true);
        tutorialText.text =
            "������ ���ӿ� ������ �Ǵ� ȿ���� ������ �ֽ��ϴ�.\n" +
            "ž�� ������ ���� ������ �߰��� �� �ֽ��ϴ�.";
        MoveHighlightBox(0, 172, 725, 577);
    }
            
    void Step48()
    {
        tutorialText.text =
            "���� óġ �ܿ��� �����濡�� ������ ȹ���� �� �ֽ��ϴ�.\n" +
            "�ٸ� �����濡�� ȹ���ϴ� ������ ���� Ȯ���� ���ֹ޽��ϴ�.";
    }

    void Step49()
    {
        tutorialText.text =
            "���ֹ��� ������ �طο� ȿ���� ��������,\n" +
            "�������� �����ϴ� [����]�� ���ָ� Ǯ �� �ֽ��ϴ�.";
    }

    void Step50()
    {
        tutorialText.text =
            "[����]�� ������ ���ֹ��� ���� �� �ϳ��� ���ָ� �����ϰ� Ǳ�ϴ�.\n" +
            "���ְ� Ǯ�� ������ �ٽ� �̷ο� ȿ���� �ݴϴ�.";
    }

    void Step51()
    {
        highlightBox.raycastTarget = false;
        textBoxButton.gameObject.SetActive(false);
        tutorialText.text =
            "��ư�� ���� ������ ȹ���ϼ���.";
        MoveHighlightBox(0, -161, 357, 107);
    }

    void Step52()
    {
        textBoxButton.gameObject.SetActive(true);
        tutorialText.text =
            "�������� Ŭ�����ϸ� �߾ӿ� ���������� �̵��ϴ� ��Ż�� �����˴ϴ�.";
        MoveHighlightBox(0, 75, 180, 300);
    }

    void Step53()
    {
        tutorialText.text =
            "7���� �������� Ŭ���� �ϸ� ������ �� �� �ִ� ��Ż�� �����˴ϴ�.";
    }

    void Step54()
    {
        tutorialText.text =
            "���� 7���� �������� ���ϰ� HP�� 0�� �ȴٸ� ž ������ �߹�ǰ�,\n" +
            "�׵��� ž���� ���� ��� ���� ������ ������ϴ�.";
    }

    void Step55()
    {
        textBoxButton.gameObject.SetActive(false);
        tutorialText.text =
            "������ Ʃ�丮�� ���̹Ƿ� ���������� �̵����� �ʰ� �κ�� ���ư��ڽ��ϴ�.\n" +
            "��Ż�� ��ġ�Ͽ� ���ӿ����ϼ���.";
    }

    void Step56()
    {
        tutorialPanel.SetActive(false);
    }
    
    void Step57()
    {
        textBox.anchoredPosition = new Vector2(0, -775);
        tutorialText.text =
            "��ȥ��ȭ â������ ���̾Ƹ�带 ����Ͽ� ��ȥ ��ȭ�� �� �� �ֽ��ϴ�.\n" +
            "��ȥ��ȭ�� ȿ���� �������̸�, ��� ���ӿ� ����˴ϴ�.";
        MoveHighlightBox(-338, -1031, 337, 163);
        textBoxButton.gameObject.SetActive(true);
        highlightBox.raycastTarget = true;
        tutorialPanel.SetActive(true);
    }
    
    void Step58()
    {
        textBox.anchoredPosition = new Vector2(0, -472);
        tutorialText.text =
            "ž�� ���� â������ ž���� �߰��� ��, ���� ���Ϳ� ���� ������ �� �� �ֽ��ϴ�.\n" +
            "����, ���õ� �ӹ��� �Ϸ��ϰ� ���̾Ƹ�带 ȹ���� �� �ֽ��ϴ�.";
        MoveHighlightBox(366, -899, 283, 500);
    }

    void Step59()
    {
        textBox.anchoredPosition = new Vector2(0, 295);
        tutorialText.text =
            "�̻����� Ʃ�丮���� �����ϰڽ��ϴ�.\n" +
            "7���� [Ÿ���� ��]�� ����� ž�� ����⿡ �����ñ� �ٶ��ϴ�.";
    }

    void Step60()
    {
        tutorialPanel.SetActive(false);
        isTutorial = false;
    }
}
