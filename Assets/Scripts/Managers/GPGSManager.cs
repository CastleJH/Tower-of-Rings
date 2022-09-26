using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;
using System.Net;

public class GPGSManager : MonoBehaviour
{
    public static GPGSManager instance;

    private PlayGamesClientConfiguration clientConfiguration;


    void Awake()
    {
        instance = this; 
        clientConfiguration = new PlayGamesClientConfiguration.Builder().EnableSavedGames().Build();
        SignIn();
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
                Debug.LogError("STEP 1***********************************************************");
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
            tmpData += "\n";
            for (int i = 0; i < GameManager.instance.baseRings.Count; i++)
                for (int j = 0; j < 5; j++) tmpData += GameManager.instance.ringCollectionProgress[i, j].ToString() + "|";
            tmpData += "\n";
            for (int i = 0; i < GameManager.instance.baseRelics.Count; i++)
                for (int j = 0; j < 5; j++) tmpData += GameManager.instance.relicCollectionProgress[i, j].ToString() + "|";
            tmpData += "\n";
            for (int i = 0; i < GameManager.instance.baseMonsters.Count; i++)
                tmpData += GameManager.instance.monsterCollectionProgress[i].ToString() + "|";
            tmpData += "\n";
            tmpData += GameManager.instance.diamond.ToString();
            tmpData += "\n";
            tmpData += GameManager.instance.hardModeOpen.ToString();
            Debug.Log(tmpData);
            byte[] saveData = System.Text.ASCIIEncoding.ASCII.GetBytes(tmpData);
            SavedGameMetadataUpdate updateForMetadata = new SavedGameMetadataUpdate.Builder().Build();
            ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(meta, updateForMetadata, saveData, SaveCallBack);
        }
        else
            Debug.LogError("STEP 2***********************************************************");
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

            UIManager.instance.gameStartPanelSignInButton.SetActive(true);
            UIManager.instance.gameStartPanelMoveToLobbyButton.SetActive(false);
            Debug.LogError("STEP 3***********************************************************");
            GameManager.instance.InitializeUserData();
        }
    }

    private void LoadCallBack(SavedGameRequestStatus status, byte[] loadedData)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            GameManager.instance.InitializeUserData();
            if (loadedData.Length > 10)
            {
                string tmpData = System.Text.ASCIIEncoding.ASCII.GetString(loadedData);
                Debug.Log(tmpData);
                string[] parseByCategory = tmpData.Split('\n');

                string[] parseById = parseByCategory[0].Split('|');
                for (int i = 0; i < GameManager.instance.spiritEnhanceLevel.Length; i++) GameManager.instance.spiritEnhanceLevel[i] = int.Parse(parseById[i]);

                parseById = parseByCategory[1].Split('|');
                for (int i = 0; i < GameManager.instance.baseRings.Count; i++)
                    for (int j = 0; j < 5; j++)
                    {
                        GameManager.instance.ringCollectionProgress[i, j] = int.Parse(parseById[i * 5 + j]);
                        Debug.Log(i * 5 + j + " : " + parseById[i * 5 + j]);
                    }

                parseById = parseByCategory[2].Split('|');
                for (int i = 0; i < GameManager.instance.baseRelics.Count; i++)
                    for (int j = 0; j < 5; j++)
                        GameManager.instance.relicCollectionProgress[i, j] = int.Parse(parseById[i * 5 + j]);

                parseById = parseByCategory[3].Split('|');
                for (int i = 0; i < GameManager.instance.baseMonsters.Count; i++) GameManager.instance.monsterCollectionProgress[i] = int.Parse(parseById[i]);

                GameManager.instance.diamond = int.Parse(parseByCategory[4]);
                GameManager.instance.hardModeOpen = int.Parse(parseByCategory[5]);
            }

            UIManager.instance.gameStartPanelSignInButton.SetActive(false);
            UIManager.instance.gameStartPanelMoveToLobbyButton.SetActive(true);
        }
        else
        {

            UIManager.instance.gameStartPanelSignInButton.SetActive(true);
            UIManager.instance.gameStartPanelMoveToLobbyButton.SetActive(false);
            Debug.LogError("STEP 4***********************************************************");
            GameManager.instance.InitializeUserData();
        }
    }
}
