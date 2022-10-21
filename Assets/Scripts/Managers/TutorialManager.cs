using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        tutorialText.text = 
            "포탈을 통해서 다음 방으로 이동할 수 있습니다.\n" +
            "포탈을 눌러 이동하세요.";
        textBoxButton.gameObject.SetActive(false);
        MoveHighlightBox(0, 860, 180, 300);
    }

    void Step3()
    {
        Time.timeScale = 0;
        tutorialText.text = 
            "전투방에 진입했습니다.\n" +
            "3번의 웨이브에 걸쳐 적이 등장합니다.";
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
            "공격 링은 쿨타임마다 사정거리 안에 있는 적을\n" +
            "자동으로 공격합니다.";
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
            "링은 사정거리 안의 다른 링들에게 시너지 효과를 줍니다.\n" +
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
            "매 웨이브마다 적의 HP가 증가합니다.\n" +
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
            "특수 능력을 가진 엘리트 적이 등장합니다.\n" +
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

        tutorialText.text = "모든 적이 사라지면 전투가 끝나고\n" +
            "일정 확률로 보상을 받을 수 있습니다.";
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

    void Step25()
    {
        tutorialText.text =
            "새로운 링을 발견했습니다.\n" +
            "탑을 올라가며 여러 링을 발견할 수 있습니다.\n" +
            "링을 터치하여 정보를 보세요.";
        MoveHighlightBox(0, 52, 280, 280);
    }

    void Step26()
    {
        textBox.anchoredPosition = new Vector2(0, 781);
        tutorialText.text =
            "각 링은 저마다 고유한 능력을 가지고 있습니다.";
        textBoxButton.gameObject.SetActive(true);
        highlightBox.raycastTarget = true;
        MoveHighlightBox(0, 170, 720, 850);
    }

    void Step27()
    {
        tutorialText.text =
            "버튼을 클릭하여 링을 가져가세요.";
        textBoxButton.gameObject.SetActive(false);
        highlightBox.raycastTarget = false;
        MoveHighlightBox(0, -308, 358, 109);
    }

    void Step28()
    {
        tutorialText.text =
            "링은 동시에 최대 6개만 가질 수 있습니다.\n" +
            "한번 가져간 링은 탑에서 발견할 수 있는\n" +
            "[파괴 망치]를 사용해야만 제거할 수 있습니다.\n" +
            "신중하게 링을 가져가세요.";
        textBoxButton.gameObject.SetActive(true);
        MoveHighlightBox(0, 0, 0, 0);
    }

    void Step29()
    {
        tutorialText.text =
            "이 버튼을 누르면 현재 플레이어가 가지고 있는\n" +
            "링들의 정보를 볼 수 있습니다.\n" +
            "진행 상황이 궁금할 때 누르면 도움이 될 수 있습니다.";
        MoveHighlightBox(-441, 1073, 115, 115);
        textBoxButton.gameObject.SetActive(true);
        highlightBox.raycastTarget = true;
    }

    void Step30()
    {
        tutorialText.text =
            "포탈을 눌러서 이전 방으로 돌아가세요.";
        textBoxButton.gameObject.SetActive(false);
        MoveHighlightBox(-388, 75, 180, 300);
    }

    void Step31()
    {
        textBox.anchoredPosition = new Vector2(0, 535);
        tutorialText.text =
            "포탈을 눌러서 다음 방으로 이동하세요.";
        MoveHighlightBox(0, 860, 180, 300);
    }

    void Step32()
    {
        Time.timeScale = 0;
        tutorialText.text =
            "전투방에 진입했습니다. 3번의 웨이브를 막으세요.";
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
            "일정 확률로 골드 대신 링이 드랍될 수 있습니다.\n" +
            "터치하여 정보를 보세요.";
        MoveHighlightBox(0, 52, 280, 280);
        textBoxButton.gameObject.SetActive(false);
        tutorialPanel.SetActive(true);
    }

    void Step35()
    {
        textBox.anchoredPosition = new Vector2(0, 791);
        tutorialText.text =
            "가져가거나 창을 닫고 넘어가세요.\n" +
            "링을 지금 가져가지 않더라도 다시 이 방에 오면\n" +
            "가져갈 수 있습니다.";
        MoveHighlightBox(0, 123, 730, 970);
        highlightBox.raycastTarget = false;
    }

    void Step36()
    {
        tutorialText.text =
            "포탈을 눌러서 다음 방으로 이동하세요.";
        textBoxButton.gameObject.SetActive(false);
        MoveHighlightBox(-388, 75, 180, 300);
    }

    void Step37()
    {
        textBox.anchoredPosition = new Vector2(0, 401);
        tutorialText.text =
            "[제련 망치]를 발견했습니다.\n" +
            "터치하여 사용하세요.";
        MoveHighlightBox(0, 52, 280, 280);
    }

    void Step38()
    {
        textBox.anchoredPosition = new Vector2(0, 590);
        tutorialText.text =
            "제련한 링은 공격력이 증가하고 공격 쿨타임이 감소합니다.\n" +
            "링에 따라 최대 5레벨 까지 제련할 수 있습니다.\n" +
            "제련할 링을 선택하세요.";
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
            "포탈을 눌러서 이전 방으로 돌아가세요.";
        MoveHighlightBox(388, 75, 180, 300);
    }

    void Step41()
    {
        textBox.anchoredPosition = new Vector2(0, 535);
        tutorialText.text =
            "검은 색 포탈은 보스방입니다.\n" +
            "포탈을 눌러 보스방으로 이동하세요.";
        MoveHighlightBox(0, 860, 180, 300);
    }

    void Step42()
    {
        Time.timeScale = 0.0f;
        tutorialText.text =
            "보스방에 진입했습니다.\n" +
            "보스방은 마지막 웨이브에서 강력한 보스가 등장합니다.\n" +
            "보스방을 클리어하세요.";
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
            "보스방을 '클리어'하면 다이아몬드가 드랍됩니다.\n" +
            "다이아몬드는 로비에서 영혼을 강화할 때 사용합니다.";
        MoveHighlightBox(-155, 457, 280, 280);
        textBoxButton.gameObject.SetActive(true);
        tutorialPanel.SetActive(true);
    }

    void Step45()
    {
        tutorialText.text =
            "터치하여 획득하세요.";
        textBoxButton.gameObject.SetActive(false);
    }

    void Step46()
    {
        tutorialText.text =
            "보스방에서 보스를 '처치'하면 유물이 드랍됩니다.\n" +
            "(튜토리얼에서는 처치 여부에 상관 없이 드랍하였습니다)\n" +
            "터치하여 유물의 정보를 확인하세요.";
        highlightBox.raycastTarget = true;
        MoveHighlightBox(165, 295, 280, 280);
    }

    void Step47()
    {
        textBox.anchoredPosition = new Vector2(0, 649);
        textBoxButton.gameObject.SetActive(true);
        tutorialText.text =
            "유물은 게임에 도움이 되는 효과를 가지고 있습니다.\n" +
            "탑을 오르며 여러 유물을 발견할 수 있습니다.";
        MoveHighlightBox(0, 172, 725, 577);
    }
            
    void Step48()
    {
        tutorialText.text =
            "보스 처치 외에도 유물방에서 유물을 획득할 수 있습니다.\n" +
            "다만 유물방에서 획득하는 유물은 일정 확률로 저주받습니다.";
    }

    void Step49()
    {
        tutorialText.text =
            "저주받은 유물은 해로운 효과를 가지지만,\n" +
            "상점에서 구매하는 [성수]로 저주를 풀 수 있습니다.";
    }

    void Step50()
    {
        tutorialText.text =
            "[성수]는 보유한 저주받은 유물 중 하나의 저주를\n" +
            "랜덤하게 해제합니다.\n" +
            "저주가 풀린 유물은 다시 이로운 효과를 줍니다.";
    }

    void Step51()
    {
        highlightBox.raycastTarget = false;
        textBoxButton.gameObject.SetActive(false);
        tutorialText.text =
            "버튼을 눌러 유물을 획득하세요.";
        MoveHighlightBox(0, -161, 357, 107);
    }

    void Step52()
    {
        textBoxButton.gameObject.SetActive(true);
        tutorialText.text =
            "보스방을 클리어하면 중앙에 다음층으로 이동하는\n" +
            "포탈이 생성됩니다.";
        MoveHighlightBox(0, 75, 180, 300);
    }

    void Step53()
    {
        tutorialText.text =
            "7층의 보스방을 클리어 하면\n" +
            "엔딩을 볼 수 있는 포탈이 생성됩니다.";
    }

    void Step54()
    {
        tutorialText.text =
            "만일 7층 보스방을 클리어하기 전에 HP가 0이 된다면," +
            "탑 밖으로 추방되고\n" +
            "그동안 탑에서 모은 모든 링과 유물이 사라집니다.";
    }

    void Step55()
    {
        textBoxButton.gameObject.SetActive(false);
        tutorialText.text =
            "지금은 튜토리얼 중이므로 다음층으로 이동하지 않고\n" +
            "로비로 돌아가겠습니다.\n" +
            "포탈을 터치하여 게임오버하세요.";
    }

    void Step56()
    {
        tutorialPanel.SetActive(false);
    }
    
    void Step57()
    {
        Invoke("PlayNextTutorialStep", 0.8f);
    }

    void Step58()
    {
        textBox.anchoredPosition = new Vector2(0, -775);
        tutorialText.text =
            "영혼강화 창에서는 다이아몬드를 사용하여\n" +
            "영혼 강화를 할 수 있습니다.\n" +
            "영혼강화의 효과는 영구적이며, 모든 게임에 적용됩니다.";
        MoveHighlightBox(-338, -1031, 337, 163);
        textBoxButton.gameObject.SetActive(true);
        highlightBox.raycastTarget = true;
        tutorialPanel.SetActive(true);
    }
    
    void Step59()
    {
        textBox.anchoredPosition = new Vector2(0, -472);
        tutorialText.text =
            "탑의 지식 창에서는 탑에서 발견한\n" +
            "링, 유물 몬스터에 대한 정보를 볼 수 있습니다.\n" +
            "또한, 관련된 임무를 완료하고 다이아몬드를 획득할 수 있습니다.";
        MoveHighlightBox(366, -899, 283, 500);
    }

    void Step60()
    {
        textBox.anchoredPosition = new Vector2(0, 295);
        tutorialText.text =
            "이상으로 튜토리얼을 종료하겠습니다.\n" +
            "7층의 [타락한 왕]을 무찌르고\n" +
            "탑의 꼭대기에 오르시길 바랍니다.";
    }

    void Step61()
    {
        tutorialPanel.SetActive(false);
        isTutorial = false;
    }
}
