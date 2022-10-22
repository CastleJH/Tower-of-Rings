using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine.UI;
public class GPGSManager : MonoBehaviour
{
    public static GPGSManager instance;

    private PlayGamesClientConfiguration clientConfiguration;

    public bool useGPGS;

    void Awake()
    {
        instance = this;
        if (useGPGS)
        {
            clientConfiguration = new PlayGamesClientConfiguration.Builder().EnableSavedGames().Build();
            SignIn();
        }
    }

    //로그인(저장된 게임을 불러온다)
    public void SignIn()
    {
        PlayGamesPlatform.InitializeInstance(clientConfiguration);
        PlayGamesPlatform.Activate();

        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (code) =>
        {
            if (code == SignInStatus.Success)
            {
                LoadGame();
                UIManager.instance.gameStartPanelInternetConnectionCheck.SetActive(false);
            }
            else
            {
                UIManager.instance.gameStartPanelInternetConnectionCheck.SetActive(true);
            }
        });
    }

    //로그아웃
    public void SignOut()
    {
        PlayGamesPlatform.Instance.SignOut();
    }

    //게임을 저장한다.
    public void SaveGame()
    {
        if (Social.localUser.authenticated)
        {
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution("SaveDataFile", DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, SaveLocalData);
        }
    }

    private void SaveLocalData(SavedGameRequestStatus status, ISavedGameMetadata meta)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            string tmpData = "";
            for (int i = 0; i < GameManager.instance.spiritEnhanceLevel.Length; i++) tmpData += GameManager.instance.spiritEnhanceLevel[i].ToString() + "|";
            if (tmpData[tmpData.Length - 1] == '|') tmpData = tmpData.Remove(tmpData.Length - 1);
            tmpData += "\n";
            for (int i = 0; i < GameManager.instance.baseRings.Count; i++)
                for (int j = 0; j < 5; j++) tmpData += GameManager.instance.ringCollectionProgress[i, j].ToString() + "|";
            if (tmpData[tmpData.Length - 1] == '|') tmpData = tmpData.Remove(tmpData.Length - 1);
            tmpData += "\n";
            for (int i = 0; i < GameManager.instance.baseRelics.Count; i++)
                for (int j = 0; j < 5; j++) tmpData += GameManager.instance.relicCollectionProgress[i, j].ToString() + "|";
            if (tmpData[tmpData.Length - 1] == '|') tmpData = tmpData.Remove(tmpData.Length - 1);
            tmpData += "\n";
            for (int i = 0; i < GameManager.instance.baseMonsters.Count; i++)
                tmpData += GameManager.instance.monsterCollectionProgress[i].ToString() + "|";
            if (tmpData[tmpData.Length - 1] == '|') tmpData = tmpData.Remove(tmpData.Length - 1);
            tmpData += "\n";
            tmpData += GameManager.instance.diamond.ToString();
            tmpData += "\n";
            tmpData += GameManager.instance.hardModeOpen.ToString();
            tmpData += "\n";
            if (GameManager.instance.saveFloor)
            {
                tmpData += "1\n";
                //층 하드모드여부 HP 골드 덱(아이디|강화) 유물(아이디|저주) 부활
                tmpData += FloorManager.instance.floor.floorNum.ToString() + "\n";
                tmpData += GameManager.instance.isNormalMode == true ? "1\n" : "0\n";
                tmpData += GameManager.instance.playerCurHP.ToString() + "\n";
                tmpData += GameManager.instance.gold.ToString() + "\n";
                for (int i = 0; i < DeckManager.instance.deck.Count; i++) tmpData += DeckManager.instance.deck[i].ToString() + "|" + GameManager.instance.baseRings[DeckManager.instance.deck[i]].level.ToString() + "|";
                tmpData += ".\n";
                for (int i = 0; i < GameManager.instance.relics.Count; i++) {
                    int pure = GameManager.instance.baseRelics[GameManager.instance.relics[i]].isPure == true ? 1 : 0;
                    tmpData += GameManager.instance.relics[i].ToString() + "|" + pure.ToString() + "|";
                }
                tmpData += ".\n";
                tmpData += GameManager.instance.revivable == true ? "1" : "0";
            }
            else tmpData += "0";

            //UIManager.instance.debugText.text += "\n" + tmpData;
            byte[] saveData = System.Text.ASCIIEncoding.ASCII.GetBytes(tmpData);
            SavedGameMetadataUpdate updateForMetadata = new SavedGameMetadataUpdate.Builder().Build();
            ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(meta, updateForMetadata, saveData, SaveCallBack);
        }
    }

    private void SaveCallBack(SavedGameRequestStatus status, ISavedGameMetadata savedData)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            Debug.Log("Successfully saved");
        }
        else Debug.LogError("Failed saving");
    }

    //게임을 불러온다.
    private void LoadGame()
    {
        if (Social.localUser.authenticated)
        {
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution("SaveDataFile", DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, LoadSavedData);
        }
    }

    private void LoadSavedData(SavedGameRequestStatus status, ISavedGameMetadata meta)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(meta, LoadCallBack);
        }
        else
        {
            UIManager.instance.gameStartPanelSignInButton.gameObject.SetActive(false);
            UIManager.instance.gameStartPanelMoveToGameButton.SetActive(false);
            UIManager.instance.gameStartPanelMoveToLobbyButton.SetActive(false);
            GameManager.instance.InitializeUserData();
        }
    }

    private void LoadCallBack(SavedGameRequestStatus status, byte[] loadedData)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            GameManager.instance.InitializeUserData();
            
            string tmpData = System.Text.ASCIIEncoding.ASCII.GetString(loadedData);
            string[] parseByCategory = tmpData.Split('\n');

            if (parseByCategory.Length >= 7)
            {
                string[] parseById = parseByCategory[0].Split('|');
                for (int i = 0; i < GameManager.instance.spiritEnhanceLevel.Length; i++) GameManager.instance.spiritEnhanceLevel[i] = int.Parse(parseById[i]);

                parseById = parseByCategory[1].Split('|');
                for (int i = 0; i < GameManager.instance.baseRings.Count; i++)
                    for (int j = 0; j < 5; j++)
                        GameManager.instance.ringCollectionProgress[i, j] = int.Parse(parseById[i * 5 + j]);

                parseById = parseByCategory[2].Split('|');
                for (int i = 0; i < GameManager.instance.baseRelics.Count; i++)
                    for (int j = 0; j < 5; j++)
                        GameManager.instance.relicCollectionProgress[i, j] = int.Parse(parseById[i * 5 + j]);

                parseById = parseByCategory[3].Split('|');
                for (int i = 0; i < GameManager.instance.baseMonsters.Count; i++) GameManager.instance.monsterCollectionProgress[i] = int.Parse(parseById[i]);

                GameManager.instance.diamond = int.Parse(parseByCategory[4]);
                GameManager.instance.hardModeOpen = int.Parse(parseByCategory[5]);
                if (parseByCategory.Length > 6 && int.Parse(parseByCategory[6]) == 1) //탑에 입장한 상태였다면
                {
                    FloorManager.instance.floor.floorNum = int.Parse(parseByCategory[7]);
                    GameManager.instance.isNormalMode = parseByCategory[8] == "1" ? true : false;
                    GameManager.instance.ResetBases(GameManager.instance.isNormalMode);
                    GameManager.instance.playerMaxHP = 100 + GameManager.instance.spiritEnhanceLevel[2] * 4;
                    if (!GameManager.instance.isNormalMode) GameManager.instance.playerMaxHP /= 2;
                    GameManager.instance.playerCurHP = int.Parse(parseByCategory[9]);
                    GameManager.instance.gold = int.Parse(parseByCategory[10]);
                    DeckManager.instance.RemoveRingFromDeck(0);
                    DeckManager.instance.RemoveRingFromDeck(0);
                    DeckManager.instance.RemoveRingFromDeck(0);
                    DeckManager.instance.RemoveRingFromDeck(0);
                    DeckManager.instance.RemoveRingFromDeck(0);
                    DeckManager.instance.RemoveRingFromDeck(0);
                    parseById = parseByCategory[11].Split('|');
                    for (int i = 0; i < parseById.Length; i += 2)
                    {
                        if (parseById[i] == ".") break;
                        int id = int.Parse(parseById[i]);
                        int lvl = int.Parse(parseById[i + 1]);
                        DeckManager.instance.AddRingToDeck(id, false);
                        while (GameManager.instance.baseRings[id].level < lvl) GameManager.instance.baseRings[id].Upgrade(2.0f);
                    }
                    parseById = parseByCategory[12].Split('|');
                    for (int i = 0; i < parseById.Length; i += 2)
                    {
                        if (parseById[i] == ".") break;
                        int id = int.Parse(parseById[i]);
                        bool isPure = parseById[i + 1] == "1" ? true : false;
                        GameManager.instance.AddRelicToPlayer(id, isPure, false);
                    }
                    GameManager.instance.revivable = parseByCategory[13] == "1" ? true : false;

                    //층 하드모드여부 HP 골드 덱(아이디|강화) 유물(아이디|저주) 
                    UIManager.instance.gameStartPanelSignInButton.gameObject.SetActive(false);
                    UIManager.instance.gameStartPanelMoveToLobbyButton.SetActive(false);
                    UIManager.instance.gameStartPanelMoveToGameButton.SetActive(true);
                }
                else
                {
                    UIManager.instance.gameStartPanelSignInButton.gameObject.SetActive(false);
                    UIManager.instance.gameStartPanelMoveToGameButton.SetActive(false);
                    UIManager.instance.gameStartPanelMoveToLobbyButton.SetActive(true);
                }
            }
            else
            {
                UIManager.instance.gameStartPanelSignInButton.gameObject.SetActive(false);
                UIManager.instance.gameStartPanelMoveToLobbyButton.SetActive(true);
                UIManager.instance.gameStartPanelMoveToGameButton.SetActive(false);
                GameManager.instance.InitializeUserData();
                SaveGame();
            }
        }
        else
        {
            UIManager.instance.gameStartPanelSignInButton.gameObject.SetActive(true);
            UIManager.instance.gameStartPanelMoveToLobbyButton.SetActive(true);
            UIManager.instance.gameStartPanelMoveToGameButton.SetActive(false);
            GameManager.instance.InitializeUserData();
            SaveGame();
        }
    }
}
