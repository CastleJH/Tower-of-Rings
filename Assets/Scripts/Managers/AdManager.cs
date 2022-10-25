using UnityEngine;
using System;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    public GameManager manager;

    RewardedAd diamondAd;
    RewardedAd floorFinishAd;
    RewardedAd startBoostAd;

    char lastCalledAd;
    string diamondAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    string floorFinishedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    string startBoostAdUnitId = "ca-app-pub-3940256099942544/5224354917";

    void Start()
    {
        MobileAds.Initialize(status => { });
        diamondAd = CreateAndLoadRewardedAd(diamondAdUnitId);
        floorFinishAd = CreateAndLoadRewardedAd(floorFinishedAdUnitId);
        startBoostAd = CreateAndLoadRewardedAd(startBoostAdUnitId);
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {

    }

    public void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {

    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        if (lastCalledAd == 'd') diamondAd = CreateAndLoadRewardedAd(diamondAdUnitId);
        else if (lastCalledAd == 'f') floorFinishAd = CreateAndLoadRewardedAd(startBoostAdUnitId);
        else startBoostAd = CreateAndLoadRewardedAd(floorFinishedAdUnitId);
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        if (type == "Diamond")
        {
            GameManager.instance.ChangeDiamond(5);
            GPGSManager.instance.SaveGame();
        }
        else if (type == "StartBoost")
        {
            GameManager.instance.boostLeft = 3;   
        }
    }

    public RewardedAd CreateAndLoadRewardedAd(string adUnitId)
    {
        RewardedAd newAd = new RewardedAd(adUnitId);

        newAd.OnAdLoaded += HandleRewardedAdLoaded;
        newAd.OnUserEarnedReward += HandleUserEarnedReward;
        newAd.OnAdClosed += HandleRewardedAdClosed;

        AdRequest request = new AdRequest.Builder().Build();
        newAd.LoadAd(request);
        return newAd;
    }

    public void ShowDiamondAd()
    {
        if (diamondAd.IsLoaded())
        {
            lastCalledAd = 'd';
            diamondAd.Show();
        }
    }
    public void ShowRingRelicAd()
    {
        if (startBoostAd.IsLoaded())
        {
            lastCalledAd = 's';
            startBoostAd.Show();
        }
    }

    public void ShowFloorFinishAd()
    {
        if (floorFinishAd.IsLoaded())
        {
            lastCalledAd = 'f';
            floorFinishAd.Show();
        }
    }
}